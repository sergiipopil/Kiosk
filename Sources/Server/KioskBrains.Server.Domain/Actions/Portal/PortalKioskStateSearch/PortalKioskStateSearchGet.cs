using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.KioskState;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Helpers.Queries;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalKioskStateSearch
{
    [AuthorizeUser(UserRoleEnum.CustomerAdmin, UserRoleEnum.CustomerSupport, UserRoleEnum.GlobalAdmin, UserRoleEnum.GlobalSupport)]
    public class PortalKioskStateSearchGet : WafActionGet<PortalKioskStateSearchGetRequest, PortalKioskStateSearchGetResponse>
    {
        private readonly KioskManager _kioskManager;

        public PortalKioskStateSearchGet(
            KioskManager kioskManager
        )
        {
            _kioskManager = kioskManager;
        }

        public override async Task<PortalKioskStateSearchGetResponse> ExecuteAsync(PortalKioskStateSearchGetRequest request)
        {
            var searchQuery = _kioskManager.GetCurrentUserKiosksQuery();

            if (request.IsAdvancedSearch)
            {
                // todo: advanced search
            }
            else
            {
                // todo: fast search
            }

            var records = await searchQuery
                .Select(x => new PortalKioskStateSearchRecord()
                {
                    Id = x.Id,
                    KioskStateId = x.CurrentStateId,
                    AddressLine1 = x.Address.AddressLine1,
                    AddressLine2 = x.Address.AddressLine2,
                    City = x.Address.City,
                    Country = x.Address.Country.Alpha3,
                    AssignedKioskVersion = x.AssignedKioskVersion,
                    KioskStateVersion = x.CurrentState.KioskVersion,
                    LastPingedOnUtc = x.LastPingedOnUtc,
                    ComponentsStatuses = x.CurrentState.ComponentsStatuses != null
                        ? x.CurrentState.ComponentsStatuses
                            .Where(componentState => componentState.StatusCode != ComponentStatusCodeEnum.Ok
                                                     && componentState.StatusCode != ComponentStatusCodeEnum.Warning)
                            .OrderBy(componentState => componentState.ComponentName)
                            .Select(componentState => new ComponentMonitorableState
                            {
                                Id = componentState.Id,
                                ComponentName = componentState.ComponentName,
                                Status = new ComponentStatus
                                {
                                    Code = componentState.StatusCode,
                                    Message = componentState.StatusMessage
                                },
                                SpecificMonitorableStateJson = componentState.SpecificMonitorableStateJson,
                            })
                            .ToList()
                        : new List<ComponentMonitorableState>()
                })
                .ApplyMetadata(request.Metadata)
                .ToArrayAsync();

            var response = new PortalKioskStateSearchGetResponse
            {
                Total = searchQuery.Count(),
                Records = records,
            };

            return response;
        }
    }
}