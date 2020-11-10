using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Helpers.Currency;
using KioskBrains.Server.Domain.Helpers.Dates;
using KioskBrains.Server.Domain.Helpers.Queries;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateSearch
{
    [AuthorizeUser(UserRoleEnum.CustomerAdmin, UserRoleEnum.CustomerSupport, UserRoleEnum.GlobalAdmin, UserRoleEnum.GlobalSupport)]
    public class PortalCentralBankExchangeRateSearchGet : WafActionGet<PortalCentralBankExchangeRateSearchGetRequest, PortalCentralBankExchangeRateSearchGetResponse>
    {
        private readonly CurrentUser _currentUser;
        private readonly KioskBrainsContext _dbContext;
        private readonly CentralBankExchangeRateManager _centralBankExchangeRateManager;

        public PortalCentralBankExchangeRateSearchGet(
            CurrentUser currentUser,
            KioskBrainsContext dbContext,
            CentralBankExchangeRateManager centralBankExchangeRateManager
        )
        {
            _currentUser = currentUser;
            _dbContext = dbContext;
            _centralBankExchangeRateManager = centralBankExchangeRateManager;
        }

        public override async Task<PortalCentralBankExchangeRateSearchGetResponse> ExecuteAsync(PortalCentralBankExchangeRateSearchGetRequest request)
        {
            var searchQuery = _dbContext.CentralBankExchangeRates
                .AsQueryable();

            if (request.IsAdvancedSearch)
            {
                searchQuery = searchQuery
                    .Where(x => (string.IsNullOrEmpty(request.SearchStruct.LocalCurrencyCode) || x.LocalCurrencyCode == request.SearchStruct.LocalCurrencyCode)
                                && (string.IsNullOrEmpty(request.SearchStruct.ForeignCurrencyCode) || x.ForeignCurrencyCode == request.SearchStruct.ForeignCurrencyCode));
            }
            else
            {
                searchQuery = searchQuery.Where(x => string.IsNullOrEmpty(request.SearchTerm)
                                                     || x.LocalCurrencyCode.Contains(request.SearchTerm)
                                                     || x.ForeignCurrencyCode.Contains(request.SearchTerm));
            }

            var totalCount = await searchQuery.CountAsync();

            var records = await searchQuery
                .OrderBy(x => x.LocalCurrencyCode)
                .ThenBy(x => x.DefaultOrder)
                .ApplyMetadata(request.Metadata, ignoreOrdering: true)
                .Select(x => new PortalCentralBankExchangeRateSearchRecord()
                {
                    Id = x.Id,
                    LocalCurrencyCode = x.LocalCurrencyCode,
                    ForeignCurrencyCode = x.ForeignCurrencyCode,
                    DefaultOrder = x.DefaultOrder,
                })
                .ToArrayAsync();

            // calculate current rates
            var currentUserTime = TimeZones.GetCustomerNow(_currentUser);
            foreach (var record in records)
            {
                record.Rate = await _centralBankExchangeRateManager.GetCurrentRateAsync(record.Id, currentUserTime) ?? 0;
            }

            return new PortalCentralBankExchangeRateSearchGetResponse
            {
                Total = totalCount,
                Records = records,
                Currencies = CurrencyHelper.GetCurrencyListOptions(),
                IsNewAllowed = _currentUser.Role == UserRoleEnum.GlobalAdmin,
            };
        }
    }
}