using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.TecDocWs;
using KioskBrains.Clients.TecDocWs.Models;
using KioskBrains.Common.EK.Helpers;
using KioskBrains.Server.Common.Log;
using KioskBrains.Server.Common.Notifications;
using KioskBrains.Server.Common.Storage;
using KioskBrains.Server.EK.Common.Cache;
using KioskBrains.Server.EK.Common.Search;
using KioskBrains.Server.EK.Common.Search.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;

namespace KioskBrains.Server.EK.Jobs
{
    public class AbsentDataJobs
    {
        private readonly TecDocWsClient _tecDocWsClient;

        private readonly AzureStorageClient _azureStorageClient;

        private readonly EkSearchManagementClient _ekSearchManagementClient;

        private readonly IEkImageCache _ekImageCache;

        private readonly INotificationManager _notificationManager;

        private readonly ILogger<AbsentDataJobs> _logger;

        public AbsentDataJobs(
            TecDocWsClient tecDocWsClient,
            AzureStorageClient azureStorageClient,
            EkSearchManagementClient ekSearchManagementClient,
            IEkImageCache ekImageCache,
            INotificationManager notificationManager,
            ILogger<AbsentDataJobs> logger)
        {
            _tecDocWsClient = tecDocWsClient;
            _azureStorageClient = azureStorageClient;
            _ekSearchManagementClient = ekSearchManagementClient;
            _ekImageCache = ekImageCache;
            _notificationManager = notificationManager;
            _logger = logger;
        }

        private const string AbsentImageUrlValue = "-none-";

