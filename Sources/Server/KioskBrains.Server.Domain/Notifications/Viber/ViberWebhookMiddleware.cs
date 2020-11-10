using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using KioskBrains.Server.Common.Log;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KioskBrains.Server.Domain.Notifications.Viber
{
    public class ViberWebhookMiddleware
    {
        public const string CallbackPath = NotificationConstants.CallbackUrlPrefix + "/viber";

        private readonly RequestDelegate _next;

        public ViberWebhookMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            NotificationCallbackManager notificationCallbackManager,
            ILogger<ViberWebhookMiddleware> logger)
        {
            var requestPath = context.Request.Path.Value;
            if (!requestPath.StartsWith(CallbackPath, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            try
            {
                // todo: verify X-Viber-Content-Signature

                using (var streamReader = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    var messageJson = await streamReader.ReadToEndAsync();
                    await notificationCallbackManager.LogReceivedCallbackMessageAsync("Viber", messageJson);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(LoggingEvents.UnhandledException, ex, "Viber callback failed.");
            }

            // always successful
            context.Response.StatusCode = 200;
        }
    }
}