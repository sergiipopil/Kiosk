using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.KioskState;
using KioskBrains.Server.Domain.Actions.Common.Models;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using KioskBrains.Waf.Helpers.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalKioskStateSearch
{
    [AuthorizeUser(UserRoleEnum.CustomerAdmin, UserRoleEnum.CustomerSupport, UserRoleEnum.GlobalAdmin, UserRoleEnum.GlobalSupport)]
    public class PortalKioskStateDetailsGet : WafActionGet<CommonDetailsGetRequest, PortalKioskStateDetailsGetResponse>
    {
        private readonly KioskManager _kioskManager;

        public PortalKioskStateDetailsGet(
            KioskManager kioskManager
        )
        {
            _kioskManager = kioskManager;
        }

        public override async Task<PortalKioskStateDetailsGetResponse> ExecuteAsync(CommonDetailsGetRequest request)
        {
            if (request?.Id == null)
            {
                throw new ArgumentNullException(nameof(request.Id));
            }

            var response = await _kioskManager.GetCurrentUserKiosksQuery()
                .Where(x => x.Id == request.Id.Value)
                .Select(x => new PortalKioskStateDetailsGetResponse
                {
                    Form = new PortalKioskStateDetailsForm
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
                    }
                })
                .FirstOrDefaultAsync();

            if (response == null)
            {
                throw EntityNotFoundException.Create<Entities.Kiosk>(request.Id.Value);
            }

            return response;
        }
    }
}