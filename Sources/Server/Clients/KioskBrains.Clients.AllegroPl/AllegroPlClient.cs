using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.AllegroPl.Models;
using KioskBrains.Clients.AllegroPl.Rest;
using KioskBrains.Clients.AllegroPl.ServiceInterfaces;
using KioskBrains.Clients.YandexTranslate;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Common.Log;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoapService;

namespace KioskBrains.Clients.AllegroPl
{
    /// <summary>
    /// Keep in mind that it's a singleton.
    /// </summary>
    public class AllegroPlClient
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly AllegroPlClientSettings _settings;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        private readonly YandexTranslateClient _yandexTranslateClient;

        private readonly ILogger<AllegroPlClient> _logger;

        private readonly RestClient _restClient;

        private object _transLock = new object();
        private ISet<String> _valuesToTranslate;

        public AllegroPlClient(
            IOptions<AllegroPlClientSettings> settings,
            YandexTranslateClient yandexTranslateClient,
            ILogger<AllegroPlClient> logger)
        {
            _settings = settings.Value;
            Assure.ArgumentNotNull(_settings, nameof(_settings));
            
            _yandexTranslateClient = yandexTranslateClient;
            _logger = logger;
            
            //_translateService = translateService;
            _restClient = new RestClient(_settings.ApiClientId, _settings.ApiClientSecret);
            lock (_transLock)
            {
                _valuesToTranslate = new HashSet<string>();
            }
        }

        #region Search

        private const int MaxPageSize = 10;
        private const int MaxDescriptionLength = 250;
        
        public async Task<IList<int>> GetCategoriesByFullModelName(string modelName, CancellationToken cancellationToken)
        {
            var categoriesResponse = await _restClient.GetCategoriesByModel(modelName, cancellationToken);
            var categories = categoriesResponse.Matching_Categories.ToList();
            var newColl = new List<Category>();
            foreach (var c in categories)
            {
                ProcessCategories(newColl, c);
            }
            categories = newColl;
            return categories.Select(x=>x.Id).Distinct().ToList();
        }

        private void ProcessCategories(IList<Category> categories, Category category)
        {
            if (category.Parent != null)
            {
                ProcessCategories(categories, category.Parent);
            }

            if (categories.FirstOrDefault(x => x.Id == category.Id) != null)
            {
                return;
            }
            categories.Add(category);
        }

