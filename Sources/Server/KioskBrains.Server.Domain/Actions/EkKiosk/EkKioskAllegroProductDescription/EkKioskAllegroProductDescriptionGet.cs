﻿using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.AllegroPl;
using KioskBrains.Clients.AllegroPl.ServiceInterfaces;
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
        private ITranslateService _translateService;

        public EkKioskAllegroProductDescriptionGet(
            AllegroPlClient allegroPlClient,
            IHttpContextAccessor httpContextAccessor, ITranslateService translateService)
        {
            _allegroPlClient = allegroPlClient;
            _httpContextAccessor = httpContextAccessor;
            _translateService = translateService;
        }

        public override async Task<EkKioskAllegroProductDescriptionGetResponse> ExecuteAsync(EkKioskAllegroProductDescriptionGetRequest request)
        {
            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            var description = await _allegroPlClient.GetOfferDescriptionAsync(_translateService, request.ProductId, cancellationToken);

            return new EkKioskAllegroProductDescriptionGetResponse()
            {
                    State = (EkProductStateEnum)(int)description.State,
                    Description = description.Description,
                    Parameters = description.Parameters
            };
        }
    }
}