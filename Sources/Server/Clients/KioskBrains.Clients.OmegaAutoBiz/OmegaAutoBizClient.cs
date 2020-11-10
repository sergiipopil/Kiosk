using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Clients.KioskProxy;
using KioskBrains.Clients.OmegaAutoBiz.Models;
using KioskBrains.Common.Contracts;
using KioskBrains.Proxy.Common.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace KioskBrains.Clients.OmegaAutoBiz
{
    public class OmegaAutoBizClient
    {
        private static readonly JsonSerializerSettings OmegaJsonSerializerSettings = new JsonSerializerSettings();

        private readonly KioskProxyClient _kioskProxyClient;

        private readonly OmegaAutoBizClientSettings _settings;

        public OmegaAutoBizClient(KioskProxyClient kioskProxyClient, IOptions<OmegaAutoBizClientSettings> settings)
        {
            _kioskProxyClient = kioskProxyClient;
            _settings = settings.Value;

            Assure.ArgumentNotNull(_settings, nameof(_settings));
        }

        private async Task<TResponse> SendRequestAsync<TResponse>(
            string actionPath,
            Dictionary<string, object> request,
            bool throwOnError)
            where TResponse : ResponseWrapperBase
        {
            Assure.ArgumentNotNull(actionPath, nameof(actionPath));
            Assure.ArgumentNotNull(request, nameof(request));

            if (!actionPath.StartsWith("/"))
            {
                actionPath = "/" + actionPath;
            }

            request["Key"] = _settings.Key;

            string requestBodyJson;
            try
            {
                requestBodyJson = JsonConvert.SerializeObject(request, OmegaJsonSerializerSettings);
            }
            catch (Exception ex)
            {
                throw new OmegaAutoBizRequestException($"Request serialization exception (action: '{actionPath}').", ex);
            }

            var proxyRequest = new PassRequest()
                {
                    Url = _settings.ApiUrl + actionPath,
                    Method = "POST",
                    RequestHeaders = new Dictionary<string, string>()
                        {
                            ["Accept"] = "application/json",
                        },
                    ContentHeaders = new Dictionary<string, string>()
                        {
                            ["Content-Type"] = "application/json; charset=utf-8",
                        },
                    ContentBody = requestBodyJson,
                };

            var apiResponse = await _kioskProxyClient.PassAsync(proxyRequest);

            if (apiResponse.StatusCode != 200)
            {
                throw new OmegaAutoBizRequestException($"Request failed (status: {apiResponse.StatusCode}, action: '{actionPath}').");
            }

            if (string.IsNullOrEmpty(apiResponse.ContentBody))
            {
                return null;
            }

            try
            {
                var response = JsonConvert.DeserializeObject<TResponse>(apiResponse.ContentBody, OmegaJsonSerializerSettings);
                if (!response.IsSuccess
                    && throwOnError)
                {
                    var errors = response.Errors?
                                     .Select(x => $"{x.Error}-{x.Description}")
                                     .ToArray()
                                 ?? new string[0];
                    throw new OmegaAutoBizRequestException($"Request failed, errors: {string.Join(", ", errors)} (action: '{actionPath}').");
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new OmegaAutoBizRequestException($"Response deserialization exception (action: '{actionPath}').", ex);
            }
        }

        public Task<CollectionResponseWrapper<ProductSearchRecord>> ProductSearchAsync(
            string searchPhrase,
            int from,
            int count,
            bool throwOnError = true)
        {
            return SendRequestAsync<CollectionResponseWrapper<ProductSearchRecord>>(
                "/product/search",
                new Dictionary<string, object>()
                    {
                        ["SearchPhrase"] = searchPhrase,
                        ["From"] = from,
                        ["Count"] = count,
                    },
                throwOnError);
        }

        public Task<DetailsResponseWrapper<ProductDetails>> ProductDetailsAsync(
            long productId,
            bool throwOnError = true)
        {
            return SendRequestAsync<DetailsResponseWrapper<ProductDetails>>(
                "/product/details",
                new Dictionary<string, object>()
                    {
                        ["ProductId"] = productId,
                    },
                throwOnError);
        }

        public Task<CollectionResponseWrapper<ProductSearchRecord>> ProductPriceListAsync(
            int from,
            int count,
            bool throwOnError = true)
        {
            return SendRequestAsync<CollectionResponseWrapper<ProductSearchRecord>>(
                "/product/pricelist/paged",
                new Dictionary<string, object>()
                    {
                        // from works as a page index
                        ["From"] = GetPageIndex(from, count),
                        ["Count"] = count,
                        ["IsPrepay"] = true,
                    },
                throwOnError);
        }

        private int GetPageIndex(int from, int count)
        {
            return from/count;
        }

        /// <summary>
        /// Not supported by reverse proxy at the moment (big delay triggers timeout).
        /// </summary>
        public async Task<string> ProductPriceListCsvAsync()
        {
            var actionPath = "/product/pricelist/csv";
            var requestBodyJson = JsonConvert.SerializeObject(
                new
                    {
                        Key = _settings.Key,
                    },
                OmegaJsonSerializerSettings);

            var proxyRequest = new PassRequest()
                {
                    Url = _settings.ApiUrl + actionPath,
                    Method = "POST",
                    RequestHeaders = new Dictionary<string, string>()
                        {
                            ["Accept"] = "text/csv",
                        },
                    ContentHeaders = new Dictionary<string, string>()
                        {
                            ["Content-Type"] = "application/json; charset=utf-8",
                        },
                    ContentBody = requestBodyJson,
                };

            var apiResponse = await _kioskProxyClient.PassAsync(proxyRequest);

            if (apiResponse.StatusCode != 200)
            {
                throw new OmegaAutoBizRequestException($"Request failed (status: {apiResponse.StatusCode}, action: '{actionPath}').");
            }

            return apiResponse.ContentBody;
        }
    }
}