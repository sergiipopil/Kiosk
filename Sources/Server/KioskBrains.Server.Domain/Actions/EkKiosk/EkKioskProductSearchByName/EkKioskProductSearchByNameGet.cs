using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Common.Search;
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

namespace KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskProductSearchByName
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class EkKioskProductSearchByNameGet : WafActionGet<EkKioskProductSearchByNameGetRequest, EkKioskProductSearchByNameGetResponse>
    {
        private readonly EkSearchSettings _ekSearchSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EkKioskProductSearchByNameGet(
            IOptions<EkSearchSettings> ekSearchSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _ekSearchSettings = ekSearchSettings.Value;
            Assure.ArgumentNotNull(_ekSearchSettings, nameof(_ekSearchSettings));
        }

        public override async Task<EkKioskProductSearchByNameGetResponse> ExecuteAsync(EkKioskProductSearchByNameGetRequest request)
        {
            if (string.IsNullOrEmpty(request.Term)
                || request.Term.Length < 3)
            {
                return new EkKioskProductSearchByNameGetResponse()
                    {
                        Products = new EkProduct[0],
                    };
            }

            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            using (var searchIndexClient = AzureSearchHelper.CreateSearchIndexClient(_ekSearchSettings.ServiceName, _ekSearchSettings.QueryKey))
            {
                searchIndexClient.IndexName = _ekSearchSettings.ProductsIndexName;

                // paging
                var indexSearchParameters = new SearchParameters()
                    {
                        Skip = request.From,
                        Top = request.Count,
                        IncludeTotalResultCount = request.IncludeTotal,
                        SearchFields = GetLanguageSpecificTextFields(request.LanguageCode),
                        ScoringProfile = SearchConstants.BoostNameScoringProfileName,
                    };

                // sorting
                switch (request.Sorting)
                {
                    case EkProductSearchSortingEnum.PriceAscending:
                        indexSearchParameters.OrderBy = new[] { "price asc" };
                        break;
                    case EkProductSearchSortingEnum.PriceDescending:
                        indexSearchParameters.OrderBy = new[] { "price desc" };
                        break;
                    case EkProductSearchSortingEnum.Default:
                    default:
                        // no sorting
                        break;
                }

                // term
                var term = request.Term;

                var searchResult = await searchIndexClient.Documents.SearchAsync<IndexProduct>(
                    term,
                    indexSearchParameters,
                    cancellationToken: cancellationToken);
                var products = searchResult.Results
                    .Select(x => EkConvertHelper.EkNewIndexProductToProduct(x.Document))
                    .ToArray();
                var total = searchResult.Count ?? 0;

                return new EkKioskProductSearchByNameGetResponse()
                    {
                        Products = products,
                        Total = total,
                    };
            }
        }

        internal static string[] GetLanguageSpecificTextFields(string languageCode)
        {
            if (languageCode != Languages.UkrainianCode
                && languageCode != Languages.RussianCode)
            {
                languageCode = Languages.RussianCode;
            }

            return new[]
                    {
                        "name",
                        // search by description is disabled at the moment since sometimes it returns irrelevant results
                        // TBD: new model of search is required - separate task
                        // "description",
                    }
                .Select(x => $"{x}_{languageCode}")
                .ToArray();
        }
    }
}