        public async Task<SearchOffersResponse> SearchOffersAsync(
            string phrase,
            string translatedPhrase,
            string categoryId,
            OfferStateEnum state,
            OfferSortingEnum sorting,
            int offset,
            int limit,
            CancellationToken cancellationToken)
        {
            if (limit > MaxPageSize)
            {
                throw new NotSupportedException($"Max '{MaxPageSize}' page size is supported.");
            }

            _valuesToTranslate = new HashSet<string>();
            var arPhrase = new[] { phrase };
            if (string.IsNullOrEmpty(translatedPhrase)
                && !string.IsNullOrEmpty(phrase))
            {
                if (_settings.IsTranslationEnabled)
                {
                    var translatedPhrases = await _yandexTranslateClient.TranslateAsync(
                        new[] { phrase },
                        Languages.RussianCode,
                        Languages.PolishCode,
                        cancellationToken);
                    translatedPhrase = translatedPhrases[0];
                }
                else
                {
                    translatedPhrase = phrase;
                }
            }

            // search for offers
            var apiResponse = await _restClient.SearchOffersAsync(translatedPhrase, categoryId, state, sorting, offset, limit, cancellationToken);
            var apiOffers = new List<Rest.Models.Offer>();
            if (apiResponse.Items.Promoted?.Length > 0)
            {
                apiOffers.AddRange(apiResponse.Items.Promoted);
            }

            if (apiResponse.Items.Regular?.Length > 0)
            {
                apiOffers.AddRange(apiResponse.Items.Regular);
            }

            if (apiOffers.Count > MaxPageSize)
            {
                apiOffers = apiOffers
                    .Take(MaxPageSize)
                    .ToList();
            }

            var offers = apiOffers
                .Select(x => new Offer()
                {
                    Id = x.Id,
                    CategoryId = x.Category?.Id,
                    Name = new MultiLanguageString()
                    {
                        [Languages.PolishCode] = x.Name ?? "",
                    },
                    Price = x.SellingMode?.GetMaxPrice()?.Amount ?? 0,
                    PriceCurrencyCode = x.SellingMode?.GetMaxPrice()?.Currency ?? "PLN",
                    Images = x.Images,
                    // WORKAROUND! since WebAPI with attributes was disabled:
                    // - state should be non-All here
                    // - delivery options are set by REST API delivery/lowestPrice
                    State = state,
                    DeliveryOptions = new[]
                    {
                        new DeliveryOption()
                        {
                            Price = x.Delivery?.LowestPrice?.Amount ?? 0,
                        },
                    }
                })
                .ToArray();



            List<Task> taskList = new List<Task>();

            /*foreach (var o in offers)
            {
                //Thread.Sleep(10);
                taskList.Add(Task.Run(() =>
                     SetOfferExtraData(o, cancellationToken)
                ));
            }*/

            /*var stateAndDeliveryOptionsTask = Task.Run(
                () => RequestOfferStatesAndDeliveryOptionsAsync(offers, state, cancellationToken),
                cancellationToken);*/


            //taskList.Add(stateAndDeliveryOptionsTask);            

            await Task.WhenAll(taskList);
            AddNamesToTranslate(offers);

            return new SearchOffersResponse()
            {
                Offers = offers,
                TranslatedPhrase = translatedPhrase,
                Total = apiResponse.SearchMeta?.TotalCount ?? 0,
            };
        }

