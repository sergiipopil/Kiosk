using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AllegroSearchService.Bl.ServiceInterfaces;
using KioskBrains.Clients.AllegroPl;
using KioskBrains.Clients.AllegroPl.Models;
using KioskBrains.Clients.AllegroPl.Rest;
using KioskBrains.Clients.YandexTranslate;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Common.Log;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoapService;

namespace AllegroSearchService.Bl
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
        private readonly ITokenService _tokenService;

        private object _transLock = new object();
        private ISet<String> _valuesToTranslate;

        public AllegroPlClient(
            IOptions<AllegroPlClientSettings> settings,
            YandexTranslateClient yandexTranslateClient,
            ILogger<AllegroPlClient> logger, ITokenService tokenService, ITranslateService translateService)
        {
            _settings = settings.Value;
            Assure.ArgumentNotNull(_settings, nameof(_settings));

            _yandexTranslateClient = yandexTranslateClient;
            _logger = logger;



            _tokenService = tokenService;
            _translateService = translateService;
            _restClient = new RestClient(_settings.ApiClientId, _settings.ApiClientSecret);
            _valuesToTranslate = new HashSet<string>();
        }

        #region Search

        private const int MaxPageSize = 10;

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
            var apiOffers = new List<KioskBrains.Clients.AllegroPl.Rest.Models.Offer>();
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
            if (_settings.IsTranslationEnabled)
            {
                AddNamesToTranslate(offers);
                await ApplyTranslations(offers, phrase, translatedPhrase, cancellationToken);
            }

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

        private async Task ApplyTranslations(Offer[] offers, string sourceText, string destText, CancellationToken cancellationToken)
        {
            var dict = _translateService.GetDictionary(_valuesToTranslate);


            var translatedTexts = dict.Select(x => x.Key).ToList();

            var forYandex = _valuesToTranslate.Where(x => !translatedTexts.Contains(x.ToLower())).ToArray();
            var yandexTranslated = await _yandexTranslateClient.TranslateAsync(forYandex, Languages.PolishCode.ToLower(), Languages.RussianCode.ToLower(), cancellationToken);

            var yandexList = from f in forYandex
                             join y in yandexTranslated
                             on Array.IndexOf(forYandex, f) equals Array.IndexOf(yandexTranslated, y)
                             select new { Key = f, Value = y };
            var yandexDict = yandexList.ToDictionary(x => x.Key.ToLower(), x => x.Value);

            if (!yandexDict.ContainsKey(sourceText.ToLower()))
            {
                yandexDict.Add(sourceText.ToLower(), destText);
            }

            await _translateService.AddRecords(yandexDict, Languages.PolishCode, Languages.RussianCode, Guid.NewGuid());

            foreach (var o in offers)
            {
                /*var state = o.Parameters.FirstOrDefault(x => x.Name[Languages.PolishCode].ToLower() == StateAttributeName.ToLower());
                if (state != null)
                {
                    o.State = RestClient.StatesByNames.ContainsKey(state.Value[Languages.PolishCode].ToLower()) ? RestClient.StatesByNames[state.Value[Languages.PolishCode].ToLower()] : OfferStateEnum.New;
                }*/
                o.Name[Languages.RussianCode] = GetSafeValFromDictionary(dict, yandexDict, o.Name[Languages.PolishCode]);
                /*o.Description[Languages.RussianCode] = GetSafeValFromDictionary(dict, yandexDict, o.Description[Languages.PolishCode]);
                foreach (var p in o.Parameters)
                {
                    p.Name[Languages.RussianCode] = GetSafeValFromDictionary(dict, yandexDict, p.Name[Languages.PolishCode]);
                    p.Value[Languages.RussianCode] = GetSafeValFromDictionary(dict, yandexDict, p.Value[Languages.PolishCode]);
                }*/
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

        private const string StateAttributeName = "Stan";

        private async Task RequestOfferStatesAndDeliveryOptionsAsync(
            Offer[] offers,
            OfferStateEnum requestState,
            CancellationToken cancellationToken)
        {
            // disabled
            return;

            //Assure.ArgumentNotNull(offers, nameof(offers));

            //if (offers.Length == 0)
            //{
            //    return;
            //}

            //var offerIds = offers
            //    .Select(x => long.Parse(x.Id))
            //    .ToArray();

            //var isStateSpecifiedByRequest = requestState != OfferStateEnum.All;

            //var infoResponse = await _soapClient.GetItemsInfoAsync(
            //    offerIds,
            //    includeAttributes: !isStateSpecifiedByRequest,
            //    includeDeliveryOptions: true,
            //    includeDescription: false,
            //    cancellationToken: cancellationToken);

            //var infoRecords = infoResponse.arrayItemListInfo ?? new ItemInfoStruct[0];
            //var infoRecordsById = infoRecords
            //    .Where(x => x.itemInfo?.itId != null)
            //    .ToDictionary(x => x.itemInfo.itId.ToString());
            //foreach (var offer in offers)
            //{
            //    var infoRecord = infoRecordsById.GetValueOrDefault(offer.Id);

            //    // STATE
            //    if (isStateSpecifiedByRequest)
            //    {
            //        offer.State = requestState;
            //    }
            //    else
            //    {
            //        const OfferStateEnum StateDefaultValue = OfferStateEnum.New;

            //        if (infoRecord != null)
            //        {
            //            switch (infoRecord.itemInfo?.itIsNewUsed)
            //            {
            //                case 1:
            //                    offer.State = OfferStateEnum.New;
            //                    break;
            //                case 2:
            //                    offer.State = OfferStateEnum.Used;
            //                    break;
            //                default:
            //                    var stateValue = infoRecord.itemAttribs
            //                        ?.Where(x => x.attribName?.Equals(StateAttributeName, StringComparison.OrdinalIgnoreCase) == true)
            //                        .Select(x => x.attribValues?.FirstOrDefault())
            //                        .FirstOrDefault();
            //                    if (stateValue == null)
            //                    {
            //                        offer.State = StateDefaultValue;
            //                    }
            //                    else if (stateValue.StartsWith("Regen", StringComparison.OrdinalIgnoreCase))
            //                    {
            //                        offer.State = OfferStateEnum.Recovered;
            //                    }
            //                    else if (stateValue.StartsWith("Usz", StringComparison.OrdinalIgnoreCase))
            //                    {
            //                        offer.State = OfferStateEnum.Broken;
            //                    }
            //                    else
            //                    {
            //                        offer.State = StateDefaultValue;
            //                    }

            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            offer.State = StateDefaultValue;
            //        }
            //    }

            //    // DELIVERY OPTIONS
            //    offer.DeliveryOptions = infoRecord
            //        ?.itemPostageOptions
            //        ?.Where(x => IsPolandShipment(x))
            //        .Select(x => new DeliveryOption()
            //        {
            //            Price = (decimal)x.postageAmount,
            //        })
            //        .ToArray();
            //}
        }

        private bool IsPolandShipment(PostageStruct postageStruct)
        {
            var shipmentId = postageStruct.postageId;
            return shipmentId < 100 || shipmentId > 200;
        }

        private async Task TranslateNamesAsync(Offer[] offers, CancellationToken cancellationToken)
        {
            try
            {
                var texts = offers
                    .Select(x => x.Name.GetValue(Languages.PolishCode))
                    .ToArray();

                var translatedTexts = await _yandexTranslateClient.TranslateAsync(
                    texts,
                    Languages.PolishCode,
                    Languages.RussianCode,
                    cancellationToken);

                for (var i = 0; i < offers.Length; i++)
                {
                    var offer = offers[i];
                    offer.Name[Languages.RussianCode] = translatedTexts[i];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.ExternalApiError, "Offer names translation failed.", ex);
            }
        }

        #endregion

        #region Description

        public OfferExtraData GetOfferDescriptionAsync(string offerId, CancellationToken cancellationToken)
        {
            return _restClient.GetOfferDescriptionAsync(offerId, cancellationToken);
        }
        #endregion       
    }
}