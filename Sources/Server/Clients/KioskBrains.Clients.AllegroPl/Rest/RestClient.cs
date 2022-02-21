using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using KioskBrains.Clients.AllegroPl.Models;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KioskBrains.Clients.AllegroPl.Rest
{
    /// <summary>
    /// Keep in mind that it's a singleton.
    /// </summary>
    public class RestClient
    {
        public static IDictionary<string, OfferStateEnum> StatesByNames => new Dictionary<string, OfferStateEnum>()
        {
            {"nowy", OfferStateEnum.New },
            {"używany", OfferStateEnum.Used },
            {"odzyskany",  OfferStateEnum.Recovered}
        };
        public RestClient(string clientId, string clientSecret)
        {
            Assure.ArgumentNotNull(clientId, nameof(clientId));
            Assure.ArgumentNotNull(clientSecret, nameof(clientSecret));

            _clientToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        }

        #region Authentication

        private readonly string _clientToken;

        private string _accessToken;

        private readonly object _authRequestLocker = new object();

        private bool _isAuthRequestInProgress;

        private async Task EnsureAuthSessionAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            if (!IsAuthSessionExpired(now))
            {
                return;
            }

            lock (_authRequestLocker)
            {
                if (_isAuthRequestInProgress)
                {
                    if (_accessToken == null)
                    {
                        // possible for parallel requests after app started
                        throw new AllegroPlRequestException("Auth session is being initialized...");
                    }

                    // AccessTokenDuration is much lower than actual one, so old access token can be used even while new one is requested
                    return;
                }

                _isAuthRequestInProgress = true;
            }

            try
            {
                string responseBody;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Clear();
                        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_clientToken}");
                        var httpResponse = await httpClient.PostAsync(
                            "https://allegro.pl/auth/oauth/token?grant_type=client_credentials",
                            new StringContent(""),
                            cancellationToken);
                        responseBody = await httpResponse.Content.ReadAsStringAsync();
                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            throw new AllegroPlRequestException($"Request to auth API failed, response code {(int)httpResponse.StatusCode}, body: {responseBody}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (AllegroPlRequestException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new AllegroPlRequestException("Request to auth API failed, no response.", ex);
                }

                string accessToken;
                try
                {
                    const string AccessTokenProperty = "access_token";
                    var response = JsonConvert.DeserializeObject<JObject>(responseBody);
                    accessToken = response[AccessTokenProperty].Value<string>();
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        throw new Exception($"'{AccessTokenProperty}' is null or empty.");
                    }
                }
                catch (Exception ex)
                {
                    throw new AllegroPlRequestException("Bad format of auth API response.", ex);
                }

                _accessToken = accessToken;
                _accessTokenTime = now;
            }
            finally
            {
                lock (_authRequestLocker)
                {
                    _isAuthRequestInProgress = false;
                }
            }
        }

        private DateTime? _accessTokenTime;

        private static readonly TimeSpan AccessTokenDuration = TimeSpan.FromHours(6);

        private bool IsAuthSessionExpired(DateTime now)
        {
            var accessTokenTime = _accessTokenTime;
            return accessTokenTime == null
                   || accessTokenTime.Value + AccessTokenDuration < now;
        }

        #endregion

        private async Task<Models.SearchOffersResponse> GetAsync<SearchOffersResponse>(
            string action,
            Dictionary<string, string> queryParameters,
            CancellationToken cancellationToken)
            where SearchOffersResponse : new()
        {
            await EnsureAuthSessionAsync(cancellationToken);

            string responseBody;
            try
            {
                var uriBuilder = new UriBuilder($"http://95.111.250.32/allegro/parser.php");
                if (queryParameters?.Count > 0)
                {
                    uriBuilder.Query = string.Join(
                        "&",
                        queryParameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
                }
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    using (var client = new HttpClient(httpClientHandler))
                    {
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
                        client.DefaultRequestHeaders.Add("Accept", "application/vnd.allegro.public.v1+json");
                        var httpResponse = await client.GetAsync(uriBuilder.Uri, cancellationToken);
                        responseBody = await httpResponse.Content.ReadAsStringAsync();
                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            throw new AllegroPlRequestException($"Request to API failed, action {action}, response code {(int)httpResponse.StatusCode}, body: {responseBody}");
                        }
                    }
                }
                
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (AllegroPlRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AllegroPlRequestException($"Request to API failed, action {action}, no response.", ex);
            }

            try
            {
                var parserResponce = JsonConvert.DeserializeObject<ParserResponse>(responseBody);
                Models.SearchOffersResponse responceOld = new Models.SearchOffersResponse();

                IList<Models.Offer> tempSSS = new List<Models.Offer>();
                //Models.Offer[] offersParse = new Models.Offer[];
                foreach (var item in parserResponce.products)
                {
                    Models.Offer offerItem = new Models.Offer();
                    offerItem.Id = item.id;
                    offerItem.Name = item.title;
                    //offerItem.Category.Id = item.ca
                    if (item.price.Contains(".")) { 
                        item.price = item.price.Substring(0, item.price.LastIndexOf("."));
                    }
                    if (item.price_with_delivery.Contains("."))
                    {
                        item.price_with_delivery = item.price_with_delivery.Substring(0, item.price_with_delivery.LastIndexOf("."));
                    }
                    offerItem.Category = new Models.OfferCategory()
                    {
                        Id = queryParameters["category"]
                    };
                    offerItem.SellingMode = new Models.OfferSellingMode()
                    {
                        Price = new Models.OfferPrice() { Amount = Convert.ToDecimal(item.price) }
                    };
                    offerItem.Delivery = new Models.OfferDelivery()
                    {
                        LowestPrice = new Models.OfferPrice() { Amount = Convert.ToDecimal(item.price_with_delivery) - Convert.ToDecimal(item.price) }
                    };
                    offerItem.Images = new OfferImage[] { new OfferImage { Url = item.mainImage } };

                    tempSSS.Add(offerItem);
                }
                responceOld.Items = new Models.SearchOffersResponseItems() { Regular = tempSSS.ToArray() };


                return responceOld;
            }
            catch (Exception ex)
            {
                throw new AllegroPlRequestException("Bad format of API response.", ex);
            }
        }
        public class ParserResponse
        {
            public string success { get; set; }
            public string totalCount { get; set; }
            public string lastAvailablePage { get; set; }
            public IList<ParserResponseItem> products { get; set; }
        }
        public class ParserResponseItem
        {
            public string id { get; set; }

            public string url { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string price_with_delivery { get; set; }
            public string mainThumbnail { get; set; }
            public string mainImage { get; set; }

        }
        private const string StateFilterId = "parameter.11323";

        public async Task<SearchCategoriesResponse> GetCategoriesByModel(string model, CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, string>
            {
                ["name"] = model
            };
            var action = "/sale/matching-categories";
            var response = await GetAsync<SearchCategoriesResponse>(action, parameters, cancellationToken);
            return null;
        }

        internal async Task<Models.SearchOffersResponse> SearchOffersAsync(
            string phrase,
            string categoryId,
            OfferStateEnum state,
            OfferSortingEnum sorting,
            int offset,
            int limit,
            CancellationToken cancellationToken)
        {
            if (state == OfferStateEnum.All) {
                state = OfferStateEnum.Used;
            }
            Assure.ArgumentNotNull(categoryId, nameof(categoryId));

            Assure.ArgumentNotNull(categoryId, nameof(categoryId));
            var parameters = new Dictionary<string, string>
            {
                ["category"] = categoryId,
            };
            
            if (!string.IsNullOrEmpty(phrase))
            {
                parameters["query"] = phrase;
            }

            if (state != OfferStateEnum.All)
            {
                string stateFilterValue;
                switch (state)
                {
                    case OfferStateEnum.New:
                        stateFilterValue = "nowe";
                        break;
                    case OfferStateEnum.Used:
                        stateFilterValue = "używane";
                        break;
                    case OfferStateEnum.Recovered:
                        stateFilterValue = "11323_246462";
                        break;
                    case OfferStateEnum.Broken:
                        stateFilterValue = "11323_238062";
                        break;
                    default:
                        stateFilterValue = null;
                        break;
                }

                parameters["status"] = stateFilterValue;
            }

            string sortingValue;
            switch (sorting)
            {
                case OfferSortingEnum.Relevance:
                    sortingValue = "-relevance";
                    break;
                case OfferSortingEnum.PriceAsc:
                    sortingValue = "p";
                    break;
                case OfferSortingEnum.PriceDesc:
                    sortingValue = "pd";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sorting), sorting, null);
            }
            parameters["sort"] = sortingValue;
            parameters["page"] = (offset / 10).ToString();
            parameters["api_key"] = "Umthudpx8FCs9ks6rBpB";
            parameters["method"] = "search";

            var action = "/offers/listing";
            var response = await GetAsync<Models.SearchOffersResponse>(action, parameters, cancellationToken);
            if (response.Items == null)
            {
                throw new AllegroPlRequestException($"Request to API failed, action {action}, {nameof(response.Items)} or {nameof(response.SearchMeta)} is null.");
            }

            return response;
        }

        public OfferExtraData GetExtraDataInit(string id)
        {
            var text = "";
            try
            {
                var res = GetAsyncDetails(id).Result;

                List<OfferParameter> listParameters = new List<OfferParameter>();

                foreach (var item in res.specifications.Parametry)
                {
                    listParameters.Add(new OfferParameter()
                    {
                        Name = new MultiLanguageString() { [Languages.PolishCode] = item.Key },
                        Value = new MultiLanguageString() { [Languages.PolishCode] = item.Value.ToString() }
                    });
                }

                var sections = res.description.sections;
                var blockSections = sections.Select(x => x.items);
                string polishDesc="";
                foreach (var itemBlock in blockSections) {
                    var textBlock = itemBlock.Where(x=>x.type=="TEXT");
                    if (textBlock.Count() > 0) {
                        polishDesc = textBlock.FirstOrDefault().content;
                        break;
                    }
                }

                var descMultiNew = new MultiLanguageString()
                {
                    [Languages.PolishCode] = polishDesc
                };

                OfferImage[] imagePathesNew = res.images.Select(x => new OfferImage() { Url = x.original }).ToArray();

                IList<OfferImage> successImages = new List<OfferImage>();
                foreach (var item in imagePathesNew)
                {
                    if (!successImages.Select(x => x.Url).Contains(item.Url))
                    {
                        successImages.Add(item);
                    }
                }
                IEnumerable<OfferImage> tempImage = successImages.Distinct();


                return new OfferExtraData()
                {
                    Description = descMultiNew,
                    Parameters = listParameters
                };
            }
            catch (AllegroPlRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AllegroPlRequestException($"Request to https://allegro.pl/oferta failed", ex);
            }
        }

        private OfferParameter GetParameterFromLine(string line)
        {
            try
            {
                return new OfferParameter()
                {
                    Name = new MultiLanguageString()
                    {
                        [Languages.PolishCode] = line.Split(':')[0].Trim(),
                        [Languages.RussianCode] = line.Split(':')[0].Trim()
                    },
                    Value = new MultiLanguageString()
                    {
                        [Languages.PolishCode] = line.Split(':')[1].Trim(),
                        [Languages.RussianCode] = line.Split(':')[1].Trim()
                    }
                };
            }
            catch (Exception er)
            {
                return new OfferParameter()
                {
                    Name = new MultiLanguageString()
                    {
                        [Languages.PolishCode] = ""
                    },
                    Value = new MultiLanguageString()
                    {
                        [Languages.PolishCode] = ""
                    }
                };
            }
        }
        private async Task<DetailParserData> GetAsyncDetails(string id)
        {
            string responseBody;
            DetailParserData parserResponce;
            using (var httpClientHandler = new HttpClientHandler())
            {

                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    var httpResponse = await client.GetAsync("http://95.111.250.32/allegro/parser.php?api_key=Umthudpx8FCs9ks6rBpB&method=details&product_id=" + id, CancellationToken.None);
                    responseBody = await httpResponse.Content.ReadAsStringAsync(); //httpResponse.Result;
                }
            }
            try
            {
                parserResponce = JsonConvert.DeserializeObject<DetailParserData>(responseBody);
            }
            catch (Exception ex)
            {
                throw new AllegroPlRequestException("Bad format of API response.", ex);
            }
            return parserResponce;
        }
        public class Specifications
        {
            public Dictionary<string, object> Parametry { get; set; }
        }
        public class Items
        {
            public string type { get; set; }
            public string alt { get; set; }
            public string url { get; set; }
            public string content { get; set; }

        }
        public class Sections
        {
            public IList<Items> items { get; set; }

        }
        public class Description
        {
            public IList<Sections> sections { get; set; }

        }
        public class DetailParserData
        {
            public string id { get; set; }
            public string title { get; set; }
            public bool active { get; set; }
            public int availableQuantity { get; set; }
            public string price { get; set; }
            public string price_with_delivery { get; set; }
            public string currency { get; set; }
            public decimal seller_rating { get; set; }
            public Specifications specifications { get; set; }
            public IEnumerable<ImagesParser> images { get; set; }
            public Description description { get; set; }
            //public object compatibility { get; set; }
        }
        public class ImagesParser
        {
            public string original { get; set; }
            public string thumbnail { get; set; }
            public string embeded { get; set; }
            public string alt { get; set; }

        }
    }
}