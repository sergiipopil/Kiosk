using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebApplication.NovaPoshtaUkraine.Models;
using Newtonsoft.Json;

namespace WebApplication.NovaPoshtaUkraine
{
    public class NovaPoshtaUkraineClient
    {
        #region Singleton

        public static NovaPoshtaUkraineClient Current { get; } = new NovaPoshtaUkraineClient();

        private NovaPoshtaUkraineClient()
        {
        }

        #endregion

        // TODO: accept from server (expiration period - 1 year)
        private const string ApiKey = "376986cb6801ed635b06e3bae756dba8";

        private const string ApiUrl = "https://api.novaposhta.ua";

        private const string Format = "json";

        private const string ApiVersion = "v2.0";

        private async Task<TResponse> SendRequestAsync<TResponse>(
            string path,
            BaseSearchRequest request,
            CancellationToken cancellationToken)
            where TResponse : BaseSearchResponse
        {
            string responseBody;
            try
            {
                request.apiKey = ApiKey;

                var requestJson = JsonConvert.SerializeObject(request);
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var httpResponse = await client.PostAsync(
                        $"{ApiUrl}/{ApiVersion}/{Format}/{path}",
                        requestContent,
                        cancellationToken);

                    responseBody = await httpResponse.Content.ReadAsStringAsync();
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new NovaPoshtaUkraineClientException($"Request to API failed, response code {(int)httpResponse.StatusCode}, body: {responseBody}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (NovaPoshtaUkraineClientException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NovaPoshtaUkraineClientException("API request failed.", ex);
            }

            TResponse response;
            try
            {
                response = JsonConvert.DeserializeObject<TResponse>(responseBody);
            }
            catch (Exception ex)
            {
                throw new NovaPoshtaUkraineClientException("Bad format of API response.", ex);
            }

            if (response == null)
            {
                throw new NovaPoshtaUkraineClientException("API response is null.");
            }

            if (!response.success)
            {
                var errors = response.errors ?? new string[0];
                throw new NovaPoshtaUkraineClientException($"API response is not successful, errors: {string.Join(", ", errors)}.");
            }

            return response;
        }

        public async Task<WarehouseSearchItem[]> GetAllWarehousesAsync(CancellationToken cancellationToken)
        {
            var warehouses = GetCachedWarehouses();
            if (warehouses == null)
            {
                var request = new WarehouseSearchRequest(null, null, null);
                var response = await SendRequestAsync<WarehouseSearchResponse>(
                    "AddressGeneral/getWarehouses/",
                    request,
                    cancellationToken);
                warehouses = response.data;
                CacheWarehouses(warehouses);
            }

            return warehouses;
        }

        #region Cache

        private DateTime? _warehousesCachedOn;

        private WarehouseSearchItem[] _cachedWarehouses;

        private readonly object _cacheLocker = new object();

        private readonly TimeSpan _cacheInvalidationPeriod = TimeSpan.FromDays(1);

        private WarehouseSearchItem[] GetCachedWarehouses()
        {
            lock (_cacheLocker)
            {
                if (_warehousesCachedOn == null
                    || (DateTime.Now - _cacheInvalidationPeriod) > _warehousesCachedOn.Value)
                {
                    return null;
                }

                return _cachedWarehouses;
            }
        }

        private void CacheWarehouses(WarehouseSearchItem[] warehouses)
        {
            lock (_cacheLocker)
            {
                if (warehouses == null
                    || warehouses.Length == 0)
                {
                    return;
                }

                _warehousesCachedOn = DateTime.Now;
                _cachedWarehouses = warehouses;
            }
        }

        #endregion
    }
}
