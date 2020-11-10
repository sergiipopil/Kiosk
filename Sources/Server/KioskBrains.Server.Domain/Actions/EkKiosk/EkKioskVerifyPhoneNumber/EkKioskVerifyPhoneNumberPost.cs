using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.Ek4Car;
using KioskBrains.Clients.Ek4Car.Models;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using Microsoft.AspNetCore.Http;

namespace KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskVerifyPhoneNumber
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class EkKioskVerifyPhoneNumberPost : WafActionPost<EkKioskVerifyPhoneNumberPostRequest, EkKioskVerifyPhoneNumberPostResponse>
    {
        private readonly Ek4CarClient _ek4CarClient;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public EkKioskVerifyPhoneNumberPost(
            Ek4CarClient ek4CarClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _ek4CarClient = ek4CarClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<EkKioskVerifyPhoneNumberPostResponse> ExecuteAsync(EkKioskVerifyPhoneNumberPostRequest request)
        {
            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            // for test purposed
            if (request.PhoneNumber == "+380000000000")
            {
                throw new Ek4CarRequestException("Wrong phone number.");
            }

            var ekResponse = await _ek4CarClient.VerifyPhoneNumberAsync(
                new VerifyPhoneNumberRequest()
                    {
                        PhoneNumber = request.PhoneNumber,
                    },
                cancellationToken);

            return new EkKioskVerifyPhoneNumberPostResponse()
                {
                    VerificationCode = ekResponse.VerificationCode,
                };
        }
    }
}