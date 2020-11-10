using System;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Waf.Managers.Common;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Managers
{
    public class CentralBankExchangeRateManager : IWafManager
    {
        private readonly KioskBrainsContext _dbContext;

        public CentralBankExchangeRateManager(
            KioskBrainsContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int?> GetCentralBankExchangeRateIdAsync(
            string localCurrencyCode,
            string foreignCurrencyCode)
        {
            var centralBankExchangeRateId = await _dbContext.CentralBankExchangeRates
                .Where(x => x.LocalCurrencyCode == localCurrencyCode
                            && x.ForeignCurrencyCode == foreignCurrencyCode)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
            return centralBankExchangeRateId;
        }

        public async Task<decimal?> GetCurrentRateAsync(
            string localCurrencyCode,
            string foreignCurrencyCode,
            DateTime localTime)
        {
            var centralBankExchangeRateId = await GetCentralBankExchangeRateIdAsync(localCurrencyCode, foreignCurrencyCode);
            if (centralBankExchangeRateId == null)
            {
                return null;
            }

            return await GetCurrentRateAsync(centralBankExchangeRateId.Value, localTime);
        }

        public async Task<decimal?> GetCurrentRateAsync(
            int centralBankExchangeRateId,
            DateTime localTime)
        {
            var currentUpdate = await _dbContext.CentralBankExchangeRateUpdates
                .Where(x => x.CentralBankExchangeRateId == centralBankExchangeRateId && x.StartTime <= localTime)
                .OrderByDescending(x => x.StartTime)
                .FirstOrDefaultAsync();
            return currentUpdate?.Rate;
        }
    }
}