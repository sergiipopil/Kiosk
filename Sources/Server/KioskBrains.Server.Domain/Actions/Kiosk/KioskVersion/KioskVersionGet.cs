using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.Api;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using KioskBrains.Waf.Helpers.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Kiosk.KioskVersion
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class KioskVersionGet : WafActionGet<EmptyRequest, KioskVersionGetResponse>
    {
        private readonly CurrentUser _currentUser;

        private readonly KioskManager _kioskManager;

        private readonly KioskVersionUpdateManager _kioskVersionUpdateManager;

        public KioskVersionGet(
            CurrentUser currentUser,
            KioskManager kioskManager,
            KioskVersionUpdateManager kioskVersionUpdateManager)
        {
            _currentUser = currentUser;
            _kioskManager = kioskManager;
            _kioskVersionUpdateManager = kioskVersionUpdateManager;
        }

        public override async Task<KioskVersionGetResponse> ExecuteAsync(EmptyRequest request)
        {
            var kioskId = _currentUser.Id;

            var kioskData = await _kioskManager.GetActiveKiosksQuery()
                .Where(x => x.Id == kioskId)
                .Select(x => new
                    {
                        x.ApplicationType,
                        x.AssignedKioskVersion,
                    })
                .FirstOrDefaultAsync();
            if (kioskData == null)
            {
                throw EntityNotFoundException.Create<Entities.Kiosk>(kioskId);
            }

            var response = new KioskVersionGetResponse()
                {
                    AssignedKioskVersion = kioskData.AssignedKioskVersion,
                };
            if (!string.IsNullOrEmpty(response.AssignedKioskVersion))
            {
                response.AssignedKioskVersionUpdateUrl = await _kioskVersionUpdateManager.GetUpdateUrlAsync(
                    kioskData.ApplicationType,
                    response.AssignedKioskVersion);
            }

            return response;
        }
    }
}