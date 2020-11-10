using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KioskBrains.Waf.Managers.Common;
using Microsoft.Extensions.DependencyInjection;

namespace KioskBrains.Waf.Managers.Processing
{
    internal class WafManagerRegistrar
    {
        public void InitializeManagers(IServiceCollection services, AssemblyName[] appManagerAssemblyNames)
        {
            // define all manager assemblies
            var managerAssemblyNames = new List<AssemblyName>();
            if (appManagerAssemblyNames != null)
            {
                managerAssemblyNames.AddRange(appManagerAssemblyNames);
            }

            // look for managers
            foreach (var managerAssemblyName in managerAssemblyNames)
            {
                var assembly = Assembly.Load(managerAssemblyName);
                var managerBaseType = typeof(IWafManager);
                foreach (var assemblyTypeInfo in assembly.DefinedTypes.Where(x => !x.IsAbstract))
                {
                    var isManager = assemblyTypeInfo.GetInterfaces().Contains(managerBaseType);
                    if (!isManager)
                    {
                        continue;
                    }
                    if (assemblyTypeInfo.IsGenericType)
                    {
                        throw new WafManagerContractException($"'{assemblyTypeInfo.FullName}' is generic. Manager type should be either abstract or non-generic.");
                    }

                    var managerType = assemblyTypeInfo.AsType();

                    // add to DI
                    services.AddScoped(managerType);
                }
            }
        }
    }
}