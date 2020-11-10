using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using KioskBrains.Common.Api;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Kiosk.KioskLogin
{
    public class KioskLoginPost : WafActionPost<KioskLoginPostRequest, KioskLoginPostResponse>
    {
        public override bool AllowAnonymous => true;

        private readonly KioskManager _kioskManager;

        public KioskLoginPost(KioskManager kioskManager)
        {
            _kioskManager = kioskManager;
        }

        public override async Task<KioskLoginPostResponse> ExecuteAsync(KioskLoginPostRequest request)
        {
            var kioskInfo = await _kioskManager
                .GetActiveKiosksQuery()
                .Include(x => x.Customer)
                .Where(x => x.SerialKey == request.SerialKey)
                .Select(x => new
                    {
                        x.Id,
                        x.CustomerId,
                        x.Customer.TimeZoneName
                    })
                .FirstOrDefaultAsync();
            if (kioskInfo == null)
            {
                throw new AuthenticationException("Invalid serial key.");
            }

            var currentUser = CurrentUser.BuildKioskAppUser(
                kioskId: kioskInfo.Id,
                customerId: kioskInfo.CustomerId,
                timeZoneName: kioskInfo.TimeZoneName);
            var token = new TokenGenerator().Generate(currentUser);

            return new KioskLoginPostResponse()
            {
                Token = token,
            };
        }
    }
}