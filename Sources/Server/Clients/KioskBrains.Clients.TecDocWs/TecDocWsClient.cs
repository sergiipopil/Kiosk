using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KioskBrains.Clients.KioskProxy;
using KioskBrains.Clients.TecDocWs.Models;
using KioskBrains.Common.Contracts;
using KioskBrains.Proxy.Common.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace KioskBrains.Clients.TecDocWs
{
    public class TecDocWsClient
    {
        private readonly KioskProxyClient _kioskProxyClient;

        private readonly TecDocWsClientSettings _settings;

        public TecDocWsClient(KioskProxyClient kioskProxyClient, IOptions<TecDocWsClientSettings> settings)
        {
            _kioskProxyClient = kioskProxyClient;
            _settings = settings.Value;

            Assure.ArgumentNotNull(_settings, nameof(_settings));
        }

        private async Task<TValue[]> SendRequestAsync<TValue>(string actionPath)
        {
            Assure.ArgumentNotNull(actionPath, nameof(actionPath));
            if (!actionPath.StartsWith("/"))
            {
                actionPath = "/" + actionPath;
            }

            var proxyRequest = new PassRequest()
                {
                    Url = _settings.ApiUrl + actionPath,
                    Method = "GET",
                    RequestHeaders = new Dictionary<string, string>()
                        {
                            ["Accept"] = "application/json",
                        },
                    ContentHeaders = new Dictionary<string, string>(),
                };

            var apiResponse = await _kioskProxyClient.PassAsync(proxyRequest);

            if (apiResponse.StatusCode != 200)
            {
                throw new TecDocWsRequestException($"Request failed (status: {apiResponse.StatusCode}, request: '{actionPath}').");
            }

            if (string.IsNullOrEmpty(apiResponse.ContentBody))
            {
                return new TValue[0];
            }

            try
            {
                return JsonConvert.DeserializeObject<TValue[]>(apiResponse.ContentBody, new BrokenJsonArrayConverter());
            }
            catch (Exception ex)
            {
                throw new TecDocWsRequestException($"Response deserialization exception (request: '{actionPath}').", ex);
            }
        }

        public async Task<SearchByVinRecord[]> SearchByVinAsync(string vinCode)
        {
            if (string.IsNullOrWhiteSpace(vinCode))
            {
                return new SearchByVinRecord[0];
            }

            var actionBuilder = new StringBuilder("/vin/");

            actionBuilder.Append(vinCode);

            return await SendRequestAsync<SearchByVinRecord>(actionBuilder.ToString());
        }

        public async Task<SearchByArticleNumberRecord[]> SearchByArticleNumberAsync(
            string articleNumber,
            int? brandId = null,
            ArticleNumberTypeEnum numberType = ArticleNumberTypeEnum.AnyNumber,
            bool exact = true)
        {
            if (string.IsNullOrWhiteSpace(articleNumber))
            {
                return new SearchByArticleNumberRecord[0];
            }

            var actionBuilder = new StringBuilder("/search/");

            actionBuilder.Append(articleNumber);

            actionBuilder.Append("/");
            actionBuilder.Append(brandId?.ToString() ?? "null");

            actionBuilder.Append("/");
            actionBuilder.Append((int)numberType);

            actionBuilder.Append("/");
            actionBuilder.Append(exact ? 1 : 0);

            return await SendRequestAsync<SearchByArticleNumberRecord>(actionBuilder.ToString());
        }

        public Task<Manufacturer[]> GetManufacturersAsync(CarTypeEnum carType, bool popularOnly)
        {
            var actionBuilder = new StringBuilder("/manufacturers/");
            switch (carType)
            {
                case CarTypeEnum.Car:
                    actionBuilder.Append("P");
                    break;
                case CarTypeEnum.Truck:
                    actionBuilder.Append("O");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(carType), carType, null);
            }

            actionBuilder.Append("/ru/");

            actionBuilder.Append(popularOnly ? "1" : "0");

            return SendRequestAsync<Manufacturer>(actionBuilder.ToString());
        }

        public Task<Model[]> GetModelsAsync(CarTypeEnum carType, int manufacturerId)
        {
            var actionBuilder = new StringBuilder("/models/");
            switch (carType)
            {
                case CarTypeEnum.Car:
                    actionBuilder.Append("P");
                    break;
                case CarTypeEnum.Truck:
                    actionBuilder.Append("O");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(carType), carType, null);
            }

            actionBuilder.Append($"/{manufacturerId}");

            return SendRequestAsync<Model>(actionBuilder.ToString());
        }

        [Obsolete("Not supported at the moment.")]
        public async Task<Model> GetModelAsync(int modelId)
        {
            var models = await SendRequestAsync<Model>($"/model/{modelId}");
            return models.FirstOrDefault();
        }

        public async Task<ModelType[]> GetModelTypesAsync(CarTypeEnum carType, int manufacturerId, int modelId)
        {
            var actionBuilder = new StringBuilder("/modelstypes/");
            switch (carType)
            {
                case CarTypeEnum.Car:
                    actionBuilder.Append("P");
                    break;
                case CarTypeEnum.Truck:
                    actionBuilder.Append("O");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(carType), carType, null);
            }

            actionBuilder.Append($"/{manufacturerId}/{modelId}");

            var modelTypes = await SendRequestAsync<ModelType>(actionBuilder.ToString());

            // sometimes empty cars are received
            return modelTypes
                .Where(x => x.CarId != 0)
                .ToArray();
        }

        public async Task<ModelType[]> GetModelTypesAsync(int[] modelTypeIds)
        {
            if (modelTypeIds == null
                || modelTypeIds.Length == 0)
            {
                return new ModelType[0];
            }

            var actionBuilder = new StringBuilder("/vehicles/");

            actionBuilder.Append(string.Join("/", modelTypeIds));

            return await SendRequestAsync<ModelType>(actionBuilder.ToString());
        }

        public Task<Category[]> GetCategoriesAsync(CarTypeEnum? carType, int? modelTypeId, int? parentCategoryId, bool childNodes = false)
        {
            if (carType != null)
            {
                if (modelTypeId == null)
                {
                    throw new ArgumentNullException(nameof(modelTypeId), $"Required if {nameof(carType)} is passed.");
                }
            }

            var actionBuilder = new StringBuilder("/categories/");
            switch (carType)
            {
                case CarTypeEnum.Car:
                    actionBuilder.Append($"P/{modelTypeId}");
                    break;
                case CarTypeEnum.Truck:
                    actionBuilder.Append($"O/{modelTypeId}");
                    break;
                case null:
                    actionBuilder.Append("U/null");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(carType), carType, null);
            }

            actionBuilder.Append($"/{parentCategoryId?.ToString() ?? "null"}");

            actionBuilder.Append("/");
            actionBuilder.Append(childNodes ? "1" : "0");

            return SendRequestAsync<Category>(actionBuilder.ToString());
        }

        public Task<ArticleCompactInfo[]> GetArticlesCompactInfoAsync(CarTypeEnum carType, int modelTypeId, int categoryId)
        {
            var actionBuilder = new StringBuilder("/articles/");
            switch (carType)
            {
                case CarTypeEnum.Car:
                    actionBuilder.Append("P");
                    break;
                case CarTypeEnum.Truck:
                    actionBuilder.Append("O");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(carType), carType, null);
            }

            actionBuilder.Append($"/{modelTypeId}/{categoryId}");

            actionBuilder.Append("/compact");

            return SendRequestAsync<ArticleCompactInfo>(actionBuilder.ToString());
        }

        public Task<ArticleCompactInfo[]> GetArticlesExtendedInfoAsync(CarTypeEnum carType, int modelTypeId, int categoryId)
        {
            var actionBuilder = new StringBuilder("/articles/");
            switch (carType)
            {
                case CarTypeEnum.Car:
                    actionBuilder.Append("P");
                    break;
                case CarTypeEnum.Truck:
                    actionBuilder.Append("O");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(carType), carType, null);
            }

            actionBuilder.Append($"/{modelTypeId}/{categoryId}");

            actionBuilder.Append("/extended");

            return SendRequestAsync<ArticleCompactInfo>(actionBuilder.ToString());
        }

        public async Task<ArticleExtendedInfo[]> GetArticlesExtendedInfoAsync(long[] articleIds)
        {
            if (articleIds == null
                || articleIds.Length == 0)
            {
                return new ArticleExtendedInfo[0];
            }

            var actionBuilder = new StringBuilder("/article/");

            actionBuilder.Append(string.Join("/", articleIds));

            return await SendRequestAsync<ArticleExtendedInfo>(actionBuilder.ToString());
        }

        public async Task<DocumentData> GetDocumentAsync(string documentId, int documentTypeId)
        {
            var models = await SendRequestAsync<DocumentData>($"/document/{documentId}/{documentTypeId}");
            return models.FirstOrDefault();
        }
    }
}