using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.AllegroPl;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using Microsoft.AspNetCore.Http;

namespace KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskAllegroProductDescription
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class EkKioskAllegroProductDescriptionGet : WafActionGet<EkKioskAllegroProductDescriptionGetRequest, EkKioskAllegroProductDescriptionGetResponse>
    {
        private readonly AllegroPlClient _allegroPlClient;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public EkKioskAllegroProductDescriptionGet(
            AllegroPlClient allegroPlClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _allegroPlClient = allegroPlClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<EkKioskAllegroProductDescriptionGetResponse> ExecuteAsync(EkKioskAllegroProductDescriptionGetRequest request)
        {
            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            var description = await _allegroPlClient.GetOfferDescriptionAsync(request.ProductId, cancellationToken);

            return new EkKioskAllegroProductDescriptionGetResponse()
                {
                    Description = description.Description,
                    Parameters = description.Parameters
                };
        }
    }
}