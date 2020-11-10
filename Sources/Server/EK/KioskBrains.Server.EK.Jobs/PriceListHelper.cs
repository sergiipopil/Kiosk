using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Common.Log;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;

namespace KioskBrains.Server.EK.Jobs
{
    internal static class PriceListHelper
    {
        public static async Task CleanExpiredRecordsAsync(
            SearchIndexClient searchIndexClient,
            EkProductSourceEnum productSource,
            long lastUpdateUtcTimestamp,
            PriceListSynchronizationStatistics statistics,
            ILogger logger)
        {
            const int BatchSize = 1_000;
            string lastProcessedKey = null;

            while (true)
            {
                var filterBuilder = new StringBuilder();
                filterBuilder.Append($"source eq {(int)productSource} and updatedOnUtcTimestamp ne {lastUpdateUtcTimestamp}");
                if (lastProcessedKey != null)
                {
                    filterBuilder.Append($" and key gt '{lastProcessedKey}'");
                }

                var searchParameters = new SearchParameters()
                    {
                        Top = BatchSize,
                        Filter = filterBuilder.ToString(),
                        Select = new List<string>()
                            {
                                "key"
                            },
                        OrderBy = new List<string>()
                            {
                                "key asc",
                            },
                    };

                var searchResult = await searchIndexClient.Documents.SearchAsync(null, searchParameters);
                var documents = searchResult.Results
                    .Select(x => x.Document)
                    .ToArray();

                if (documents.Length > 0)
                {
                    // remove records
                    var indexBatch = IndexBatch.New(documents.Select(x => IndexAction.Merge(x)));
                    try
                    {
                        await searchIndexClient.Documents.IndexAsync(indexBatch);
                    }
                    catch (IndexBatchException ex)
                    {
                        var reasons = string.Join(", ", ex.IndexingResults
                            .Where(x => !x.Succeeded)
                            .Select(x => $"{x.Key}-{x.ErrorMessage}"));
                        var message = $"Failed to index products: {reasons}.";

                        throw new InvalidOperationException(message);
                    }

                    // use last processed key as additional filter since indexing takes some time
                    lastProcessedKey = (string)documents.Last()["key"];

                    logger.LogInformation(LoggingEvents.Synchronization, $"Deleted: {documents.Length}.");
                    statistics.Removed += documents.Length;
                }
                else
                {
                    logger.LogInformation(LoggingEvents.Synchronization, "No expired records.");

                    break;
                }
            }
        }
    }
}