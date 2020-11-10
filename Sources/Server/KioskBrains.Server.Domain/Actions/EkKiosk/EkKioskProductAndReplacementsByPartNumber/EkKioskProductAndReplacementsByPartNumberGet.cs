using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.TecDocWs;
using KioskBrains.Clients.TecDocWs.Models;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Common.EK.Helpers;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Helpers.Search;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Server.Domain.Settings;
using KioskBrains.Server.EK.Common.Search.Models;
using KioskBrains.Waf.Actions.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Options;
using Microsoft.Rest.Azure;

namespace KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskProductAndReplacementsByPartNumber
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class EkKioskProductAndReplacementsByPartNumberGet : WafActionGet<EkKioskProductAndReplacementsByPartNumberGetRequest, EkKioskProductAndReplacementsByPartNumberGetResponse>
    {
        private readonly EkSearchSettings _ekSearchSettings;
        private readonly TecDocWsClient _tecDocWsClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EkKioskProductAndReplacementsByPartNumberGet(
            IOptions<EkSearchSettings> ekSearchSettings,
            TecDocWsClient tecDocWsClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _tecDocWsClient = tecDocWsClient;
            _httpContextAccessor = httpContextAccessor;
            _ekSearchSettings = ekSearchSettings.Value;
            Assure.ArgumentNotNull(_ekSearchSettings, nameof(_ekSearchSettings));
        }

        public override async Task<EkKioskProductAndReplacementsByPartNumberGetResponse> ExecuteAsync(EkKioskProductAndReplacementsByPartNumberGetRequest request)
        {
            Assure.ArgumentNotNull(request.PartNumberBrand, nameof(request.PartNumberBrand));
            Assure.ArgumentNotNull(request.PartNumberBrand.ProductKey, nameof(request.PartNumberBrand.ProductKey));
            Assure.ArgumentNotNull(request.PartNumberBrand.PartNumber, nameof(request.PartNumberBrand.PartNumber));

            var response = new EkKioskProductAndReplacementsByPartNumberGetResponse();

            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            using (var searchIndexClient = AzureSearchHelper.CreateSearchIndexClient(_ekSearchSettings.ServiceName, _ekSearchSettings.QueryKey))
            {
                searchIndexClient.IndexName = _ekSearchSettings.ProductsIndexName;

                // FIND PRODUCT
                try
                {
                    var indexProduct = await searchIndexClient.Documents.GetAsync<IndexProduct>(
                        request.PartNumberBrand.ProductKey,
                        cancellationToken: cancellationToken);
                    response.Product = EkConvertHelper.EkNewIndexProductToProduct(indexProduct);

                    // todo: add search by Brand/PartNumber to find all direct matches since many products sources are supported
                    // it's not done since new product search model is planned anyway
                }
                catch (CloudException)
                {
                    response.Product = EkConvertHelper.EkOmegaPartNumberBrandToProduct(request.PartNumberBrand);
                }

                // FIND REPLACEMENTS
                var cleanedPartNumber = PartNumberCleaner.GetCleanedPartNumber(request.PartNumberBrand.PartNumber);

                // TecDoc replacements
                // todo: add cancellationToken support to proxy based clients
                var tecDocReplacements = await _tecDocWsClient.SearchByArticleNumberAsync(cleanedPartNumber);
                tecDocReplacements = tecDocReplacements
                    // except direct match
                    .Where(x => x.NumberType != ArticleNumberTypeEnum.ArticleNumber)
                    .ToArray();

                var replacementCleanedBrandPartNumbers = tecDocReplacements
                    .Select(x => PartNumberCleaner.GetCleanedBrandPartNumber(x.BrandName, x.ArticleNo))
                    .Take(100)
                    .ToArray();

                if (replacementCleanedBrandPartNumbers.Length > 0)
                {
                    var replacementsIndexSearchParameters = new SearchParameters()
                        {
                            Top = 100,
                            SearchFields = new[] { "cleanedBrandPartNumber" },
                        };
                    var searchTerm = string.Join("|", replacementCleanedBrandPartNumbers);
                    var searchResult = await searchIndexClient.Documents.SearchAsync<IndexProduct>(
                        searchTerm,
                        replacementsIndexSearchParameters,
                        cancellationToken: cancellationToken);

                    response.Replacements = searchResult.Results
                        .Select(x => EkConvertHelper.EkNewIndexProductToProduct(x.Document))
                        .ToArray();
                }
                else
                {
                    response.Replacements = new EkProduct[0];
                }

                return response;
            }
        }
    }
}