using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Server.Common.Log;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace KioskBrains.Server.Common.Storage
{
    public class AzureStorageClient
    {
        private readonly AzureStorageSettings _settings;

        private readonly ILogger<AzureStorageClient> _logger;

        public AzureStorageClient(
            IOptions<AzureStorageSettings> settings,
            ILogger<AzureStorageClient> logger)
        {
            _settings = settings.Value;
            Assure.ArgumentNotNull(_settings, nameof(_settings));

            _logger = logger;
        }

        public async Task<AzureFileInfo> UploadBlobFileAsync(string blobContainerName, string fileName, byte[] fileBytes)
        {
            Assure.ArgumentNotNull(blobContainerName, nameof(blobContainerName));
            Assure.ArgumentNotNull(fileName, nameof(fileName));
            Assure.ArgumentNotNull(fileBytes, nameof(fileBytes));

            var blobClient = CreateBlobClient();

            var blobContainer = blobClient.GetContainerReference(blobContainerName);
            var blockBlob = blobContainer.GetBlockBlobReference(fileName);

            await blockBlob.UploadFromByteArrayAsync(fileBytes, 0, fileBytes.Length);

            return new AzureFileInfo()
                {
                    Url = blockBlob.Uri.ToString(),
                };
        }

        public async Task<AzureFileInfo> GetBlobFileIfExistsAsync(string blobContainerName, string fileName)
        {
            Assure.ArgumentNotNull(blobContainerName, nameof(blobContainerName));
            Assure.ArgumentNotNull(fileName, nameof(fileName));

            var blobClient = CreateBlobClient();

            var blobContainer = blobClient.GetContainerReference(blobContainerName);
            var blockBlob = blobContainer.GetBlockBlobReference(fileName);

            var exists = await blockBlob.ExistsAsync();

            return exists
                ? new AzureFileInfo()
                    {
                        Url = blockBlob.Uri.ToString(),
                    }
                : null;
        }

        public async Task SendInQueueAsync<TMessage>(
            string queueName,
            TMessage[] messages,
            TimeSpan? timeToLive,
            CancellationToken cancellationToken)
            where TMessage : class
        {
            Assure.ArgumentNotNull(queueName, nameof(queueName));
            Assure.ArgumentNotNull(messages, nameof(messages));

            if (messages.Length == 0)
            {
                return;
            }

            var queueClient = CreateQueueClient();
            var queue = queueClient.GetQueueReference(queueName);
            foreach (var message in messages)
            {
                var messageContent = JsonConvert.SerializeObject(message);
                await queue.AddMessageAsync(
                    new CloudQueueMessage(messageContent),
                    timeToLive,
                    null,
                    null,
                    null,
                    cancellationToken);
            }
        }

        public async Task<TMessage[]> GetNextFromQueueAsync<TMessage>(
            string queueName,
            int messageCount,
            CancellationToken cancellationToken)
            where TMessage : class, new()
        {
            Assure.ArgumentNotNull(queueName, nameof(queueName));
            Assure.CheckFlowState(messageCount > 0 && messageCount <= 32, $"{nameof(messageCount)} should belong to interval (0..32].");

            var queueClient = CreateQueueClient();
            var queue = queueClient.GetQueueReference(queueName);
            var queueMessages = await queue.GetMessagesAsync(
                messageCount,
                null,
                null,
                null,
                cancellationToken);
            var messageList = new List<TMessage>();
            foreach (var queueMessage in queueMessages)
            {
                await queue.DeleteMessageAsync(
                    queueMessage.Id,
                    queueMessage.PopReceipt,
                    null,
                    null,
                    cancellationToken);

                try
                {
                    var message = JsonConvert.DeserializeObject<TMessage>(queueMessage.AsString);
                    messageList.Add(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(LoggingEvents.NotificationError, ex, $"Bad format of queue message '{queueMessage.Id}'.");
                }
            }

            return messageList.ToArray();
        }

        private CloudBlobClient CreateBlobClient()
        {
            var storageCredentials = new StorageCredentials(_settings.AccountName, _settings.AccountKey);
            var storageAccount = new CloudStorageAccount(storageCredentials, true);
            return storageAccount.CreateCloudBlobClient();
        }

        private CloudQueueClient CreateQueueClient()
        {
            var storageCredentials = new StorageCredentials(_settings.AccountName, _settings.AccountKey);
            var storageAccount = new CloudStorageAccount(storageCredentials, true);
            return storageAccount.CreateCloudQueueClient();
        }
    }
}