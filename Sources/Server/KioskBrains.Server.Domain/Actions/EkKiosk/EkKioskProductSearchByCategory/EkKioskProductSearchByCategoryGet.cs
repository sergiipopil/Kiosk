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
using KioskBrains.Server.EK.Common.Search;
using KioskBrains.Server.EK.Common.Search.Models;
using KioskBrains.Waf.Actions.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Options;

namespace KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskProductSearchByCategory
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class EkKioskProductSearchByCategoryGet : WafActionGet<EkKioskProductSearchByCategoryGetRequest, EkKioskProductSearchByCategoryGetResponse>
    {
        private readonly EkSearchSettings _ekSearchSettings;
        private readonly TecDocWsClient _tecDocWsClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EkKioskProductSearchByCategoryGet(
            IOptions<EkSearchSettings> ekSearchSettings,
            TecDocWsClient tecDocWsClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _tecDocWsClient = tecDocWsClient;
            _httpContextAccessor = httpContextAccessor;
            _ekSearchSettings = ekSearchSettings.Value;
            Assure.ArgumentNotNull(_ekSearchSettings, nameof(_ekSearchSettings));
        }

        public override async Task<EkKioskProductSearchByCategoryGetResponse> ExecuteAsync(EkKioskProductSearchByCategoryGetRequest request)
        {
            var response = new EkKioskProductSearchByCategoryGetResponse();

            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            // todo: add cancellationToken support to proxy based clients

            // determine TecDoc car type of modification first
            CarTypeEnum carType;
            // request categories for cars first
            var categories = await _tecDocWsClient.GetCategoriesAsync(CarTypeEnum.Car, request.ModificationId, null, childNodes: false);
            if (categories?.Length > 0)
            {
                carType = CarTypeEnum.Car;
            }
            else
            {
                // then request for trucks
                categories = await _tecDocWsClient.GetCategoriesAsync(CarTypeEnum.Truck, request.ModificationId, null, childNodes: false);
                if (categories?.Length > 0)
                {
                    carType = CarTypeEnum.Truck;
                }
                else
                {
                    return response;
                }
            }

            const int MaxProductCount = 200;

            // TecDoc articles
            var tecDocProducts = await _tecDocWsClient.GetArticlesCompactInfoAsync(carType, request.ModificationId, request.CategoryId);
            var productCleanedBrandPartNumbers = tecDocProducts
                .Select(x => PartNumberCleaner.GetCleanedBrandPartNumber(x.BrandName, x.ArticleNo))
                .Take(MaxProductCount)
                .ToArray();
            if (productCleanedBrandPartNumbers.Length == 0)
            {
                return response;
            }

            // FIND PRODUCTS IN STOCK
            using (var searchIndexClient = AzureSearchHelper.CreateSearchIndexClient(_ekSearchSettings.ServiceName, _ekSearchSettings.QueryKey))
            {
                searchIndexClient.IndexName = _ekSearchSettings.ProductsIndexName;

                var replacementsIndexSearchParameters = new SearchParameters()
                    {
                        Top = MaxProductCount,
                        SearchFields = new[] { "cleanedBrandPartNumber" },
                    };
                var searchTerm = string.Join("|", productCleanedBrandPartNumbers);
                var searchResult = await searchIndexClient.Documents.SearchAsync<IndexProduct>(
                    searchTerm,
                    replacementsIndexSearchParameters,
                    cancellationToken: cancellationToken);

                response.Products = searchResult.Results
                    .Select(x => EkConvertHelper.EkNewIndexProductToProduct(x.Document))
                    .ToArray();

                return response;
            }
        }
    }
}