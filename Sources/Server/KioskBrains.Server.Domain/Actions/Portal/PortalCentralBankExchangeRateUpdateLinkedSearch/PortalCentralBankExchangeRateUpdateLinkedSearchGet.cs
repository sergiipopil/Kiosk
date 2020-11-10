using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Helpers.Queries;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateUpdateLinkedSearch
{
    [AuthorizeUser(UserRoleEnum.CustomerAdmin, UserRoleEnum.CustomerSupport, UserRoleEnum.GlobalAdmin, UserRoleEnum.GlobalSupport)]
    public class PortalCentralBankExchangeRateUpdateLinkedSearchGet : WafActionGet<PortalCentralBankExchangeRateUpdateLinkedSearchGetRequest, PortalCentralBankExchangeRateUpdateLinkedSearchGetResponse>
    {
        private readonly CurrentUser _currentUser;
        private readonly KioskBrainsContext _dbContext;

        public PortalCentralBankExchangeRateUpdateLinkedSearchGet(CurrentUser currentUser, KioskBrainsContext dbContext)
        {
            _currentUser = currentUser;
            _dbContext = dbContext;
        }

        public override async Task<PortalCentralBankExchangeRateUpdateLinkedSearchGetResponse> ExecuteAsync(PortalCentralBankExchangeRateUpdateLinkedSearchGetRequest request)
        {
            var searchQuery = _dbContext.CentralBankExchangeRateUpdates
                .Where(x => x.CentralBankExchangeRateId == request.CentralBankExchangeRateId);

            var records = await searchQuery
                .Select(x => new PortalCentralBankExchangeRateUpdateLinkedSearchRecord()
                    {
                        Id = x.Id,
                        StartTime = x.StartTime,
                        Rate = x.Rate
                    })
                .ApplyMetadata(request.Metadata)
                .ToArrayAsync();

            var response = new PortalCentralBankExchangeRateUpdateLinkedSearchGetResponse
                {
                    Total = searchQuery.Count(),
                    Records = records,
                };

            return response;
        }
    }
}