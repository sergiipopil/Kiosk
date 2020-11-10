using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KioskBrains.Waf.Security
{
    public static class WafSecurityServiceCollectionExtensions
    {
        public static void AddCurrentUserForWebHost(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ICurrentUser>(serviceProvider =>
                {
                    var httpContext = serviceProvider.GetService<IHttpContextAccessor>().HttpContext;
                    if (httpContext.User?.Identity?.IsAuthenticated != true)
                    {
                        return null;
                    }
                    return new WebCurrentUser(httpContext.User);
                });
        }
    }
}