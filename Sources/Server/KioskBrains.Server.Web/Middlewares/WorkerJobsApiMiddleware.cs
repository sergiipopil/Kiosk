using System;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Server.Common.Log;
using KioskBrains.Server.Common.Notifications;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Server.EK.Integration.Jobs;
using KioskBrains.Server.EK.Jobs;
using KioskBrains.Waf.Actions.Processing.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KioskBrains.Server.Web.Middlewares
{
    public class WorkerJobsApiMiddleware
    {
        private readonly RequestDelegate _next;

        public WorkerJobsApiMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public const string WorkerApiPathPrefix = "/worker-jobs/";

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider, ILogger<WorkerJobsApiMiddleware> logger)
        {
            var requestPath = context.Request.Path.Value;
            if (!requestPath.StartsWith(WorkerApiPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            try
            {
                var settings = serviceProvider.GetRequiredService<IOptions<WorkerJobsApiSettings>>().Value;
                if (settings?.ApiKey == null)
                {
                    throw new InvalidOperationException("Configuration error.");
                }

                var jobName = requestPath.Substring(WorkerApiPathPrefix.Length);
                switch (jobName)
                {
                    case "sync-ek-orders":
                        var orderSyncJobs = serviceProvider.GetRequiredService<OrderSyncJobs>();
                        await orderSyncJobs.TrySendOrderBatchAsync();
                        break;

                    case "sync-ek-omega-price-lists":
                    {
                        var omegaAutoBizJobs = serviceProvider.GetRequiredService<OmegaAutoBizJobs>();
                        await omegaAutoBizJobs.RunPriceListSynchronizationAsync();
                        var absentDataJobs = serviceProvider.GetRequiredService<AbsentDataJobs>();
                        await absentDataJobs.SearchForAbsentThumbnailsAsync();
                        break;
                    }

                    case "sync-ek-elit-price-lists":
                    {
                        var elitUaJobs = serviceProvider.GetRequiredService<ElitUaJobs>();
                        await elitUaJobs.RunPriceListSynchronizationAsync();
                        var absentDataJobs = serviceProvider.GetRequiredService<AbsentDataJobs>();
                        await absentDataJobs.SearchForAbsentThumbnailsAsync();
                        break;
                    }

                    case "process-notifications":
                        var notificationManager = serviceProvider.GetRequiredService<INotificationManager>();
                        await notificationManager.ProcessNotificationsAsync(CancellationToken.None);
                        break;

                    case "exchange-rates-auto-update":
                        var exchangeRateAutoUpdateManager = serviceProvider.GetRequiredService<ExchangeRateAutoUpdateManager>();
                        await exchangeRateAutoUpdateManager.AutoUpdateRatesAsync(CancellationToken.None);
                        break;
                }

                context.Response.StatusCode = (int)WafActionResponseCodeEnum.Ok;
            }
            catch (Exception ex)
            {
                logger.LogError(LoggingEvents.UnhandledException, ex, "Unhandled exception.");
                context.Response.StatusCode = (int)WafActionResponseCodeEnum.InternalServerError;
            }
        }
    }
}
