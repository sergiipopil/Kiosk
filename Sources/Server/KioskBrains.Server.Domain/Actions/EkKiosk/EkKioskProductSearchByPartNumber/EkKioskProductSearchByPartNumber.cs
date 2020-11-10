using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.OmegaAutoBiz;
using KioskBrains.Common.Constants;
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

namespace KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskProductSearchByPartNumber
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class EkKioskProductSearchByPartNumberGet : WafActionGet<EkKioskProductSearchByPartNumberGetRequest, EkKioskProductSearchByPartNumberGetResponse>
    {
        private readonly EkSearchSettings _ekSearchSettings;
        private readonly OmegaAutoBizClient _omegaAutoBizClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EkKioskProductSearchByPartNumberGet(
            IOptions<EkSearchSettings> ekSearchSettings,
            OmegaAutoBizClient omegaAutoBizClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _ekSearchSettings = ekSearchSettings.Value;
            Assure.ArgumentNotNull(_ekSearchSettings, nameof(_ekSearchSettings));
            _omegaAutoBizClient = omegaAutoBizClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<EkKioskProductSearchByPartNumberGetResponse> ExecuteAsync(EkKioskProductSearchByPartNumberGetRequest request)
        {
            var cleanedPartNumber = PartNumberCleaner.GetCleanedPartNumber(request.PartNumber);

            if (string.IsNullOrEmpty(cleanedPartNumber))
            {
                return GetEmptyResponse();
            }

            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            var indexedPartNumberBrands = await FindIndexedPartNumberBrandsAsync(cleanedPartNumber, cancellationToken);
            if (indexedPartNumberBrands.Length > 0)
            {
                // return only found product brands
                // TBD: we can attach other brands from Omega for wider results
                return new EkKioskProductSearchByPartNumberGetResponse()
                    {
                        Brands = indexedPartNumberBrands,
                    };
            }

            // if no products are found in index, look for brands in Omega to allow buying of at least replacements
            // todo: add cancellationToken support to proxy based clients
            var response = await _omegaAutoBizClient.ProductSearchAsync(cleanedPartNumber, 0, 50);
            var partNumberBrands = response.Result?
                .Select(x => new EkPartNumberBrand()
                    {
                        ProductKey = new EkProductKey(EkProductSourceEnum.OmegaAutoBiz, x.ProductId.ToString()).ToKey(),
                        BrandName = x.BrandDescription?.Trim(),
                        PartNumber = x.Number?.Trim(),
                        Name = new MultiLanguageString()
                            {
                                [Languages.RussianCode] = x.Description?.Trim(),
                            },
                    })
                .ToArray();

            return new EkKioskProductSearchByPartNumberGetResponse()
                {
                    Brands = partNumberBrands ?? new EkPartNumberBrand[0],
                };
        }

        private async Task<EkPartNumberBrand[]> FindIndexedPartNumberBrandsAsync(
            string cleanedPartNumber,
            CancellationToken cancellationToken)
        {
            using (var searchIndexClient = AzureSearchHelper.CreateSearchIndexClient(_ekSearchSettings.ServiceName, _ekSearchSettings.QueryKey))
            {
                searchIndexClient.IndexName = _ekSearchSettings.ProductsIndexName;
                var searchParameters = new SearchParameters()
                    {
                        Top = 100,
                        SearchFields = new[] { "cleanedPartNumber" },
                    };
                var searchResult = await searchIndexClient.Documents.SearchAsync<IndexProduct>(
                    cleanedPartNumber,
                    searchParameters,
                    cancellationToken: cancellationToken);

                var matchingProducts = searchResult.Results
                    .Select(x => x.Document)
                    .Select(x => new
                            {
                                PartNumberBrand = new EkPartNumberBrand()
                                    {
                                        ProductKey = x.Key,
                                        BrandName = x.BrandName,
                                        PartNumber = x.PartNumber,
                                        Name = new MultiLanguageString()
                                            {
                                                [Languages.RussianCode] = x.Name_ru,
                                            },
                                    },
                                BrandKey = GetBrandKey(x.BrandName),
                                Price = (decimal)(x.Price ?? 0),
                            }
                    )
                    .Where(x => !string.IsNullOrEmpty(x.BrandKey)
                                && x.Price > 0)
                    .ToArray();

                // return the cheapest product for each brand
                return matchingProducts
                    .GroupBy(x => x.BrandKey)
                    .Select(x => x
                        .OrderBy(p => p.Price)
                        .Select(p => p.PartNumberBrand)
                        .First())
                    .ToArray();
            }
        }

        private static string GetBrandKey(string brandName)
        {
            return PartNumberCleaner.GetCleanedBrandName(brandName)?.ToLower();
        }

        private EkKioskProductSearchByPartNumberGetResponse GetEmptyResponse()
        {
            return new EkKioskProductSearchByPartNumberGetResponse()
                {
                    Brands = new EkPartNumberBrand[0],
                };
        }
    }
}