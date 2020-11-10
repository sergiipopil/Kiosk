using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Server.Common.Cache;
using KioskBrains.Server.Common.Log;
using KioskBrains.Server.Common.Notifications;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Helpers.Dates;
using KioskBrains.Server.Domain.Managers.Integration.Rates.UaCentralBank;
using KioskBrains.Waf.Managers.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KioskBrains.Server.Domain.Managers
{
    public class ExchangeRateAutoUpdateManager : IWafManager
    {
        private readonly KioskBrainsContext _dbContext;
        private readonly UaCentralBankApiProxy _uaCentralBankApiProxy;
        private readonly INotificationManager _notificationManager;
        private readonly IPersistentCache _persistentCache;
        private readonly ILogger<ExchangeRateAutoUpdateManager> _logger;

        public ExchangeRateAutoUpdateManager(
            KioskBrainsContext dbContext,
            UaCentralBankApiProxy uaCentralBankApiProxy,
            INotificationManager notificationManager,
            IPersistentCache persistentCache,
            SystemCustomerProvider systemCustomerProvider,
            ILogger<ExchangeRateAutoUpdateManager> logger)
        {
            _dbContext = dbContext;
            _uaCentralBankApiProxy = uaCentralBankApiProxy;
            _notificationManager = notificationManager;
            _persistentCache = persistentCache;
            _logger = logger;
        }

        private const string LastAppliedNbuRatesDateKey = "LastAppliedNbuRatesDate";

        public async Task AutoUpdateRatesAsync(CancellationToken cancellationToken)
        {
            try
            {
                // NBU
                string nbuRatesStatusMessage;
                var nbuRates = await _uaCentralBankApiProxy.GetRatesAsync();
                if (nbuRates.Count > 0)
                {
                    var lastAppliedNbuRatesDateString = await _persistentCache.GetValueAsync(LastAppliedNbuRatesDateKey, cancellationToken);

                    // at the moment NBU returns rates only for one date
                    var nbuRateDate = nbuRates[0].Date;
                    var nbuRateDateString = nbuRateDate.ToString("yyyy-MM-dd");
                    if (lastAppliedNbuRatesDateString == nbuRateDateString)
                    {
                        nbuRatesStatusMessage = $"INFO: NBU rates are already updated to {nbuRateDateString}.";
                    }
                    else
                    {
                        // tomorrow 00:00
                        var uahRateUpdateStartTime = TimeZones.GetTimeZoneNow(TimeZones.UkrainianTime).Date.AddDays(1);
                        var uahRates = await _dbContext.CentralBankExchangeRates
                            .Where(x => x.LocalCurrencyCode == "UAH")
                            .Select(x => new
                                {
                                    x.Id,
                                    x.LocalCurrencyCode,
                                    x.ForeignCurrencyCode,
                                })
                            .ToArrayAsync(cancellationToken);
                        var updatedCurrencies = new List<string>();
                        foreach (var uahRate in uahRates)
                        {
                            var nbuRate = nbuRates
                                .Where(x => x.LocalCurrencyCode == uahRate.LocalCurrencyCode
                                            && x.ForeignCurrencyCode == uahRate.ForeignCurrencyCode)
                                .FirstOrDefault();
                            if (nbuRate != null)
                            {
                                _dbContext.CentralBankExchangeRateUpdates.Add(
                                    new CentralBankExchangeRateUpdate()
                                        {
                                            CentralBankExchangeRateId = uahRate.Id,
                                            Rate = nbuRate.Rate,
                                            StartTime = uahRateUpdateStartTime,
                                        });
                                updatedCurrencies.Add(nbuRate.ForeignCurrencyCode);
                            }
                        }

                        await _dbContext.SaveChangesAsync(cancellationToken);

                        await _persistentCache.SetValueAsync(LastAppliedNbuRatesDateKey, nbuRateDateString, CancellationToken.None);

                        nbuRatesStatusMessage = $"SUCCESS: NBU rates for {string.Join(", ", updatedCurrencies)} were updated to {nbuRateDateString}.";
                    }
                }
                else
                {
                    nbuRatesStatusMessage = "WARNING: No NBU rates.";
                }

                await _notificationManager.SendWorkerMessageAsync(nbuRatesStatusMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.UnhandledException, ex, $"{nameof(AutoUpdateRatesAsync)} failed.");

                await _notificationManager.SendWorkerMessageAsync($"ERROR: {nameof(AutoUpdateRatesAsync)} failed: {ex.Message}.");
            }
        }
    }
}