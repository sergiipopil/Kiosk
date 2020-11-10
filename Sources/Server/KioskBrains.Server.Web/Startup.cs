using System;
using KioskBrains.Server.Domain.Notifications;
using KioskBrains.Server.Domain.Startup;
using KioskBrains.Server.EK.Integration.Middleware;
using KioskBrains.Server.Web.Middlewares;
using KioskBrains.Waf.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KioskBrains.Server.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<WorkerJobsApiSettings>(Configuration.GetSection("WorkerJobsApiSettings"));

            services.AddEkApi(Configuration);

            services.AddKioskBrainsApplication(WafHostEnum.Web, Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseNotificationCallbacks();

            app.UseWorkerJobsApi();

            app.UseAuthentication();

            // todo: for test only - move to separate middleware
            // easing of request rate requirements
            app.Use(async (context, next) =>
            {
                var minRequestBodyDataRateFeature = context.Features.Get<IHttpMinRequestBodyDataRateFeature>();
                minRequestBodyDataRateFeature.MinDataRate = new MinDataRate(
                    bytesPerSecond: 100, // default 240
                    gracePeriod: TimeSpan.FromSeconds(10) // default 5
                );

                await next();
            });

            app.UseEkApi();

            app.UseWaf()
                .UseWafSpa();
        }
    }
}
