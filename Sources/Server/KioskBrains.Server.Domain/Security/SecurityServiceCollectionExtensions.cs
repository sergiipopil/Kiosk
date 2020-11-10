using KioskBrains.Waf.Security;
using Microsoft.Extensions.DependencyInjection;

namespace KioskBrains.Server.Domain.Security
{
    public static class SecurityServiceCollectionExtensions
    {
        public static void AddCurrentUser(this IServiceCollection services)
        {
            services.AddScoped(serviceProvider =>
                {
                    var wafCurrentUser = serviceProvider.GetService<ICurrentUser>();
                    if (wafCurrentUser == null)
                    {
                        return null;
                    }

                    return CurrentUser.GetByAuthenticatedUser(wafCurrentUser);
                });
        }
    }
}