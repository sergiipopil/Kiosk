using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.Ek4Car;
using KioskBrains.Common.Transactions;
using KioskBrains.Server.Common.Log;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Entities.EK;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KioskBrains.Server.EK.Integration.Jobs
{
    public class OrderSyncJobs
    {
        private readonly KioskBrainsContext _dbContext;

        private readonly Ek4CarClient _ek4CarClient;

        private readonly ILogger<OrderSyncJobs> _logger;

        public OrderSyncJobs(
            KioskBrainsContext dbContext,
            Ek4CarClient ek4CarClient,
            ILogger<OrderSyncJobs> logger)
        {
            _dbContext = dbContext;
            _ek4CarClient = ek4CarClient;
            _logger = logger;
        }

        /// <summary>
        /// Safe operation.
        /// </summary>
        public async Task TrySendOrderBatchAsync()
        {
            const int MaxOrdersPerTry = 100;

            try
            {
                var utcNow = DateTime.UtcNow;

                var ekTransactions = await _dbContext.EkTransactions
                    .AsNoTracking()
                    .Where(x => x.CompletionStatus == TransactionCompletionStatusEnum.Success
                                && !x.IsSentToEkSystem
                                && (x.NextSendingToEkTimeUtc == null || x.NextSendingToEkTimeUtc <= utcNow))
                    .Take(MaxOrdersPerTry)
                    .ToArrayAsync();

                if (ekTransactions.Length == 0)
                {
                    return;
                }

                foreach (var ekTransaction in ekTransactions)
                {
                    try
                    {
                        var ekOrder = ekTransaction.ToEkOrder();

                        await _ek4CarClient.SendOrderAsync(ekOrder, CancellationToken.None);

                        // success - mark the order as sent

#pragma warning disable EF1000 // Possible SQL injection vulnerability.
                        await _dbContext.Database.ExecuteSqlCommandAsync((RawSqlString)$@"
UPDATE
    {nameof(KioskBrainsContext.EkTransactions)}
SET
    {nameof(EkTransaction.IsSentToEkSystem)} = 1
WHERE
    {nameof(EkTransaction.Id)} = {ekTransaction.Id}
");
#pragma warning restore EF1000 // Possible SQL injection vulnerability.
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(LoggingEvents.UnhandledException, ex, $"EK order sync failed (id: {ekTransaction.Id}).");

                        // in order to avoid spamming with broken orders
                        var nextSyncTimeUtc = ekTransaction.NextSendingToEkTimeUtc == null
                            ? utcNow.AddMinutes(1) // in 1 minute if first try
                            : utcNow.AddMinutes(10); // in 10 minutes until it's fixed

#pragma warning disable EF1000 // Possible SQL injection vulnerability.
                        await _dbContext.Database.ExecuteSqlCommandAsync((RawSqlString)$@"
UPDATE
    {nameof(KioskBrainsContext.EkTransactions)}
SET
    {nameof(EkTransaction.NextSendingToEkTimeUtc)} = '{nextSyncTimeUtc:yyyy-MM-dd HH:mm:ss}'
WHERE
    {nameof(EkTransaction.Id)} = {ekTransaction.Id}
");
#pragma warning restore EF1000 // Possible SQL injection vulnerability.
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.UnhandledException, ex, "Unhandled exception.");
            }
        }
    }
}