        private void AddNamesToTranslate(Offer[] offers)
        {
            try
            {
                for (var i = 0; i < offers.Length; i++)
                {
                    var offer = offers[i];
                    lock (_transLock)
                    {
                        if (!_valuesToTranslate.Contains(offer.Name[Languages.PolishCode]))
                        {
                            _valuesToTranslate.Add(offer.Name[Languages.PolishCode]);
                        }
                    }
                    //offer.Name[Languages.RussianCode] = await TranslateLocalAsync(offer.Name[Languages.PolishCode], cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Offer names translation failed.", ex);
            }
        }

        private void AddExtraDataToTranslate(OfferExtraData data)
        {
            lock (_transLock)
            {
                /*if (!_valuesToTranslate.Contains(data.Description[Languages.PolishCode]))
                {
                    _valuesToTranslate.Add(data.Description[Languages.PolishCode]);
                }*/
                foreach (var p in data.Parameters)
                {
                    if (!_valuesToTranslate.Contains(p.Name[Languages.RussianCode]))
                    {
                        _valuesToTranslate.Add(p.Name[Languages.RussianCode]);
                    }
                    if (!_valuesToTranslate.Contains(p.Value[Languages.RussianCode]))
                    {
                        _valuesToTranslate.Add(p.Value[Languages.RussianCode]);
                    }
                }
            }
        }

        private async Task<IDictionary<string,string>> GetForTranslateDictionary(IDictionary<string,string> dict, CancellationToken cancellationToken)
        {             
            if (!_settings.IsTranslationEnabled)
            {
                return new Dictionary<string, string>();
            }

            var translatedTexts = dict.Select(x => x.Key).ToList();

            var forYandex = _valuesToTranslate.Where(x => !translatedTexts.Contains(x.ToLower())).ToArray();
            var yandexTranslated = await _yandexTranslateClient.TranslateAsync(forYandex, Languages.PolishCode.ToLower(), Languages.RussianCode.ToLower(), cancellationToken);


            var yandexDict = new Dictionary<string, string>();

            if (forYandex.Count() == yandexTranslated.Count())
                for (var i = 0; i < forYandex.Count(); i++)
                {
                    if (!yandexDict.ContainsKey(forYandex[i].ToLower()))
                    {
                        yandexDict.Add(forYandex[i].ToLower(), yandexTranslated[i].ToLower());
                    }
                }
            return yandexDict;
        }

        public async Task ApplyTranslations(ITranslateService translateService, Offer[] offers, string sourceText, string destText, CancellationToken cancellationToken)
        {
            try
            {
                var dict = await translateService.GetDictionary(_valuesToTranslate);
                var yandexDict = await GetForTranslateDictionary(dict, cancellationToken);
                await translateService.AddRecords(yandexDict, Languages.PolishCode, Languages.RussianCode, Guid.NewGuid()); 

                foreach (var o in offers)
                {                    
                    o.Name[Languages.RussianCode] = GetSafeValFromDictionary(dict, yandexDict, o.Name[Languages.PolishCode]);                    
                }
            }
            catch(Exception er)
            {
                throw;
            }
        }
        
        private async Task ApplyTranslationsExtraData(ITranslateService translateService, OfferExtraData data, CancellationToken cancellationToken)
        {
            var dict = await translateService.GetDictionary(_valuesToTranslate);
            var yandexDict = await GetForTranslateDictionary(dict, cancellationToken);
            await translateService.AddRecords(yandexDict, Languages.PolishCode, Languages.RussianCode, Guid.NewGuid());            
            
            var state = data.Parameters.FirstOrDefault(x => x.Name[Languages.PolishCode].ToLower() == StateAttributeName.ToLower());
            if (state != null)
            {
                data.State = RestClient.StatesByNames.ContainsKey(state.Value[Languages.PolishCode].ToLower()) ? RestClient.StatesByNames[state.Value[Languages.PolishCode].ToLower()] : OfferStateEnum.New;
            }

            data.Description[Languages.RussianCode] = GetSafeValFromDictionary(dict, yandexDict, data.Description[Languages.PolishCode]);
            foreach (var p in data.Parameters)
            {
                p.Name[Languages.RussianCode] = GetSafeValFromDictionary(dict, yandexDict, p.Name[Languages.PolishCode]);
                p.Value[Languages.RussianCode] = GetSafeValFromDictionary(dict, yandexDict, p.Value[Languages.PolishCode]);
            }
            
        }

        private string GetSafeValFromDictionary(IDictionary<string, string> dict1, IDictionary<string, string> dict2, string val)
        {
            if (dict1.ContainsKey(val.ToLower()))
            {
                return dict1[val.ToLower()];
            }
            if (dict2.ContainsKey(val.ToLower()))
            {
                return dict2[val.ToLower()];
            }
            return _valuesToTranslate.FirstOrDefault(x => x.ToLower() == val) ?? val;
        }

        public async Task<string> GetTranslation(ITranslateService service, string term, CancellationToken cancellationToken)
        {
            lock (_transLock)
            {
                _valuesToTranslate = new HashSet<string>() { term };
            }
            var old = await service.GetTranslatedText(term);

            if (!String.IsNullOrEmpty(old))
            {
                return old;
            }

            if (_settings.IsTranslationEnabled)
            {
                var res = await _yandexTranslateClient.TranslateAsync(new string[] { term }, Languages.PolishCode, Languages.RussianCode, cancellationToken);
                if (res.Any())
                {
                    return res[0];
                }
            }

            return term;
        }

        private const string StateAttributeName = "Stan";        

        private bool IsPolandShipment(PostageStruct postageStruct)
        {
            var shipmentId = postageStruct.postageId;
            return shipmentId < 100 || shipmentId > 200;
        }       

        #endregion

        #region Description

        public async Task<OfferExtraData> GetOfferDescriptionAsync(ITranslateService translateService, string offerId, CancellationToken cancellationToken)
        {   try
            {
                lock (_transLock)
                {
                    _valuesToTranslate = new HashSet<string>();
                }
                /*return new OfferExtraData()
                {
                    Description = new MultiLanguageString() { [Languages.PolishCode] = "test", [Languages.RussianCode] = "test11" },
                    State = OfferStateEnum.Used,
                    Parameters = new List<OfferParameter>() { new OfferParameter() { Name = new MultiLanguageString() { [Languages.PolishCode] = "test", [Languages.RussianCode] = "test11" }, Value = new MultiLanguageString() { [Languages.PolishCode] = "test", [Languages.RussianCode] = "test11" } } }
                };*/
                var data = _restClient.GetExtraDataInit(offerId);
                var state = data.Parameters.FirstOrDefault(x => x.Name[Languages.PolishCode].ToLower() == StateAttributeName.ToLower());
                if (state != null) 
                {
                    data.State = RestClient.StatesByNames.ContainsKey(state.Value[Languages.PolishCode].ToLower()) ? RestClient.StatesByNames[state.Value[Languages.PolishCode].ToLower()] : OfferStateEnum.New;
                }

                data.Description[Languages.RussianCode] =
                   data.Description[Languages.PolishCode] = ConvertDescriptionHtmlToText(data.Description[Languages.PolishCode]);
                AddExtraDataToTranslate(data);
                await ApplyTranslationsExtraData(translateService, data, cancellationToken);
                return data;
            }
            catch(Exception er)
            {
                throw;
            }
        }

        #endregion

        #region HtmlToText

        // http://htmlbook.ru/samhtml/tekst/spetssimvoly
        private static readonly Dictionary<string, string> SpecialHtmlSymbolReplacements = new Dictionary<string, string>()
        {
            ["&nbsp;"] = " ",
            ["&amp;"] = "&",
            ["&quot;"] = "\"",
            ["&lt;"] = "<",
            ["&gt;"] = ">",
            ["&copy;"] = "©",
            ["&reg;"] = "®",
            ["&trade;"] = "™",
        };

        // http://www.beansoftware.com/ASP.NET-Tutorials/Convert-HTML-To-Plain-Text.aspx
        // https://www.codeproject.com/Articles/11902/Convert-HTML-to-Plain-Text-2
        private string ConvertDescriptionHtmlToText(string html)
        {
            Assure.ArgumentNotNull(html, nameof(html));

            // removal of head/script are not required since they are not presented in Allegro description HTML

            var htmlStringBuilder = new StringBuilder(html);

            // remove new lines since they are not visible in HTML
            htmlStringBuilder.Replace("\n", " ");
            htmlStringBuilder.Replace("\r", " ");

            // remove tab spaces
            htmlStringBuilder.Replace("\t", " ");

            // replace special characters like &, <, >, " etc.
            foreach (var (specialSymbol, replacement) in SpecialHtmlSymbolReplacements)
            {
                htmlStringBuilder.Replace(specialSymbol, replacement);
            }

            // insert line breaks, spaces, etc. (simple replace is used instead of regex since allegro description contains HTML tags without attributes)
            htmlStringBuilder.Replace("<p>", "\n<p>");
            htmlStringBuilder.Replace("<h1>", "\n<h1>");
            htmlStringBuilder.Replace("<h2>", "\n<h2>");
            htmlStringBuilder.Replace("<h3>", "\n<h3>");
            htmlStringBuilder.Replace("<tr>", "\n<tr>");
            htmlStringBuilder.Replace("<td>", " <td>");
            htmlStringBuilder.Replace("<li>", "\n- <li>");

            html = htmlStringBuilder.ToString();

            // remove others special symbols
            html = Regex.Replace(html, @"&(.{2,6});", "", RegexOptions.IgnoreCase);

            // remove all HTML tags
            html = Regex.Replace(html, "<[^>]*>", "");

            // remove multiple spaces
            html = Regex.Replace(html, " +", " ");

            // remove first space in line
            html = html
                .Replace("\n ", "\n").Replace("&amp;", "&")
                .Trim();

            return html.Length > MaxDescriptionLength ? html.Substring(0, MaxDescriptionLength - 3) + "..." : html;
        }

        #endregion
    }
}