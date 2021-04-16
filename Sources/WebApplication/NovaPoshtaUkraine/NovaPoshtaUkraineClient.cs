using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebApplication.NovaPoshtaUkraine.Models;
using Newtonsoft.Json;
using System.IO;

namespace WebApplication.NovaPoshtaUkraine
{
    public class NovaPoshtaUkraineClient
    {
        #region Singleton

        public static NovaPoshtaUkraineClient Current { get; } = new NovaPoshtaUkraineClient();
        public static List<WarehouseSearchItem> NovaPoshtaCities = new List<WarehouseSearchItem>();
        //public string pathToNPCities = @"c:\temp\novaPoshtaCities.json";
        
        public string pathToNPCities = @"D:\Domains\bi-bi.com.ua\httpdocs\novaPoshtaCities.json";
        public NovaPoshtaUkraineClient()
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

        public async void GetAllWarehousesAsyncAndWriteToFile(CancellationToken cancellationToken)
        {
            for (int i = 1; i < 100; i++)
            {
                var request = new SettlemeentsSearchRequest(i);
                var response = await SendRequestAsync<WarehouseSearchResponse>(
                    "AddressGeneral/getSettlements/",
                    request,
                    cancellationToken);
                if (response.data.Length == 0)
                {
                    break;
                }             
                foreach (var item in response.data)
                {
                    NovaPoshtaCities.Add(item);
                }
            }
            //write to file all nova poshta cities
            File.WriteAllText(pathToNPCities, JsonConvert.SerializeObject(NovaPoshtaCities));
        }
        public async Task<WarehouseSearchItem[]> GetAllDepartmentsOfTheCity(CancellationToken cancellationToken, string cityName)
        {            
                var request = new WarehouseSearchRequest(cityName);
                var response = await SendRequestAsync<WarehouseSearchResponse>(
                    "AddressGeneral/getWarehouses/",
                    request,
                    cancellationToken);
            return response.data;
        }
        public List<WarehouseSearchItem> GetDataFromFile() {
            //read from file all nova poshta cities
            string readData = File.ReadAllText(pathToNPCities);
            return JsonConvert.DeserializeObject<List<WarehouseSearchItem>>(readData);
        }
        public async Task<AreasSearchItem[]> GetAllAreasAsync(CancellationToken cancellationToken)
        {
            var areas = GetCachedAreas();
            if (areas == null)
            {
                var request = new AreasSearchRequest(null, null, null);
                var response = await SendRequestAsync<AreasSearchResponse>(
                    "Address/getAreas/",
                    request,
                    cancellationToken);
                areas = response.data;
                CacheAreas(areas);
            }

            return areas;
        }

        #region Cache

        private DateTime? _warehousesCachedOn;

        private WarehouseSearchItem[] _cachedWarehouses;

        private AreasSearchItem[] _cachedAreas;

        private readonly object _cacheLocker = new object();

        private readonly TimeSpan _cacheInvalidationPeriod = TimeSpan.FromDays(1);

        private AreasSearchItem[] GetCachedAreas()
        {
            lock (_cacheLocker)
            {
                if (_warehousesCachedOn == null
                    || (DateTime.Now - _cacheInvalidationPeriod) > _warehousesCachedOn.Value)
                {
                    return null;
                }

                return _cachedAreas;
            }
        }

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

        private void CacheAreas(AreasSearchItem[] areas)
        {
            lock (_cacheLocker)
            {
                if (areas == null
                    || areas.Length == 0)
                {
                    return;
                }

                _warehousesCachedOn = DateTime.Now;
                _cachedAreas = areas;
            }
        }

        #endregion
    }
}
