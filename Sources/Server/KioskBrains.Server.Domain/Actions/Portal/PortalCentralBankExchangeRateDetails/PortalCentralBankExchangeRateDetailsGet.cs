using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Server.Domain.Actions.Common.Models;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Helpers.Currency;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using KioskBrains.Waf.Helpers.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateDetails
{
    [AuthorizeUser(UserRoleEnum.CustomerAdmin, UserRoleEnum.CustomerSupport, UserRoleEnum.GlobalAdmin, UserRoleEnum.GlobalSupport)]
    public class PortalCentralBankExchangeRateDetailsGet : WafActionGet<CommonDetailsGetRequest, PortalCentralBankExchangeRateDetailsGetResponse>
    {
        private readonly CurrentUser _currentUser;
        private readonly KioskBrainsContext _dbContext;

        public PortalCentralBankExchangeRateDetailsGet(
            CurrentUser currentUser,
            KioskBrainsContext dbContext)
        {
            _currentUser = currentUser;
            _dbContext = dbContext;
        }

        public override async Task<PortalCentralBankExchangeRateDetailsGetResponse> ExecuteAsync(CommonDetailsGetRequest request)
        {
            var entityId = request.Id;
            PortalCentralBankExchangeRateDetailsGetResponse response;
            if (entityId == null)
            {
                response = new PortalCentralBankExchangeRateDetailsGetResponse()
                    {
                        Form = new PortalCentralBankExchangeRateDetailsForm(),
                    };
            }
            else
            {
                response = await _dbContext.CentralBankExchangeRates
                    .Where(x => x.Id == entityId.Value)
                    .Select(x => new PortalCentralBankExchangeRateDetailsGetResponse()
                        {
                            Form = new PortalCentralBankExchangeRateDetailsForm()
                                {
                                    Id = x.Id,
                                    LocalCurrencyCode = x.LocalCurrencyCode,
                                    ForeignCurrencyCode = x.ForeignCurrencyCode,
                                    DefaultOrder = x.DefaultOrder,
                                }
                        })
                    .FirstOrDefaultAsync();

                if (response == null)
                {
                    throw EntityNotFoundException.Create<CentralBankExchangeRate>(entityId.Value);
                }
            }

            response.Currencies = CurrencyHelper.GetCurrencyListOptions();

            return response;
        }
    }
}