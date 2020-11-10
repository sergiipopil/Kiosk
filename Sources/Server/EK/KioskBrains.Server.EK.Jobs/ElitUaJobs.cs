using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.ElitUa;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Common.Cache;
using KioskBrains.Server.Common.Collections;
using KioskBrains.Server.Common.Helpers;
using KioskBrains.Server.Common.Log;
using KioskBrains.Server.Common.Notifications;
using KioskBrains.Server.EK.Common.Search;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;

namespace KioskBrains.Server.EK.Jobs
{
    public class ElitUaJobs
    {
        private readonly ILogger<ElitUaJobs> _logger;

        private readonly ElitUaClient _elitUaClient;

        private readonly EkSearchManagementClient _ekSearchManagementClient;

        private readonly IPersistentCache _persistentCache;

        private readonly INotificationManager _notificationManager;

        public ElitUaJobs(
            ILogger<ElitUaJobs> logger,
            ElitUaClient elitUaClient,
            EkSearchManagementClient ekSearchManagementClient,
            IPersistentCache persistentCache,
            INotificationManager notificationManager)
        {
            _logger = logger;
            _elitUaClient = elitUaClient;
            _ekSearchManagementClient = ekSearchManagementClient;
            _persistentCache = persistentCache;
            _notificationManager = notificationManager;
        }

        private const string LastAppliedElitPriceListIdKey = "LastAppliedElitPriceListId";

        public async Task RunPriceListSynchronizationAsync()
        {
            var cancellationToken = CancellationToken.None;
            var statistics = new PriceListSynchronizationStatistics();
            string priceListStatusMessage;

            try
            {
                var lastAppliedPriceListId = await _persistentCache.GetValueAsync(LastAppliedElitPriceListIdKey, cancellationToken);
                var priceList = await _elitUaClient.GetUnappliedPriceListAsync(lastAppliedPriceListId, cancellationToken);
                if (!priceList.IsSuccess)
                {
                    throw new InvalidOperationException(priceList.StatusMessage);
                }

                priceListStatusMessage = priceList.StatusMessage;

                if (priceList.Records?.Count > 0)
                {
                    await _ekSearchManagementClient.CreateProductIndexIfNotExistAsync();

                    using (var searchIndexClient = _ekSearchManagementClient.CreateSearchIndexClient())
                    {
                        searchIndexClient.IndexName = _ekSearchManagementClient.ProductsIndexName;
                        var updateUtcTimestamp = TimestampHelper.GetCurrentUtcTotalMinutes();

                        // max size of Azure Search batch
                        const int PageSize = 1_000;
                        var page = 0;
                        foreach (var priceListPage in priceList.Records.Batch(PageSize))
                        {
                            _logger.LogTrace(LoggingEvents.Synchronization, $"Processing {page*PageSize}+{PageSize}...");

                            await IndexPriceListPageAsync(searchIndexClient, priceListPage.ToArray(), updateUtcTimestamp, statistics);

                            page++;
                        }

                        // wait half of minute until changes are applied
                        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                        await PriceListHelper.CleanExpiredRecordsAsync(
                            searchIndexClient,
                            EkProductSourceEnum.ElitUa,
                            updateUtcTimestamp,
                            statistics,
                            _logger);
                    }
                }

                await _persistentCache.SetValueAsync(LastAppliedElitPriceListIdKey, priceList.PriceListId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.UnhandledException, ex, "Elit price list synchronization failed.");
                await _notificationManager.SendWorkerMessageAsync($"ERROR! Elit price list synchronization failed, {statistics}: {ex.Message}");

                throw;
            }

            await _notificationManager.SendWorkerMessageAsync($"Elit price list synchronization succeed, {statistics}, status: {priceListStatusMessage}");
        }

        private async Task IndexPriceListPageAsync(
            SearchIndexClient searchIndexClient,
            ElitPriceListRecord[] priceListPage,
            long updateUtcTimestamp,
            PriceListSynchronizationStatistics statistics)
        {
            var tryNumber = 1;
            while (true)
            {
                try
                {
                    var indexActions = priceListPage
                        .Select(x => ConvertHelper.ElitUaApiModelToIndexAction(x, updateUtcTimestamp))
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
                    statistics.Received += priceListPage.Length;

                    return;
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