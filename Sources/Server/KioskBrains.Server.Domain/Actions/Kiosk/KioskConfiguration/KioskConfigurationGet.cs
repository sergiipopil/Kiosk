using System.Threading.Tasks;
using KioskBrains.Common.Api;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;

namespace KioskBrains.Server.Domain.Actions.Kiosk.KioskConfiguration
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class KioskConfigurationGet : WafActionGet<EmptyRequest, KioskConfigurationGetResponse>
    {
        private readonly CurrentUser _currentUser;

        private readonly KioskManager _kioskManager;

        public KioskConfigurationGet(
            CurrentUser currentUser,
            KioskManager kioskManager)
        {
            _currentUser = currentUser;
            _kioskManager = kioskManager;
        }

        public override async Task<KioskConfigurationGetResponse> ExecuteAsync(EmptyRequest request)
        {
            var kioskConfiguration = await _kioskManager.GetKioskConfigurationAsync(_currentUser.Id);
            return new KioskConfigurationGetResponse()
                {
                    KioskConfiguration = kioskConfiguration,
                };
        }
    }
}