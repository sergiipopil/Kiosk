using AllegroSearchService.Bl.ServiceInterfaces.Repo;
using KioskBrains.Clients.AllegroPl;
using KioskBrains.Clients.AllegroPl.ServiceInterfaces;
using KioskBrains.Clients.Ek4Car;
using KioskBrains.Clients.ElitUa;
using KioskBrains.Clients.OmegaAutoBiz;
using KioskBrains.Clients.TecDocWs;
using KioskBrains.Clients.YandexTranslate;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Server.Domain.Services;
using KioskBrains.Server.Domain.Services.Repo;
using KioskBrains.Server.EK.Common.Cache;
using KioskBrains.Server.EK.Common.Search;
using KioskBrains.Server.EK.Integration.Jobs;
using KioskBrains.Server.EK.Integration.Managers;
using KioskBrains.Server.EK.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KioskBrains.Server.EK.Integration.Middleware
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEkApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<EkApiRoleProvider>();
            services.AddScoped<EkIntegrationManager>();
            services.AddScoped<TestEkIntegrationManager>();
            services.AddScoped<IEkIntegrationManager>(sp =>
                {
                    var apiRoleProvider = sp.GetRequiredService<EkApiRoleProvider>();
                    return apiRoleProvider.GetEkApiRole() == EkApiRoleEnum.EkTest
                        ? (IEkIntegrationManager)sp.GetRequiredService<TestEkIntegrationManager>()
                        : sp.GetRequiredService<EkIntegrationManager>();
                });

            // Image Cache
            services.AddScoped<IEkImageCache, EkImageCache>();

            // Clients
            services.Configure<TecDocWsClientSettings>(configuration.GetSection("TecDocWsClientSettings"));
            services.AddScoped<TecDocWsClient>();
            services.Configure<OmegaAutoBizClientSettings>(configuration.GetSection("OmegaAutoBizClientSettings"));
            services.AddScoped<OmegaAutoBizClient>();
            services.Configure<ElitUaClientSettings>(configuration.GetSection("ElitUaClientSettings"));
            services.AddScoped<ElitUaClient>();
            //services.Configure<YandexTranslateClientSettings>(configuration.GetSection("YandexTranslateClientSettings"));
            services.AddSingleton<YandexTranslateClient>();
            services.Configure<AllegroPlClientSettings>(configuration.GetSection("AllegroPlClientSettings"));
            services.AddSingleton<ITranslateService, TranslateService>();

            services.AddSingleton<IReadOnlyRepository, ReadOnlyRepository<KioskBrainsContext>>();
            services.AddSingleton<IWriteOnlyRepository, WriteOnlyRepository<KioskBrainsContext>>();
            services.AddSingleton<AllegroPlClient>();
            services.Configure<Ek4CarClientSettings>(configuration.GetSection("Ek4CarClientSettings"));
            services.AddScoped<Ek4CarClient>();
            services.Configure<EkSearchManagementSettings>(configuration.GetSection("EkSearchManagementSettings"));
            services.AddScoped<EkSearchManagementClient>();

            // Jobs
            services.AddScoped<OrderSyncJobs>();
            services.AddScoped<OmegaAutoBizJobs>();
            services.AddScoped<ElitUaJobs>();
            services.AddScoped<AbsentDataJobs>();
        }
    }
}