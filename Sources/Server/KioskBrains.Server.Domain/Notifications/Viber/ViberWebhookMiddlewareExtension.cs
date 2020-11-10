using Microsoft.AspNetCore.Builder;

namespace KioskBrains.Server.Domain.Notifications.Viber
{
    public static class ViberWebhookMiddlewareExtension
    {
        public static IApplicationBuilder UseViberWebhook(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ViberWebhookMiddleware>();
        }
    }
}