﻿using System;
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
    internal class RestClient
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

        private async Task<TResponse> GetAsync<TResponse>(
            string action,
            Dictionary<string, string> queryParameters,
            CancellationToken cancellationToken)
            where TResponse : new()
        {
            await EnsureAuthSessionAsync(cancellationToken);

            string responseBody;
            try
            {
                var uriBuilder = new UriBuilder($"https://api.allegro.pl{action}");
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
                var response = JsonConvert.DeserializeObject<TResponse>(responseBody);
                return response;
            }
            catch (Exception ex)
            {
                throw new AllegroPlRequestException("Bad format of API response.", ex);
            }
        }

        private const string StateFilterId = "parameter.11323";

        public async Task<Models.SearchOffersResponse> SearchOffersAsync(
            string phrase,
            string categoryId,
            OfferStateEnum state,
            OfferSortingEnum sorting,
            int offset,
            int limit,
            CancellationToken cancellationToken)
        {
            Assure.ArgumentNotNull(categoryId, nameof(categoryId));

            var parameters = new Dictionary<string, string>
            {
                ["searchMode"] = "REGULAR", // by title only
                ["category.id"] = categoryId,
                ["sellingMode.format"] = "BUY_NOW", // exclude auctions (sellingMode.format=AUCTION)
            };

            if (!string.IsNullOrEmpty(phrase))
            {
                parameters["phrase"] = phrase;
            }

            if (state != OfferStateEnum.All)
            {
                string stateFilterValue;
                switch (state)
                {
                    case OfferStateEnum.New:
                        stateFilterValue = "11323_1";
                        break;
                    case OfferStateEnum.Used:
                        stateFilterValue = "11323_2";
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

                parameters[StateFilterId] = stateFilterValue;
            }

            string sortingValue;
            switch (sorting)
            {
                case OfferSortingEnum.Relevance:
                    sortingValue = "-relevance";
                    break;
                case OfferSortingEnum.PriceAsc:
                    sortingValue = "price";
                    break;
                case OfferSortingEnum.PriceDesc:
                    sortingValue = "-price";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sorting), sorting, null);
            }

            parameters["sort"] = sortingValue;

            parameters["offset"] = offset.ToString();
            parameters["limit"] = limit.ToString();

            var action = "/offers/listing";
            var response = await GetAsync<Models.SearchOffersResponse>(action, parameters, cancellationToken);
            if (response.Items == null
                || response.SearchMeta == null)
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
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load("https://allegro.pl/oferta/" + id);
                text = doc.ParsedText;
                var divsDesc = doc.DocumentNode.QuerySelectorAll("div[data-box-name='Description'] div._2d49e_5pK0q div");

                if (!divsDesc.Any())
                {
                    divsDesc = doc.DocumentNode.QuerySelectorAll("div[data-box-name='Description'] div._2d49e_5pK0q");
                    if (!divsDesc.Any())
                    {
                        divsDesc = doc.DocumentNode.QuerySelectorAll("div[data-box-name='Description']");
                    }
                }

                var desc = divsDesc.Any() ? divsDesc[0].InnerHtml : "";


                var liParams = doc.DocumentNode.QuerySelectorAll("div[data-box-name='Parameters'] li div._f8818_3-1jj");

                if (!liParams.Any() && !divsDesc.Any())
                {
                    throw new AllegroPlRequestException("Error read https://allegro.pl/oferta/" + id + text);
                }

                var divParamsInit = liParams.Select(x => x.QuerySelectorAll("div").FirstOrDefault());
                var lineParamsDest = divParamsInit.Where(x => x != null && x.InnerText.Contains(":")).Select(x => x.InnerText).ToList();


                var parameters = lineParamsDest.Select(x => GetParameterFromLine(x)).ToList();

                var descMulti = new MultiLanguageString()
                {
                    [Languages.PolishCode] = desc,
                    [Languages.RussianCode] = desc
                };

                return new OfferExtraData()
                {
                    Description = descMulti,
                    Parameters = parameters
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
    }
}