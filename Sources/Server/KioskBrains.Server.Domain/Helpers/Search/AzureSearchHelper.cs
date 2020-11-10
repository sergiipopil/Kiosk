using Microsoft.Azure.Search;

namespace KioskBrains.Server.Domain.Helpers.Search
{
    public static class AzureSearchHelper
    {
        public static SearchIndexClient CreateSearchIndexClient(string serviceName, string apiKey)
        {
            var indexClient = new SearchIndexClient(new SearchCredentials(apiKey))
                {
                    SearchServiceName = serviceName,
                };
            return indexClient;
        }
    }
}