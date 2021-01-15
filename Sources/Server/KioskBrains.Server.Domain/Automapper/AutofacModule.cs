using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using KioskBrains.Server.Domain.Services;
using KioskBrains.Clients.AllegroPl.ServiceInterfaces;
using KioskBrains.Server.Domain.ServiceInterfaces;
using KioskBrains.Server.Domain.Services.Repo;
using AllegroSearchService.Bl.ServiceInterfaces.Repo;

namespace KioskBrains.Server.Domain.Automapper
{
    public class AutofacModule : Module
    {
        public AutofacModule()
        {

        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TranslateService>()
               .As<ITranslateService>()
               .InstancePerLifetimeScope();
            builder.RegisterType<TokenService>()
               .As<ITokenService>()
               .InstancePerLifetimeScope();
        }

        public class DataAccessEventsAutofacModule<TContext> : Module where TContext : DbContext
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<ReadOnlyRepository<TContext>>()
                    .As<IReadOnlyRepository>()
                    .InstancePerLifetimeScope();

                builder.RegisterType<WriteOnlyRepository<TContext>>()
                    .As<IWriteOnlyRepository>()
                    .InstancePerLifetimeScope();
            }
        }
    }
}
