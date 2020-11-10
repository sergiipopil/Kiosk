using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Common.Search;
using KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskProductSearchByName;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Helpers.Search;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Server.Domain.Settings;
using KioskBrains.Server.EK.Common.Search;
using KioskBrains.Waf.Actions.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Options;

namespace KioskBrains.Server.Domain.Actions.EkKiosk.EkKioskAutocompleteOptions
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class EkKioskAutocompleteOptionsGet : WafActionGet<EkKioskAutocompleteOptionsGetRequest, EkKioskAutocompleteOptionsGetResponse>
    {
        private readonly EkSearchSettings _ekSearchSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EkKioskAutocompleteOptionsGet(
            IOptions<EkSearchSettings> ekSearchSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _ekSearchSettings = ekSearchSettings.Value;
            Assure.ArgumentNotNull(_ekSearchSettings, nameof(_ekSearchSettings));
        }

        public override async Task<EkKioskAutocompleteOptionsGetResponse> ExecuteAsync(EkKioskAutocompleteOptionsGetRequest request)
        {
            if (string.IsNullOrEmpty(request.Term)
                || request.SearchType != EkSearchTypeEnum.Name)
            {
                return new EkKioskAutocompleteOptionsGetResponse()
                    {
                        AutocompleteOptions = new string[0],
                    };
            }

            // cancellation token
            var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

            using (var searchIndexClient = AzureSearchHelper.CreateSearchIndexClient(_ekSearchSettings.ServiceName, _ekSearchSettings.QueryKey))
            {
                searchIndexClient.IndexName = _ekSearchSettings.ProductsIndexName;

                var autocompleteParameters = new AutocompleteParameters()
                    {
                        // todo: request by OneTermWithContext first, only then by OneTerm (if not enough results)
                        AutocompleteMode = AutocompleteMode.OneTerm,
                        UseFuzzyMatching = true,
                        Top = 10,
                        SearchFields = EkKioskProductSearchByNameGet.GetLanguageSpecificTextFields(request.LanguageCode),
                    };

                var autocompleteResult = await searchIndexClient.Documents.AutocompleteAsync(
                    request.Term,
                    SearchConstants.SuggesterName,
                    autocompleteParameters,
                    cancellationToken: cancellationToken);
                var autocompleteOptions = autocompleteResult.Results
                    .Select(x => x.Text)
                    .ToArray();

                return new EkKioskAutocompleteOptionsGetResponse()
                    {
                        AutocompleteOptions = autocompleteOptions,
                    };
            }
        }
    }
}