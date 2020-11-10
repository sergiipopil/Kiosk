using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Clients.Ek4Car.Models;
using KioskBrains.Common.EK.Transactions;
using KioskBrains.Common.Transactions;
using KioskBrains.Server.Common.Services;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.EK.Integration.Jobs;
using KioskBrains.Waf.Helpers.Exceptions;
using Microsoft.EntityFrameworkCore;
using EkApiKiosk = KioskBrains.Clients.Ek4Car.Models.Kiosk;
using EkApiAddress = KioskBrains.Clients.Ek4Car.Models.Address;

namespace KioskBrains.Server.EK.Integration.Managers
{
    public class EkIntegrationManager : IEkIntegrationManager
    {
        private readonly KioskBrainsContext _dbContext;

        public EkIntegrationManager(
            KioskBrainsContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<EmptyData> ApplyUpdatesAsync(Update[] updates, IIntegrationLogManager integrationLogManager)
        {
            return new EmptyData();
        }

        public async Task<EkApiKiosk> GetKioskAsync(int kioskId)
        {
            var kiosk = await _dbContext.Kiosks
                .Where(x => x.Id == kioskId
                            && x.ApplicationType == KioskApplicationTypeEnum.EK)
                .Select(x => new EkApiKiosk()
                {
                    Id = x.Id,
                    Address = new EkApiAddress()
                    {
                        CountryCode = x.Address.Country.Alpha3,
                        RegionCode = x.Address.State.Code,
                        City = x.Address.City,
                        AddressLine1 = x.Address.AddressLine1,
                        AddressLine2 = x.Address.AddressLine2,
                    }
                })
                .FirstOrDefaultAsync();
            if (kiosk == null)
            {
                throw EntityNotFoundException.Create<EkApiKiosk>(kioskId);
            }

            return kiosk;
        }

        public async Task<Order> GetOrderAsync(int orderId)
        {
            var ekTransaction = await _dbContext.EkTransactions
                .AsNoTracking()
                .Where(x => x.Id == orderId
                            && x.CompletionStatus == TransactionCompletionStatusEnum.Success)
                .FirstOrDefaultAsync();
            if (ekTransaction == null)
            {
                throw EntityNotFoundException.Create<EkTransaction>(orderId);
            }

            return ekTransaction.ToEkOrder();
        }
    }
}