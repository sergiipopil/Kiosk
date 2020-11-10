using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Server.EK.Common.Search.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Options;

namespace KioskBrains.Server.EK.Common.Search
{
    public class EkSearchManagementClient
    {
        private readonly EkSearchManagementSettings _searchManagementSettings;

        public EkSearchManagementClient(
            IOptions<EkSearchManagementSettings> searchManagementSettings)
        {
            _searchManagementSettings = searchManagementSettings.Value;

            Assure.ArgumentNotNull(_searchManagementSettings, nameof(_searchManagementSettings));
        }

        public string ProductsIndexName => _searchManagementSettings.ProductsIndexName;

        public SearchServiceClient CreateSearchServiceClient()
        {
            var serviceClient = new SearchServiceClient(
                _searchManagementSettings.ServiceName,
                new SearchCredentials(_searchManagementSettings.AdminKey));
            return serviceClient;
        }

        public SearchIndexClient CreateSearchIndexClient()
        {
            var indexClient = new SearchIndexClient(new SearchCredentials(_searchManagementSettings.AdminKey))
                {
                    SearchServiceName = _searchManagementSettings.ServiceName
                };
            return indexClient;
        }

        public async Task CreateProductIndexIfNotExistAsync()
        {
            var indexName = _searchManagementSettings.ProductsIndexName;
            using (var serviceClient = CreateSearchServiceClient())
            {
                // check if index exists
                if (serviceClient.Indexes.Exists(indexName))
                {
                    return;
                }

                var indexDefinition = new Index()
                    {
                        Name = indexName,
                        Fields = FieldBuilder.BuildForType<IndexProduct>(),
                        Suggesters = IndexProduct.GetSuggesters(),
                        ScoringProfiles = IndexProduct.GetScoringProfiles(),
                    };

                await serviceClient.Indexes.CreateAsync(indexDefinition);
            }
        }
    }
}