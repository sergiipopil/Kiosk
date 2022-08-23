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
using Microsoft.AspNetCore.Mvc;
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
        IList<string> rulevoe = new List<string>() { "PRZEKŁADNIA KIER", "MAGLOWNICA PRZEKŁADNIA UKŁAD DRĄŻEK", "PRZEKŁADNIA UKŁAD MAGLOWNICA", "przekładnia kierownicza maglownica", "PRZEKLADNIA MAGLOWNICA UKŁAD", "MAGLOWNICA PRZEKŁADNIA UKŁAD",
                "PRZEKŁADNIA MAGLOWNICA UKŁAD", "PRZEKŁADNIA MAGLOWNICA WSPOMAGANIE","MAGLOWNICA PRZEKŁADNIA KIEROWNICZA", "PRZEKŁADNIA KIEROWNICZA ANGLIA", "WSPOMAGANIE KIEROWNICY", "PRZEKŁADNIA MAGLOWNICA", "MAGLOWNICA PRZEKŁADNIA", "MAGLOWNICA PRZEKŁADN", "PRZEKŁADNIA KIEROWNICZA", "PRZEKLADNIA MAGLOWNICA", "PRZKLADNIA KIEROWNICZA",
                "PRZEKLADNIA KIEROWNICZA", "PRZEKŁADNIA UKŁAD", "MAGLOWNICA UKŁAD", "Maglownica", "PRZEKŁADNIA", "PRZEKLADNIA" };
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
            string responseBodyDetail;
            if (queryParameters["page"] == "0")
            {
                queryParameters["page"] = "1";
            }
            try
            {
                var uriBuilder = new UriBuilder($"http://38.242.150.247/allegro/parser.php");
                if (queryParameters?.Count > 0)
                {
                    uriBuilder.Query = string.Join(
                        "&",
                        queryParameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
                }
                var handler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                //using (var httpClient = new HttpClient(handler))
                //{
                //    var httpResponse = await httpClient.GetAsync(uriBuilder.Uri, cancellationToken);
                //    responseBody = await httpResponse.Content.ReadAsStringAsync();
                //    if (!httpResponse.IsSuccessStatusCode)
                //    {
                //        throw new AllegroPlRequestException($"Request to API failed, action {action}, response code {(int)httpResponse.StatusCode}, body: {responseBody}");
                //    }
                //}
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    using (var client = new HttpClient(httpClientHandler))
                    {
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
                string categoryId = "";
                //Models.Offer[] offersParse = new Models.Offer[];
                if (parserResponce.products != null)
                {
                    foreach (var item in parserResponce.products)
                    {
                        if (queryParameters["category"] == "3")
                        {
                            using (var httpClientHandler = new HttpClientHandler())
                            {
                                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                                using (var client = new HttpClient(httpClientHandler))
                                {
                                    client.DefaultRequestHeaders.Clear();
                                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
                                    client.DefaultRequestHeaders.Add("Accept", "application/vnd.allegro.public.v1+json");
                                    var httpResponseDetail = await client.GetAsync("http://38.242.150.247/allegro/parser.php?api_key=Ah4Yg2nzA6kQ3EmPvUA7bN&method=details&product_id=" + item.id, cancellationToken);
                                    responseBodyDetail = await httpResponseDetail.Content.ReadAsStringAsync();
                                    if (!httpResponseDetail.IsSuccessStatusCode)
                                    {
                                        throw new AllegroPlRequestException($"Request to API failed, action {action}, response code {(int)httpResponseDetail.StatusCode}, body: {responseBody}");
                                    }
                                    var parserResponceDetail = JsonConvert.DeserializeObject<ParserResponseDetails>(responseBodyDetail);
                                    queryParameters["category"] = parserResponceDetail.category_path.Last().id;
                                }
                            }
                        }
                        Models.Offer offerItem = new Models.Offer();
                        offerItem.Id = item.id;
                        offerItem.Name = item.title;
                        offerItem.SellingMode = new Models.OfferSellingMode()
                        {
                            Price = new Models.OfferPrice() { Amount = Convert.ToDecimal(item.price.Replace('.', ',')) }
                        };
                        offerItem.Category = new Models.OfferCategory()
                        {
                            Id = queryParameters["category"]
                        };
                        if (item.price_with_delivery != null)
                        {
                            offerItem.Delivery = new Models.OfferDelivery()
                            {
                                LowestPrice = new Models.OfferPrice() { Amount = Convert.ToDecimal(item.price_with_delivery.Replace('.', ',')) - Convert.ToDecimal(item.price.Replace('.', ',')) }
                            };
                        }
                        offerItem.Images = new OfferImage[] { new OfferImage { Url = item.mainImage } };

                        tempSSS.Add(offerItem);
                    }
                }
                responceOld.ReallyTotalCount = parserResponce.totalCount;
                responceOld.Items = new Models.SearchOffersResponseItems() { Regular = tempSSS.ToArray() };


                return responceOld;
            }
            catch (Exception ex)
            {
                throw new AllegroPlRequestException("Bad format of API response.", ex);
            }
        }
        public class ParserDetailCategory
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
        }
        public class ParserResponseDetails
        {
            public string success { get; set; }
            public string id { get; set; }
            public string title { get; set; }
            public string url { get; set; }
            public string active { get; set; }
            public string availableQuantity { get; set; }
            public string price { get; set; }
            public string price_with_delivery { get; set; }
            public string currency { get; set; }
            public string seller_id { get; set; }
            public string seller_rating { get; set; }
            public IList<ParserDetailCategory> category_path { get; set; }
            //public IEnumerable<string> specifications { get; set; }
            //public IEnumerable<string> images { get; set; }
            //public IEnumerable<string> description { get; set; }
            //public IEnumerable<string> compatibility { get; set; }
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
            if (categoryId == "50825" || categoryId == "50838" || categoryId == "312565" || categoryId == "50873")
            {
                parameters.Add("price_from", "100");
            }
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
                    sortingValue = "";
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
            parameters["page"] = (offset / 40).ToString();
            parameters["api_key"] = "Ah4Yg2nzA6kQ3EmPvUA7bN";
            parameters["method"] = "search";

            var action = "/offers/listing";
            var response = await GetAsync<Models.SearchOffersResponse>(action, parameters, cancellationToken);

            if (response.Items != null && categoryId == "250847")
            {
                foreach (var item in response.Items.Regular)
                {
                    foreach (var itemSub in rulevoe)
                    {
                        item.Name = item.Name.ToLower().Replace(itemSub.ToLower(), "рулевая рейка");
                    }

                }
            }
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
            public decimal? seller_rating { get; set; }
            public Specifications specifications { get; set; }
            public IEnumerable<ImagesParser> images { get; set; }
            public IEnumerable<CategoryParser> category_path { get; set; }
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
        public class CategoryParser
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }

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
                    var httpResponse = await client.GetAsync("http://38.242.150.247/allegro/parser.php?api_key=Ah4Yg2nzA6kQ3EmPvUA7bN&method=details&product_id=" + id, CancellationToken.None);
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
        public Offer GetOfferInit(string id)
        {
            var text = "";
            try
            {
                var res = GetAsyncDetails(id).Result;

                List<OfferParameter> listParameters = new List<OfferParameter>();

                foreach (var item in res.specifications.Parametry)
                {
                    if (item.Key != "Faktura")
                    {
                        listParameters.Add(new OfferParameter()
                        {
                            Name = new MultiLanguageString() { [Languages.PolishCode] = item.Key },
                            Value = new MultiLanguageString() { [Languages.PolishCode] = item.Value.ToString() }
                        });
                    }
                }
                var temp = res.description.sections.Select(x => x.items).ToList();
                var resTemp = "";
                foreach (var item in temp)
                {
                    if (item.Where(x => x.type == "TEXT").FirstOrDefault() != null)
                    {
                        resTemp = resTemp + item.Where(x => x.type == "TEXT").FirstOrDefault().content;
                    }
                }
                var descMultiNew = new MultiLanguageString()
                {
                    [Languages.PolishCode] = resTemp
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

                if (res.price.Contains("."))
                {
                    res.price = res.price.Replace('.', ',');
                }
                if (res.price_with_delivery.Contains("."))
                {
                    res.price_with_delivery = res.price_with_delivery.Replace('.', ',');
                }

                return new Offer()
                {
                    CategoryId = res.category_path.Count() > 0 ? res.category_path.Last().id : "",
                    Name = new MultiLanguageString()
                    {
                        [Languages.PolishCode] = res.title
                    },
                    Description = descMultiNew,
                    Parameters = listParameters,
                    Images = tempImage.ToArray(),
                    AvailableQuantity = res.availableQuantity,
                    SellerRating = res.seller_rating,
                    Price = Convert.ToDecimal(res.price),
                    DeliveryOptions = new[]
                    {
                        new DeliveryOption()
                        {
                            Price = Convert.ToDecimal(res.price_with_delivery) - Convert.ToDecimal(res.price)
                        },
                    }

                };


                //HtmlWeb web = new HtmlWeb();

                //HtmlDocument doc = web.Load("https://spolshy.com.ua/product/" + id);
                //text = doc.ParsedText;

                //var divsDescNew = doc.DocumentNode.QuerySelectorAll(".product__description.description");
                //var divNameNew = doc.DocumentNode.QuerySelectorAll("h1.product-box__title.product-box__title--second").FirstOrDefault().InnerText;

                //string newDescNew = "";
                //if (divsDescNew.Any())
                //{
                //    if (divsDescNew.Count() > 0)
                //    {
                //        foreach (var item in divsDescNew)
                //        {
                //            newDescNew += item.InnerHtml;
                //        }
                //    }
                //}

                //newDescNew = newDescNew.Replace("Описание запчасти (на польском)", "");

                //var liParamsNew = doc.DocumentNode.QuerySelectorAll("td.describe__item");
                //var imagesNew = doc.DocumentNode.QuerySelectorAll(".gallery-large__picture.swiper-slide picture source");
                //OfferImage[] imagePathesNew = imagesNew.Where(x => x.Attributes["data-srcset"].Value.Contains("s800/")).Select(x => new OfferImage() { Url = x.Attributes["data-srcset"].Value }).ToArray();

                //IList<OfferImage> successImages = new List<OfferImage>();
                //foreach (var item in imagePathesNew)
                //{
                //    if (!successImages.Select(x => x.Url).Contains(item.Url))
                //    {
                //        successImages.Add(item);
                //    }
                //}
                //IEnumerable<OfferImage> tempImage = successImages.Distinct();
                //var divParamsInitNew = liParamsNew.Select(x => x.QuerySelectorAll("td").FirstOrDefault());
                //var lineParamsDestNew = divParamsInitNew.Select(x => x.InnerText).ToList();


                //var parametersNew = lineParamsDestNew.Select(x => GetParameterFromLine(x)).ToList();

                //var descMultiNew = new MultiLanguageString()
                //{
                //    [Languages.PolishCode] = newDescNew.ToString(),
                //    [Languages.RussianCode] = newDescNew.ToString()
                //};

                //return new Offer()
                //{
                //    Name = new MultiLanguageString()
                //    {
                //        [Languages.PolishCode] = divNameNew.ToString(),
                //        [Languages.RussianCode] = divNameNew.ToString()
                //    },
                //    Description = descMultiNew,
                //    Parameters = parametersNew,
                //    Images = tempImage.ToArray()
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