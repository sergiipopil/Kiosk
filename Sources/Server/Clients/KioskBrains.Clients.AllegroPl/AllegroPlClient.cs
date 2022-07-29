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

        private readonly ITranslateService _translateService;

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

        private const int MaxPageSize = 40;
        private const int MaxDescriptionLength = 250;

        public async Task<IList<string>> GetCategoriesByFullModelName(string modelName, CancellationToken cancellationToken)
        {
            var categoriesResponse = await _restClient.GetCategoriesByModel(modelName, cancellationToken);
            var categories = categoriesResponse.Matching_Categories.ToList();
            var newColl = new List<Category>();
            foreach (var c in categories)
            {
                ProcessCategories(newColl, c);
            }
            categories = newColl;
            return categories.Select(x => x.Id).Distinct().ToList();
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
            CancellationToken cancellationToken,
            bool isBody)
        {
            phrase = phrase.TrimEnd();
            if (limit > MaxPageSize)
            {
                throw new NotSupportedException($"Max '{MaxPageSize}' page size is supported.");
            }

            _valuesToTranslate = new HashSet<string>();
            var arPhrase = new[] { phrase };
            //if (phrase.ToLower().Contains("рулевая рейка"))
            //{
            //    phrase = phrase.ToLower().Replace("рулевая рейка", ("PRZEKŁADNIA MAGLOWNICA").ToLower());
            //}

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
            var apiResponse = await _restClient.SearchOffersAsync(translatedPhrase, categoryId, state, sorting, offset, limit, cancellationToken, isBody);
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
            try
            {
                AddNamesToTranslate(offers);
            }
            catch
            {

            }


            return new SearchOffersResponse()
            {
                Offers = offers,
                TranslatedPhrase = translatedPhrase,
                Total = Convert.ToInt32(apiResponse.ReallyTotalCount),
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

        private void AddParametersToTranslate(List<OfferParameter> parameters)
        {
            lock (_transLock)
            {
                foreach (var p in parameters)
                {
                    if (!_valuesToTranslate.Contains(p.Name[Languages.PolishCode]))
                    {
                        _valuesToTranslate.Add(p.Name[Languages.PolishCode]);
                    }
                    if (!_valuesToTranslate.Contains(p.Value[Languages.PolishCode]))
                    {
                        _valuesToTranslate.Add(p.Value[Languages.PolishCode]);
                    }
                }
            }
        }

        private async Task<IDictionary<string, string>> GetTranslateDictionary(ITranslateService translateService, IDictionary<string, string> dict, CancellationToken cancellationToken)
        {
            if (!_settings.IsTranslationEnabled)
            {
                return new Dictionary<string, string>();
            }

            var translatedTexts = dict.Select(x => x.Key).ToList();

            var forYandex = _valuesToTranslate.Where(x => !translatedTexts.Contains(x.ToLower())).ToArray();


            /*if (forYandex.Any())
            {
                var nameTerms = await translateService.GetNamesDictionary();
                var j = 0;
                foreach (var name in forYandex)
                {
                    foreach (var t in nameTerms)
                    {
                        if (!String.IsNullOrEmpty(t.Key))
                            forYandex[j] = name.Replace(t.Key, t.Value);
                    }
                    j++;
                }
            }*/

            var yandexTranslated = new string[0];

            try
            {
                yandexTranslated = await _yandexTranslateClient.TranslateAsync(forYandex, Languages.PolishCode.ToLower(), Languages.RussianCode.ToLower(), cancellationToken);
            }
            catch (Exception er)
            {
                yandexTranslated = forYandex;
            }



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

        private async Task<IDictionary<string, string>> GetWoTranslateDictionary(IDictionary<string, string> dict, CancellationToken cancellationToken)
        {
            if (!_settings.IsTranslationEnabled)
            {
                return new Dictionary<string, string>();
            }

            var translatedTexts = dict.Select(x => x.Key).ToList();

            var forYandex = _valuesToTranslate.Where(x => !translatedTexts.Contains(x.ToLower())).ToArray();

            return forYandex.ToDictionary(x => x, x => x);
        }

        public async Task ApplyTranslations(ITranslateService translateService, Offer[] offers, string sourceText, string destText, CancellationToken cancellationToken)
        {
            try
            {
                //Dictionary<string, string> tempDic = new Dictionary<string, string>();
                var dict = await translateService.GetDictionary(_valuesToTranslate);
                //var dictNames = await translateService.GetNamesDictionary();
                //var names = await translateService.GetNamesDictionary();
                //foreach (var item in names)
                //{
                //    foreach (var translateItem in _valuesToTranslate)
                //    {
                //        if (translateItem.ToLower().Contains(item.Key.ToLower()))
                //        {
                //            if (!tempDic.ContainsKey(translateItem.ToLower()))
                //            {
                //                tempDic.Add(translateItem.ToLower(), translateItem.ToLower().Replace(item.Key.ToLower(), item.Value.ToLower()));
                //            }
                //        }
                //    }
                //}

                var yandexDict = await GetTranslateDictionary(translateService, dict, cancellationToken);
                try
                {
                    await translateService.AddRecords(yandexDict, Languages.PolishCode, Languages.RussianCode, Guid.NewGuid());
                }
                catch
                {
                    _logger.LogError("Error translate");
                }

                foreach (var o in offers)
                {
                    if (o.CategoryId == "255119")
                    {
                        if (o.Name[Languages.PolishCode].ToLower().Contains("lampa"))
                        {
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("lampa", "фонарь");
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("tył tylna", "задний");
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("tylne", "задний");
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("tylna", "задний");
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("tył", "задний");
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("lewa", "левый");
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("prawa", "правый");
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("prawy", "правый");
                        }
                        if (o.Name[Languages.PolishCode].ToLower().Contains("lampy"))
                        {
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("lampy", "фонари"); 
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("tył", "задние");
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("tyl", "задниe");
                        }
                    }
                    if (o.CategoryId == "255099")
                    {
                        if (o.Name[Languages.PolishCode].ToLower().Contains("lampa"))
                        {
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("lampa", "фара");
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("lewa", "левая");
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("lewy", "левая");
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("prawa", "правая");
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("prawy", "правая");
                        }
                        if (o.Name[Languages.PolishCode].ToLower().Contains("lampy"))
                        {
                            o.Name[Languages.PolishCode] = o.Name[Languages.PolishCode].ToLower().Replace("lampy", "фары");
                        }
                    }
                    o.Name[Languages.RussianCode] = GetSafeValFromDictionary(dict, yandexDict, o.Name[Languages.PolishCode], translateService);
                }
            }
            catch (Exception er)
            {
                //throw;
            }
        }
        private async Task<string> ApplyTranslationsDescription(ITranslateService translateService, string descPolish, CancellationToken cancellationToken)
        {
            var desc = descPolish.ToLower();
            var allParams = await translateService.GetDescriptionDictionary();

            var dict123 = GetTranslation(translateService, desc, cancellationToken);
            foreach (var p in allParams)
            {
                if (!String.IsNullOrEmpty(p.Key))
                    desc = desc.Replace(p.Key, p.Value);
            }
            return dict123.Result;
        }
        private async Task ApplyTranslationsExtraData(ITranslateService translateService, Offer data, CancellationToken cancellationToken)
        {
            var dict = await translateService.GetDictionary(_valuesToTranslate);
            var woTranslateDict = await GetWoTranslateDictionary(dict, cancellationToken);

            var yandexDict = await GetTranslateDictionary(translateService, dict, cancellationToken);
            try
            {
                await translateService.AddRecords(yandexDict, Languages.PolishCode, Languages.RussianCode, Guid.NewGuid());
            }
            catch
            {
                _logger.LogError("Error translate");
            }

            var state = data.Parameters.FirstOrDefault(x => x.Name[Languages.PolishCode].ToLower() == StateAttributeName.ToLower());
            if (state != null)
            {
                data.State = RestClient.StatesByNames.ContainsKey(state.Value[Languages.PolishCode].ToLower()) ? RestClient.StatesByNames[state.Value[Languages.PolishCode].ToLower()] : OfferStateEnum.New;
            }
            if (data.CategoryId == "255119")
            {
                if (data.Name[Languages.PolishCode].ToLower().Contains("lampa"))
                {
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("lampa", "фонарь");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("tył tylna", "задний");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("tylne", "задний");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("tylna", "задний");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("tył", "задний");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("lewa", "левый");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("prawa", "правый");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("prawy", "правый");
                }
                if (data.Name[Languages.PolishCode].ToLower().Contains("lampy"))
                {
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("lampy", "фонари");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("tył", "задние");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("tyl", "задниe");
                }
            }
            if (data.CategoryId == "255099")
            {
                if (data.Name[Languages.PolishCode].ToLower().Contains("lampa"))
                {
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("lampa", "фара");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("lewa", "левая");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("lewy", "левая");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("prawa", "правая");
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("prawy", "правая");
                }
                if (data.Name[Languages.PolishCode].ToLower().Contains("lampy"))
                {
                    data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace("lampy", "фары");
                }
            }
            data.Name[Languages.RussianCode] = GetSafeValFromDictionary(dict, woTranslateDict, data.Name[Languages.PolishCode], translateService);

            foreach (var p in data.Parameters)
            {
                p.Name[Languages.RussianCode] = GetSafeValFromDictionary(dict, woTranslateDict, p.Name[Languages.PolishCode], translateService);
                p.Value[Languages.RussianCode] = GetSafeValFromDictionary(dict, woTranslateDict, p.Value[Languages.PolishCode], translateService);
            }
            if (data.CategoryId == "255119")
            {
                if (data.Description[Languages.PolishCode].ToLower().Contains("lampa"))
                {
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("lampa", "фонарь");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("tył tylna", "задний");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("tylne", "задний");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("tylna", "задний");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("tył", "задний");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("lewa", "левый");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("prawa", "правый");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("prawy", "правый");
                }
                if (data.Name[Languages.PolishCode].ToLower().Contains("lampy"))
                {
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("lampy", "фонари");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("tył", "задние");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("tyl", "задниe");
                }
            }
            if (data.CategoryId == "255099")
            {
                if (data.Description[Languages.PolishCode].ToLower().Contains("lampa"))
                {
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("lampa", "фара");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("lewa", "левая");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("lewy", "левая");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("prawa", "правая");
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("prawy", "правая");
                }
                if (data.Description[Languages.PolishCode].ToLower().Contains("lampy"))
                {
                    data.Description[Languages.PolishCode] = data.Description[Languages.PolishCode].ToLower().Replace("lampy", "фары");
                }
            }
            data.Description[Languages.RussianCode] = GetSafeValFromDictionary(dict, woTranslateDict, data.Description[Languages.PolishCode], translateService);
        }

        private string GetSafeValFromDictionary(IDictionary<string, string> dict1, IDictionary<string, string> dict2, string val, ITranslateService translateService = null)
        {
            if (dict1.ContainsKey(val.ToLower()))
            {
                return dict1[val.ToLower()];
            }
            if (dict2.ContainsKey(val.ToLower()))
            {
                return dict2[val.ToLower()];
            }
            var resWords = val.Split().Select(x => x.ToLower());
            IList<string> resReturn = new List<string>();
            var resVal = translateService.GetDictionaryNameTemp(val.ToLower().Split()).Result;

            if (resVal.Count > 0)
            {
                foreach (var itemDic in resWords)
                {
                    if (resVal.ContainsKey(itemDic))
                    {
                        resReturn.Add(resVal.Where(x => x.Key == itemDic).FirstOrDefault().Value);
                    }
                    else
                    {
                        resReturn.Add(itemDic);
                    }
                }
            }
            else
            {
                return val;
            }
            return string.Join(" ", resReturn);
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
        {
            try
            {
                lock (_transLock)
                {
                    _valuesToTranslate = new HashSet<string>();
                }

                var data = _restClient.GetExtraDataInit(offerId);
                var state = data.Parameters.FirstOrDefault(x => x.Name[Languages.PolishCode].ToLower() == StateAttributeName.ToLower());
                if (state != null)
                {
                    data.State = RestClient.StatesByNames.ContainsKey(state.Value[Languages.PolishCode].ToLower()) ? RestClient.StatesByNames[state.Value[Languages.PolishCode].ToLower()] : OfferStateEnum.New;
                }

                data.Description[Languages.RussianCode] =
                   data.Description[Languages.PolishCode] = ConvertDescriptionHtmlToText(data.Description[Languages.PolishCode]);
                AddParametersToTranslate(data.Parameters);
                await ApplyTranslationsExtraData(translateService, new Offer(data), cancellationToken);
                return data;
            }
            catch (Exception er)
            {
                throw;
            }
        }

        #endregion

        public async Task<string> GetOfferDescTranslate(ITranslateService translateService, string descPolish, CancellationToken cancellationToken)
        {
            var result = await ApplyTranslationsDescription(translateService, descPolish, cancellationToken);
            return result;
        }
        IList<string> rulevoe = new List<string>() { "PRZEKŁADNIA KIER", "MAGLOWNICA PRZEKŁADNIA UKŁAD DRĄŻEK", "PRZEKŁADNIA UKŁAD MAGLOWNICA", "przekładnia kierownicza maglownica", "PRZEKLADNIA MAGLOWNICA UKŁAD", "MAGLOWNICA PRZEKŁADNIA UKŁAD",
                "PRZEKŁADNIA MAGLOWNICA UKŁAD", "PRZEKŁADNIA MAGLOWNICA WSPOMAGANIE","MAGLOWNICA PRZEKŁADNIA KIEROWNICZA", "PRZEKŁADNIA KIEROWNICZA ANGLIA", "WSPOMAGANIE KIEROWNICY", "PRZEKŁADNIA MAGLOWNICA", "MAGLOWNICA PRZEKŁADNIA", "MAGLOWNICA PRZEKŁADN", "PRZEKŁADNIA KIEROWNICZA", "PRZEKLADNIA MAGLOWNICA", "PRZKLADNIA KIEROWNICZA",
                "PRZEKLADNIA KIEROWNICZA", "PRZEKŁADNIA UKŁAD", "MAGLOWNICA UKŁAD", "Maglownica", "PRZEKŁADNIA", "PRZEKLADNIA" };
        public async Task<Offer> GetOfferById(ITranslateService translateService, string offerId, CancellationToken cancellationToken)
        {
            lock (_transLock)
            {
                _valuesToTranslate = new HashSet<string>();
            }
            var data = _restClient.GetOfferInit(offerId);
            var state = data.Parameters.FirstOrDefault(x => x.Name[Languages.PolishCode].ToLower() == StateAttributeName.ToLower());
            if (state != null)
            {
                data.State = RestClient.StatesByNames.ContainsKey(state.Value[Languages.PolishCode].ToLower()) ? RestClient.StatesByNames[state.Value[Languages.PolishCode].ToLower()] : OfferStateEnum.New;
            }

            data.Description[Languages.RussianCode] =
               data.Description[Languages.PolishCode] = ConvertDescriptionHtmlToText(data.Description[Languages.PolishCode]);
            foreach (var itemSub in rulevoe)
            {
                data.Name[Languages.PolishCode] = data.Name[Languages.PolishCode].ToLower().Replace(itemSub.ToLower(), "рулевая рейка");
            }
            lock (_transLock)
            {
                _valuesToTranslate.Add(data.Name[Languages.PolishCode]);
            }
            AddParametersToTranslate(data.Parameters);
            await ApplyTranslationsExtraData(translateService, data, cancellationToken);
            return data;
        }

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

            //return html.Length > MaxDescriptionLength ? html.Substring(0, MaxDescriptionLength - 3) + "..." : html;
            return html;
        }

        #endregion
    }
}