        public async Task SearchForAbsentThumbnailsAsync()
        {
            var totalProcessed = 0;
            var totalThumbnailFound = 0;

            try
            {
                const int BatchSize = 10;
                using (var indexClient = _ekSearchManagementClient.CreateSearchIndexClient())
                {
                    indexClient.IndexName = _ekSearchManagementClient.ProductsIndexName;

                    string lastProcessedKey = null;
                    while (true)
                    {
                        var filterBuilder = new StringBuilder();
                        filterBuilder.Append("isThumbnailSearchProvided eq null and thumbnailUrl eq null");
                        if (lastProcessedKey != null)
                        {
                            filterBuilder.Append($" and key gt '{lastProcessedKey}'");
                        }

                        var searchParameters = new SearchParameters()
                            {
                                Top = BatchSize,
                                Filter = filterBuilder.ToString(),
                                OrderBy = new List<string>()
                                    {
                                        "key asc",
                                    },
                            };

                        var searchResult = await indexClient.Documents.SearchAsync<IndexProduct>(null, searchParameters);
                        var records = searchResult.Results
                            .Select(x => x.Document)
                            .ToArray();

                        if (records.Length > 0)
                        {
                            // search for thumbnails
                            var searchTasks = records.Select(record =>
                                Task.Run(async () =>
                                    {
                                        var document = new Document()
                                            {
                                                ["key"] = record.Key,
                                            };

                                        var fileInfo = await GetPartNumberThumbnailAsync(
                                            brandName: record.BrandName,
                                            partNumber: record.PartNumber);
                                        if (fileInfo?.Url != null)
                                        {
                                            document["thumbnailUrl"] = fileInfo.Url;

                                            Interlocked.Increment(ref totalThumbnailFound);
                                        }

                                        document["isThumbnailSearchProvided"] = true;

                                        Interlocked.Increment(ref totalProcessed);

                                        return document;
                                    }));

                            var documents = await Task.WhenAll(searchTasks);

                            // index flags and thumbnail urls if found
                            var indexBatch = IndexBatch.New(documents.Select(x => IndexAction.Merge(x)));
                            try
                            {
                                await indexClient.Documents.IndexAsync(indexBatch);
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
                            lastProcessedKey = records.Last().Key;

                            var foundPercentage = (decimal)totalThumbnailFound/totalProcessed;
                            _logger.LogInformation($"Processed: {totalProcessed}, found: {totalThumbnailFound}, {foundPercentage:P}.");
                        }
                        else
                        {
                            _logger.LogInformation("No more records.");

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.UnhandledException, ex, null);
                await _notificationManager.SendWorkerMessageAsync($"Search for absent thumbnails failed, processed: {totalProcessed}, found: {totalThumbnailFound}, {ex.Message}");

                throw;
            }

            await _notificationManager.SendWorkerMessageAsync($"Search for absent thumbnails succeed, processed: {totalProcessed}, found: {totalThumbnailFound}.");
        }

        public async Task<AzureFileInfo> GetPartNumberThumbnailAsync(
            string brandName,
            string partNumber)
        {
            var imageKey = PartNumberCleaner.GetCleanedBrandPartNumber(brandName, partNumber);
            var cachedImageUrl = await _ekImageCache.GetValueAsync(imageKey, CancellationToken.None);

            if (cachedImageUrl == null)
            {
                // try find in TecDoc
                var fileInfo = await FindAndUploadPartNumberThumbnailAsync(
                    brandName: brandName,
                    partNumber: partNumber,
                    imageKey: imageKey);

                // cache both success and not found
                var imageUrlValue = fileInfo?.Url ?? AbsentImageUrlValue;
                await _ekImageCache.SetValueAsync(imageKey, imageUrlValue, CancellationToken.None);

                return fileInfo;
            }

            if (cachedImageUrl == AbsentImageUrlValue)
            {
                return null;
            }

            return new AzureFileInfo()
                {
                    Url = cachedImageUrl,
                };
        }

        private async Task<AzureFileInfo> FindAndUploadPartNumberThumbnailAsync(
            string brandName,
            string partNumber,
            string imageKey)
        {
            try
            {
                var cleanedBrandName = PartNumberCleaner.GetCleanedBrandName(brandName);
                var cleanedPartNumber = PartNumberCleaner.GetCleanedPartNumber(partNumber);

                var articles = await _tecDocWsClient.SearchByArticleNumberAsync(cleanedPartNumber, null, ArticleNumberTypeEnum.ArticleNumber);
                var article = articles
                    .Where(x => PartNumberCleaner.GetCleanedBrandName(x.BrandName)?.Equals(cleanedBrandName, StringComparison.OrdinalIgnoreCase) == true)
                    .FirstOrDefault();
                if (article == null)
                {
                    return null;
                }

                var articleExtendedInfos = await _tecDocWsClient.GetArticlesExtendedInfoAsync(new long[] { article.ArticleId });
                var articleExtendedInfo = articleExtendedInfos.FirstOrDefault();
                if (articleExtendedInfo?.ArticleDocuments == null
                    || articleExtendedInfo.ArticleDocuments.Length == 0)
                {
                    return null;
                }

                var imageDocument = articleExtendedInfo.ArticleDocuments
                    .Where(x => x.DocFileName != null)
                    .Where(x => x.DocFileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                                || x.DocFileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                || x.DocFileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)
                                || x.DocFileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                if (imageDocument == null)
                {
                    return null;
                }

                var fileName = $"{imageKey}{Path.GetExtension(imageDocument.DocFileName)}";

                var fileInfo = await _azureStorageClient.GetBlobFileIfExistsAsync(
                    EkBlobContainerNames.Images,
                    fileName);
                if (fileInfo != null)
                {
                    // if uploaded earlier
                    return fileInfo;
                }

                var imageContent = await _tecDocWsClient.GetDocumentAsync(imageDocument.DocId, imageDocument.DocTypeId);
                if (imageContent.Content == null)
                {
                    return null;
                }

                const string ImageBodyStartMarker = "base64,";
                var imageBodyStartIndex = imageContent.Content.IndexOf(ImageBodyStartMarker, StringComparison.Ordinal);
                if (imageBodyStartIndex == -1)
                {
                    return null;
                }

                imageBodyStartIndex += ImageBodyStartMarker.Length;
                var imageBodyBase64 = imageContent.Content.Substring(imageBodyStartIndex);
                var imageBody = Convert.FromBase64String(imageBodyBase64);

                fileInfo = await _azureStorageClient.UploadBlobFileAsync(
                    EkBlobContainerNames.Images,
                    fileName,
                    imageBody);

                return fileInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.UnhandledException, nameof(FindAndUploadPartNumberThumbnailAsync), ex);

                return null;
            }
        }
    }
}