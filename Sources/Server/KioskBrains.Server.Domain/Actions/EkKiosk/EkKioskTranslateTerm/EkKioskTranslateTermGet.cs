using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.AllegroPl;
using KioskBrains.Clients.AllegroPl.ServiceInterfaces;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using Microsoft.AspNetCore.Http;

namespace KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskTranslateTerm
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class EkKioskTranslateTermGet : WafActionGet<EkKioskTranslateTermGetRequest, EkKioskTranslateTermGetResponse>
    {
        private readonly AllegroPlClient _allegroPlClient;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private ITranslateService _translateService;

        public EkKioskTranslateTermGet(
            AllegroPlClient allegroPlClient,
            IHttpContextAccessor httpContextAccessor, ITranslateService translateService)
        {
            _allegroPlClient = allegroPlClient;            
            _httpContextAccessor = httpContextAccessor;
            _translateService = translateService;
        }

        public override async Task<EkKioskTranslateTermGetResponse> ExecuteAsync(EkKioskTranslateTermGetRequest request)
        {
            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            var description = await _allegroPlClient.GetTranslation(_translateService, request.Term, cancellationToken);

            return new EkKioskTranslateTermGetResponse()
            {
                Translation = description
            };
        }
    }
}