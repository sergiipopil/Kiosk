using KioskBrains.Server.Domain.Notifications.Viber;
using Microsoft.AspNetCore.Builder;

namespace KioskBrains.Server.Domain.Notifications
{
    public static class NotificationMiddlewareExtension
    {
        public static IApplicationBuilder UseNotificationCallbacks(this IApplicationBuilder builder)
        {
            return builder
                .UseViberWebhook();
        }
    }
}