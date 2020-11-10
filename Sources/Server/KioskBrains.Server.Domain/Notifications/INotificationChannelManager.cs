using System.Threading;
using System.Threading.Tasks;

namespace KioskBrains.Server.Domain.Notifications
{
    public interface INotificationChannelManager
    {
        Task<NotificationResult> SendMessageAsync(string receiverId, string message, CancellationToken cancellationToken);
    }
}