using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using KioskBrains.Server.Domain.Automapper;
using Serilog;
using KioskBrains.Server.Domain.Config;
using static KioskBrains.Server.Domain.Automapper.AutofacModule;
using KioskBrains.Server.Domain.Entities.DbStorage;

namespace WebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).UseSerilog()
            .Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
                {
                    containerBuilder.RegisterModule<AutofacModule>();
                    //containerBuilder.RegisterModule(new IntegrationEventsAutofacModule<MessageBusListener>(context.Configuration));
                    containerBuilder.RegisterModule(new DataAccessEventsAutofacModule<KioskBrainsContext>());
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
