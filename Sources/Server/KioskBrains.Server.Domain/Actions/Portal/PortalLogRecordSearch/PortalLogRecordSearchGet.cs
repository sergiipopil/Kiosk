using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Server.Domain.Actions.Common.Models;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Helpers.Queries;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalLogRecordSearch
{
    [AuthorizeUser(UserRoleEnum.CustomerAdmin, UserRoleEnum.CustomerSupport, UserRoleEnum.GlobalAdmin, UserRoleEnum.GlobalSupport)]
    public class PortalLogRecordSearchGet : WafActionGet<PortalLogRecordSearchGetRequest, PortalLogRecordSearchGetResponse>
    {
        private readonly CurrentUser _currentUser;
        private readonly KioskBrainsContext _dbContext;

        public PortalLogRecordSearchGet(CurrentUser currentUser, KioskBrainsContext dbContext)
        {
            _currentUser = currentUser;
            _dbContext = dbContext;
        }

        public override async Task<PortalLogRecordSearchGetResponse> ExecuteAsync(PortalLogRecordSearchGetRequest request)
        {
            if (request.Metadata != null)
            {
                if (string.IsNullOrEmpty(request.Metadata.OrderBy))
                {
                    request.Metadata.OrderBy = nameof(PortalLogRecordSearchRecord.LocalTime);
                    request.Metadata.OrderDirection = OrderDirectionEnum.DESC;
                }
            }

            var searchQuery = _dbContext.LogRecords.AsQueryable();
            if (!_currentUser.IsGlobalPortalUser)
            {
                searchQuery = searchQuery
                    .Where(x => x.Kiosk.CustomerId == _currentUser.CustomerId);
            }

            if (request.IsAdvancedSearch)
            {
                // todo: advanced search
            }
            else
            {
                // todo: fast search
            }

            var records = await searchQuery
                .Select(x => new PortalLogRecordSearchRecord()
                    {
                        Id = x.Id,
                        KioskId = x.KioskId,
                        KioskVersion = x.KioskVersion,
                        LocalTime = x.LocalTime,
                        Type = x.Type,
                        Context = x.Context,
                        Message = x.Message,
                        AdditionalDataJson = x.AdditionalDataJson,
                    })
                .ApplyMetadata(request.Metadata)
                .ToArrayAsync();

            foreach (var record in records)
            {
                record.TypeDisplayName = record.Type.ToString();
                record.ContextDisplayName = record.Context.ToString();
            }

            var response = new PortalLogRecordSearchGetResponse
                {
                    Total = searchQuery.Count(),
                    Records = records,
                };

            return response;
        }
    }
}