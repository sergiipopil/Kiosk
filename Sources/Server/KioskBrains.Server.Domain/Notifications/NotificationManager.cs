using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Server.Common.Log;
using KioskBrains.Server.Common.Notifications;
using KioskBrains.Server.Common.Storage;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Server.Domain.Notifications.Viber;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KioskBrains.Server.Domain.Notifications
{
    public class NotificationManager : INotificationManager
    {
        private readonly NotificationManagerSettings _settings;

        private readonly KioskBrainsContext _dbContext;

        private readonly ViberChannelManager _viberChannelManager;

        private readonly AzureStorageClient _azureStorageClient;

        private readonly SystemCustomerProvider _systemCustomerProvider;

        private readonly ILogger<NotificationManager> _logger;

        public NotificationManager(
            IOptions<NotificationManagerSettings> settings,
            KioskBrainsContext dbContext,
            ViberChannelManager viberChannelManager,
            AzureStorageClient azureStorageClient,
            SystemCustomerProvider systemCustomerProvider,
            ILogger<NotificationManager> logger)
        {
            _settings = settings.Value;
            Assure.ArgumentNotNull(_settings, nameof(_settings));

            _dbContext = dbContext;
            _viberChannelManager = viberChannelManager;
            _azureStorageClient = azureStorageClient;
            _systemCustomerProvider = systemCustomerProvider;
            _logger = logger;
        }

        private static readonly TimeSpan NotificationTimeToLive = TimeSpan.FromHours(12);

        public async Task SendNotificationsAsync(Notification[] notifications, CancellationToken cancellationToken)
        {
            Assure.ArgumentNotNull(notifications, nameof(notifications));

            await _azureStorageClient.SendInQueueAsync(
                _settings.NotificationQueueName,
                notifications,
                NotificationTimeToLive,
                cancellationToken);
        }

        public async Task ProcessNotificationsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var notifications = await _azureStorageClient.GetNextFromQueueAsync<Notification>(
                    _settings.NotificationQueueName,
                    20,
                    cancellationToken);

                if (notifications.Length == 0)
                {
                    return;
                }

                var customerIds = notifications
                    .Select(x => x.CustomerId)
                    .Distinct()
                    .ToArray();
                var notificationTypes = notifications
                    .Select(x => x.Type)
                    .Distinct()
                    .ToArray();

                var allReceivers = await _dbContext.NotificationReceivers
                    .Where(x => customerIds.Contains(x.CustomerId)
                                && notificationTypes.Contains(x.NotificationType))
                    .Select(x => new
                        {
                            x.CustomerId,
                            x.NotificationType,
                            x.Channel,
                            x.ReceiverId,
                        })
                    .ToArrayAsync(cancellationToken);

                foreach (var notification in notifications)
                {
                    var notificationReceivers = allReceivers
                        .Where(x => x.CustomerId == notification.CustomerId
                                    && x.NotificationType == notification.Type)
                        .ToArray();
                    if (notificationReceivers.Length == 0)
                    {
                        continue;
                    }

                    foreach (var notificationReceiver in notificationReceivers)
                    {
                        try
                        {
                            await SendNotificationAsync(
                                notificationReceiver.ReceiverId,
                                notificationReceiver.Channel,
                                notification,
                                cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(LoggingEvents.NotificationError, ex, $"Sending notification to '{notificationReceiver.ReceiverId}' via {notificationReceiver.Channel} failed.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.NotificationError, ex, $"{nameof(ProcessNotificationsAsync)} failed.");
            }
        }

        public async Task SendWorkerMessageAsync(string message)
        {
            var systemCustomerId = await _systemCustomerProvider.GetSystemCustomerIdAsync(CancellationToken.None);

            await SendNotificationsAsync(
                new Notification[]
                    {
                        new Notification()
                            {
                                CustomerId = systemCustomerId,
                                Type = NotificationTypeEnum.Worker,
                                Message = message,
                            },
                    },
                CancellationToken.None);
        }

        private async Task SendNotificationAsync(
            string receiverId,
            NotificationChannelEnum channel,
            Notification notification,
            CancellationToken cancellationToken)
        {
            INotificationChannelManager channelManager;
            switch (channel)
            {
                case NotificationChannelEnum.Viber:
                    channelManager = _viberChannelManager;
                    break;
                case NotificationChannelEnum.Email:
                    throw new NotSupportedException($"Notification channel {NotificationChannelEnum.Email} is not supported.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
            }

            var result = await channelManager.SendMessageAsync(receiverId, notification.Message, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogError(LoggingEvents.NotificationError, $"Sending notification to '{receiverId}' via {channel} failed: {result.ErrorInfo}.");
            }
        }
    }
}