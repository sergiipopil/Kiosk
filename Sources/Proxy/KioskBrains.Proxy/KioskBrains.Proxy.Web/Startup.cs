using KioskBrains.Common.Contracts;
using KioskBrains.Proxy.Web.Authentication;
using KioskBrains.Proxy.Web.ReverseProxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace KioskBrains.Proxy.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonDefaultSettings.Initialize();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

            app.UseSimpleKeyAuthentication();

            app.RunReverseProxy();
        }
    }
}