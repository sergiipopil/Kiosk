using System;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Clients.OmegaAutoBiz;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Common.Helpers;
using KioskBrains.Server.Common.Log;
using KioskBrains.Server.Common.Notifications;
using KioskBrains.Server.EK.Common.Search;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;

namespace KioskBrains.Server.EK.Jobs
{
    public class OmegaAutoBizJobs
    {
        private readonly ILogger<OmegaAutoBizJobs> _logger;

        private readonly OmegaAutoBizClient _omegaAutoBizClient;

        private readonly EkSearchManagementClient _ekSearchManagementClient;

        private readonly INotificationManager _notificationManager;

        public OmegaAutoBizJobs(
            ILogger<OmegaAutoBizJobs> logger,
            OmegaAutoBizClient omegaAutoBizClient,
            EkSearchManagementClient ekSearchManagementClient,
            INotificationManager notificationManager)
        {
            _logger = logger;
            _omegaAutoBizClient = omegaAutoBizClient;
            _ekSearchManagementClient = ekSearchManagementClient;
            _notificationManager = notificationManager;
        }

        public async Task RunPriceListSynchronizationAsync()
        {
            var statistics = new PriceListSynchronizationStatistics();
            try
            {
                await _ekSearchManagementClient.CreateProductIndexIfNotExistAsync();

                var from = 0;
                const int count = 1_000;

                using (var searchIndexClient = _ekSearchManagementClient.CreateSearchIndexClient())
                {
                    searchIndexClient.IndexName = _ekSearchManagementClient.ProductsIndexName;
                    var updateUtcTimestamp = TimestampHelper.GetCurrentUtcTotalMinutes();

                    while (true)
                    {
                        var isFinalPage = await RequestAndIndexPriceListPageAsync(searchIndexClient, from, count, updateUtcTimestamp, statistics);
                        if (isFinalPage)
                        {
                            break;
                        }

                        from += count;
                    }

                    // wait half of minute until changes are applied
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    await PriceListHelper.CleanExpiredRecordsAsync(
                        searchIndexClient,
                        EkProductSourceEnum.OmegaAutoBiz,
                        updateUtcTimestamp,
                        statistics,
                        _logger);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.UnhandledException, ex, "Omega price list synchronization failed.");
                await _notificationManager.SendWorkerMessageAsync($"ERROR! Omega price list synchronization failed, {statistics}: {ex.Message}");

                throw;
            }

            await _notificationManager.SendWorkerMessageAsync($"Omega price list synchronization succeed, {statistics}.");
        }

        private async Task<bool> RequestAndIndexPriceListPageAsync(
            SearchIndexClient searchIndexClient,
            int from,
            int count,
            long updateUtcTimestamp,
            PriceListSynchronizationStatistics statistics)
        {
            var tryNumber = 1;
            while (true)
            {
                try
                {
                    _logger.LogTrace(LoggingEvents.Synchronization, $"Processing {from}+{count}...");

                    var omegaResponse = await _omegaAutoBizClient.ProductPriceListAsync(from, count);
                    if (omegaResponse.Result == null
                        || omegaResponse.Result.Length == 0)
                    {
                        return true;
                    }

                    var indexActions = omegaResponse.Result
                        .Select(x => ConvertHelper.OmegaAutoBizApiModelToIndexAction(x, updateUtcTimestamp))
                        .ToArray();
                    var indexBatch = IndexBatch.New(indexActions);
                    try
                    {
                        await searchIndexClient.Documents.IndexAsync(indexBatch);
                    }
                    catch (IndexBatchException ex)
                    {
                        var reasons = string.Join(", ", ex.IndexingResults
                            .Where(x => !x.Succeeded)
                            .Select(x => $"{x.Key}-{x.ErrorMessage}"));
                        var message = $"Failed to index products: {reasons}.";

                        throw new InvalidOperationException(message);
                    }

                    _logger.LogTrace(LoggingEvents.Synchronization, "Success.");
                    statistics.Received += omegaResponse.Result.Length;

                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(LoggingEvents.UnhandledException, ex, $"Try {tryNumber}.");
                    if (tryNumber >= 3)
                    {
                        throw;
                    }

                    tryNumber++;
                }
            }
        }
    }
}