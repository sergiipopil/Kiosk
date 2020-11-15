using KioskBrains.Server.Common.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace KioskBrains.Server.Domain.Notifications
{
    public class NotificationManagerStub : INotificationManager
    {
        public Task ProcessNotificationsAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SendNotificationsAsync(Notification[] notifications, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SendWorkerMessageAsync(string message)
        {
            return Task.CompletedTask;
        }
    }
}
