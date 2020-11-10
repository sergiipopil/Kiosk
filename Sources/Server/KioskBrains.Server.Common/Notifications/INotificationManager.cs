using System.Threading;
using System.Threading.Tasks;

namespace KioskBrains.Server.Common.Notifications
{
    public interface INotificationManager
    {
        Task SendNotificationsAsync(Notification[] notifications, CancellationToken cancellationToken);

        Task ProcessNotificationsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Send worker status notification for system customer.
        /// </summary>
        Task SendWorkerMessageAsync(string message);
    }
}