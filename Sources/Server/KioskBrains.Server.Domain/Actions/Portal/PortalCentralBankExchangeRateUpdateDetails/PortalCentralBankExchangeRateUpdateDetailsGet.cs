using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Helpers.Dates;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using KioskBrains.Waf.Helpers.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateUpdateDetails
{
    [AuthorizeUser(UserRoleEnum.CustomerAdmin, UserRoleEnum.CustomerSupport, UserRoleEnum.GlobalAdmin, UserRoleEnum.GlobalSupport)]
    public class PortalCentralBankExchangeRateUpdateDetailsGet : WafActionGet<PortalCentralBankExchangeRateUpdateDetailsGetRequest, PortalCentralBankExchangeRateUpdateDetailsGetResponse>
    {
        private readonly CurrentUser _currentUser;
        private readonly KioskBrainsContext _dbContext;

        public PortalCentralBankExchangeRateUpdateDetailsGet(
            CurrentUser currentUser,
            KioskBrainsContext dbContext)
        {
            _currentUser = currentUser;
            _dbContext = dbContext;
        }

        public override async Task<PortalCentralBankExchangeRateUpdateDetailsGetResponse> ExecuteAsync(PortalCentralBankExchangeRateUpdateDetailsGetRequest request)
        {
            var entityId = request.Id;
            PortalCentralBankExchangeRateUpdateDetailsGetResponse response;
            if (entityId == null)
            {
                response = new PortalCentralBankExchangeRateUpdateDetailsGetResponse
                    {
                        Form = new PortalCentralBankExchangeRateUpdateDetailsForm
                            {
                                CentralBankExchangeRateId = request.CentralBankExchangeRateId,
                                // start of tomorrow by default
                                StartDate = TimeZones.GetCustomerNow(_currentUser).Date.AddDays(1),
                                StartTime = "00:00:00",
                            },
                    };
            }
            else
            {
                response = await _dbContext.CentralBankExchangeRateUpdates
                    .Where(x => x.Id == entityId.Value)
                    .Select(x => new PortalCentralBankExchangeRateUpdateDetailsGetResponse
                        {
                            Form = new PortalCentralBankExchangeRateUpdateDetailsForm
                                {
                                    Id = x.Id,
                                    CentralBankExchangeRateId = x.CentralBankExchangeRateId,
                                    StartDate = x.StartTime.Date,
                                    StartTime = x.StartTime.ToString("HH:mm:ss"),
                                    Rate = x.Rate,
                                }
                        })
                    .FirstOrDefaultAsync();

                if (response == null)
                {
                    throw EntityNotFoundException.Create<CentralBankExchangeRate>(entityId.Value);
                }
            }

            return response;
        }
    }
}