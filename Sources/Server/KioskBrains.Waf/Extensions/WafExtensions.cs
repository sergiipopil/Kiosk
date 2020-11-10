using System;
using System.Reflection;
using KioskBrains.Waf.Actions.Processing;
using KioskBrains.Waf.Actions.Processing.Web;
using KioskBrains.Waf.Managers.Processing;
using KioskBrains.Waf.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace KioskBrains.Waf.Extensions
{
    public static class WafExtensions
    {
        public static void AddWaf(
            this IServiceCollection services,
            WafHostEnum wafHost,
            AssemblyName[] appActionAssemblyNames,
            AssemblyName[] appManagerAssemblyNames)
        {
            // memory cache
            services.AddMemoryCache();

            // register managers
            var managerRegistrar = new WafManagerRegistrar();
            services.AddSingleton<WafManagerRegistrar>(managerRegistrar);
            managerRegistrar.InitializeManagers(services, appManagerAssemblyNames);

            // register actions
            var actionProcessor = new WafActionProcessor();
            services.AddSingleton<WafActionProcessor>(actionProcessor);

            actionProcessor.InitializeActions(services, appActionAssemblyNames);

            if (wafHost == WafHostEnum.Web)
            {
                // current user
                services.AddCurrentUserForWebHost();

                // routing services for web API
                var actionWebProcessor = new WafActionWebProcessor(actionProcessor);
                services.AddSingleton<WafActionWebProcessor>(actionWebProcessor);
                actionWebProcessor.AddRouting(services);
            }
        }

        public static WafWebApplicationBuilder UseWaf(this IApplicationBuilder applicationBuilder)
        {
            var wafWebApplicationBuilder = new WafWebApplicationBuilder(applicationBuilder);
            wafWebApplicationBuilder.AddWafApiRoutes();
            return wafWebApplicationBuilder;
        }

        public static Type GetWafActionType(string requestMethod, string actionKebabName)
        {
            return WafActionWebProcessor.GetWafActionType(requestMethod, actionKebabName);
        }
    }
}