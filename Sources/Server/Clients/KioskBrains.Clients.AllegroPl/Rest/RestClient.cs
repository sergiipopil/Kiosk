using System;
using System.Collections.Generic;
using System.Globalization;
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
using ScraperApi;
using System.IO;
using System.Net;

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
                var uriBuilder = new UriBuilder($"https://nifty-volhard.95-111-250-32.plesk.page/allegro/parser.php");
                if (queryParameters?.Count > 0)
                {
                    uriBuilder.Query = string.Join(
                        "&",
                        queryParameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.allegro.public.v1+json");
                    var httpResponse = await httpClient.GetAsync(uriBuilder.Uri, cancellationToken);
                    responseBody = await httpResponse.Content.ReadAsStringAsync();
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new AllegroPlRequestException($"Request to API failed, action {action}, response code {(int)httpResponse.StatusCode}, body: {responseBody}");
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
                    offerItem.SellingMode = new Models.OfferSellingMode()
                    {
                        Price = new Models.OfferPrice() { Amount = Convert.ToDecimal(item.price.Replace('.', ',')) }
                    };
                    offerItem.Delivery = new Models.OfferDelivery()
                    {
                        LowestPrice = new Models.OfferPrice() { Amount = Convert.ToDecimal(item.price_with_delivery.Replace('.', ',')) - Convert.ToDecimal(item.price.Replace('.', ',')) }
                    };
                    offerItem.Images = new OfferImage[] { new OfferImage { Url = item.mainImage } };

                    tempSSS.Add(offerItem);
                }
                responceOld.Items = new Models.SearchOffersResponseItems() {  Regular = tempSSS.ToArray() };


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
            CancellationToken cancellationToken,
            bool isBody)
        {
            Assure.ArgumentNotNull(categoryId, nameof(categoryId));
            var parameters = new Dictionary<string, string>
            {                
                ["category"] = categoryId,                
            };
            if (isBody)
            {
                parameters.Add("price_from", "400");
            }
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
            parameters["page"] = (offset/10).ToString();
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
            var o = GetOfferInit(id);
            return new OfferExtraData() { Description = o.Description, Parameters = o.Parameters };
        }

        public Offer GetOfferInit(string id)
        {
            var text = "";
            try
            {
                //using (var httpClient = new HttpClient())
                //{
                //    var httpResponse = httpClient.GetAsync("https://www.zoom-media.pw/allegro/parser.php?api_key=1DnGB5KoH5NF8vQ56&method=details&product_id="+id, CancellationToken.None);
                //    var responseBody = httpResponse.Result;
                //}

                HtmlWeb web = new HtmlWeb();
                //web.UseCookies = true;
                web.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 11_2_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36";
                web.PreRequest += (request) =>
                {
                    request.Headers.Add("Accept", "*/*");
                    return true;
                };
                // string s = code("http://api.scrapeup.com/?api_key=41f9cbb0dae6469ac6771b992224a0bd&url=http://allegro.pl/oferta/" + id+ "country_code=uk");
                //HtmlDocument doc = web.Load("http://api.scraperapi.com/?api_key=715027614bace80158a839d45247c02d&url=http://allegro.pl/oferta/" + id);
                //HttpClient httpClient = new HttpClient();
                //ScraperApiClient client = new ScraperApiClient("715027614bace80158a839d45247c02d", httpClient);

                //var sss = client.GetAsync("http://allegro.pl/oferta/" + id).Result;



                HtmlDocument doc = web.Load("https://spolshy.com.ua/product/" + id);
                //doc.LoadHtml(sss);
                text = doc.ParsedText;

                var divsDescNew = doc.DocumentNode.QuerySelectorAll(".product__description.description");
                var divNameNew = doc.DocumentNode.QuerySelectorAll("h1.product-box__title.product-box__title--second").FirstOrDefault().InnerText;

                string newDescNew = "";
                if (divsDescNew.Any())
                {
                    if (divsDescNew.Count() > 0)
                    {
                        foreach (var item in divsDescNew)
                        {
                            newDescNew += item.InnerHtml;
                        }
                    }
                }

                newDescNew = newDescNew.Replace("Описание запчасти (на польском)", "");

                var liParamsNew = doc.DocumentNode.QuerySelectorAll("td.describe__item");
                var imagesNew = doc.DocumentNode.QuerySelectorAll(".gallery-large__picture.swiper-slide picture source");
                OfferImage[] imagePathesNew = imagesNew.Where(x => x.Attributes["srcset"].Value.Contains("s800/")).Select(x => new OfferImage() { Url = x.Attributes["srcset"].Value }).ToArray();

                IList<OfferImage> successImages = new List<OfferImage>();
                foreach (var item in imagePathesNew)
                {
                    if (!successImages.Select(x => x.Url).Contains(item.Url))
                    {
                        successImages.Add(item);
                    }
                }
                IEnumerable<OfferImage> tempImage = successImages.Distinct();
                var divParamsInitNew = liParamsNew.Select(x => x.QuerySelectorAll("td").FirstOrDefault());
                var lineParamsDestNew = divParamsInitNew.Select(x => x.InnerText).ToList();


                var parametersNew = lineParamsDestNew.Select(x => GetParameterFromLine(x)).ToList();

                var descMultiNew = new MultiLanguageString()
                {
                    [Languages.PolishCode] = newDescNew,
                    [Languages.RussianCode] = newDescNew
                };

                return new Offer()
                {
                    Name = new MultiLanguageString()
                    {
                        [Languages.PolishCode] = divNameNew.ToString(),
                        [Languages.RussianCode] = divNameNew.ToString()
                    },
                    Description = descMultiNew,
                    Parameters = parametersNew,
                    Images = tempImage.ToArray()
                };


                //var divName = doc.DocumentNode.QuerySelector("meta[property='og:title']");
                //var name = divName.Attributes["content"].Value.ToString();

                //var divsDesc = doc.DocumentNode.QuerySelectorAll("div[data-box-name='Description'] div._2d49e_5pK0q div");

                //if (!divsDesc.Any())
                //{
                //    divsDesc = doc.DocumentNode.QuerySelectorAll("div[data-box-name='Description'] div._2d49e_5pK0q");
                //    if (!divsDesc.Any())
                //    {
                //        divsDesc = doc.DocumentNode.QuerySelectorAll("div[data-box-name='Description']");
                //    }
                //}
                //var tempdesc = doc.DocumentNode.QuerySelectorAll("div._2d49e_5pK0q");
                //var desc = divsDesc.Any() ? divsDesc[0].InnerHtml : "";
                //string newDesc = "";
                //if (divsDesc.Any()) {
                //    if (divsDesc.Count() > 0) {
                //        foreach (var item in divsDesc)
                //        {
                //            newDesc += item.InnerHtml;
                //        }
                //    }
                //}
                //var liParams = doc.DocumentNode.QuerySelectorAll("div[data-box-name='Parameters'] li div._f8818_3-1jj");

                //var tempPrice = doc.DocumentNode.QuerySelector("meta[itemprop='price']");
                //var priceStr = tempPrice.Attributes["content"].Value.ToString().Replace(",", ".");
                //decimal productPricePLN = decimal.Parse(priceStr, CultureInfo.InvariantCulture);
                //var images = doc.DocumentNode.QuerySelectorAll("img._b8e15_2LNko");//doc.DocumentNode.QuerySelectorAll("div[data-prototype-id='allegro.gallery'] img");

                //OfferImage[] imagePathes = images.Where(x => x.Attributes["src"] != null).Select(x => new OfferImage() { Url = x.Attributes["src"].Value.Replace("s128","original") }).ToArray();


                //if (!liParams.Any() && !divsDesc.Any())
                //{
                //    throw new AllegroPlRequestException("Error read https://allegro.pl/oferta/" + id + text);
                //}

                //var divParamsInit = liParams.Select(x => x.QuerySelectorAll("div").FirstOrDefault());
                //var lineParamsDest = divParamsInit.Where(x => x != null && x.InnerText.Contains(":")).Select(x => x.InnerText).ToList();


                //var parameters = lineParamsDest.Select(x => GetParameterFromLine(x)).ToList();

                //var descMulti = new MultiLanguageString()
                //{
                //    [Languages.PolishCode] = newDesc,
                //    [Languages.RussianCode] = newDesc
                //};

                //return new Offer()
                //{
                //    Name = new MultiLanguageString()
                //    {
                //        [Languages.PolishCode] = name,
                //        [Languages.RussianCode] = name
                //    },
                //    Description = descMulti,
                //    Parameters = parameters,
                //    Images = imagePathes,
                //    Price = productPricePLN
                //};
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
    }
}