using System.Reflection;
using KioskBrains.Clients.KioskProxy;
using KioskBrains.Common.Contracts;
using KioskBrains.Server.Common.Cache;
using KioskBrains.Server.Common.Notifications;
using KioskBrains.Server.Common.Services;
using KioskBrains.Server.Common.Storage;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Server.Domain.Managers.Integration;
using KioskBrains.Server.Domain.Notifications;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Server.Domain.Settings;
using KioskBrains.Waf.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace KioskBrains.Server.Domain.Startup
{
    public static class StartupExtensions
    {
        public static AssemblyName[] AppActionAssemblyNames { get; } =
            {
                new AssemblyName("KioskBrains.Server.Domain")
            };

        public static void AddKioskBrainsApplication(
            this IServiceCollection services,
            WafHostEnum wafHost,
            IConfiguration configuration)
        {
            Assure.ArgumentNotNull(configuration, nameof(configuration));

            JsonDefaultSettings.Initialize();

            // Settings
            services.Configure<EkSearchSettings>(configuration.GetSection("EkSearchSettings"));

            // Memory Cache
            services.AddMemoryCache();

            // Persistent Cache
            services.AddScoped<IPersistentCache, DbPersistentCache>();

            // DB Context
            services.AddDbContextPool<KioskBrainsContext>(options =>
                options
                    .UseSqlServer(configuration.GetConnectionString("KioskBrainsContext"))
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning)));

            if (wafHost == WafHostEnum.Web)
            {
                // Authentication
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                                {
                                    ValidateIssuer = true,
                                    ValidIssuer = JwtAuthOptions.Issuer,

                                    ValidateAudience = true,
                                    ValidAudience = JwtAuthOptions.Audience,

                                    ValidateLifetime = true,

                                    ValidateIssuerSigningKey = true,
                                    IssuerSigningKey = JwtAuthOptions.GetSymmetricSecurityKey(),
                                };
                        });

                // Add CurrentUser
                services.AddCurrentUser();
            }

            // WAF
            services.AddWaf(
                wafHost,
                appActionAssemblyNames: AppActionAssemblyNames,
                appManagerAssemblyNames: AppActionAssemblyNames);

            // Integration Log
            services.AddTransient<IIntegrationLogManager, IntegrationLogManager>();

            // Notifications
            services.Configure<NotificationManagerSettings>(configuration.GetSection("NotificationManagerSettings"));
            services.AddScoped<INotificationManager, NotificationManager>();

            // Common Clients
            services.Configure<KioskProxyClientSettings>(configuration.GetSection("KioskProxyClientSettings"));
            services.AddScoped<KioskProxyClient>();
            services.Configure<AzureStorageSettings>(configuration.GetSection("AzureStorageSettings"));
            services.AddScoped<AzureStorageClient>();
        }
    }
}