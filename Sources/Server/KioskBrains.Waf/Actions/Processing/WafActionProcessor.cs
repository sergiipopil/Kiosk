using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Waf.Actions.Common;
using KioskBrains.Waf.Helpers.Exceptions;
using KioskBrains.Waf.Security;
using Microsoft.Extensions.DependencyInjection;

namespace KioskBrains.Waf.Actions.Processing
{
    internal class WafActionProcessor
    {
        public void InitializeActions(IServiceCollection services, AssemblyName[] appActionAssemblyNames)
        {
            // define all action assemblies
            var actionAssemblyNames = new List<AssemblyName>()
                {
                    new AssemblyName("KioskBrains.Waf"),
                };
            if (appActionAssemblyNames != null)
            {
                actionAssemblyNames.AddRange(appActionAssemblyNames);
            }

            // look for actions
            var wafActionFullNames = new HashSet<string>();
            foreach (var actionAssemblyName in actionAssemblyNames)
            {
                var assembly = Assembly.Load(actionAssemblyName);
                var actionInterfaceType = typeof(IWafActionInternal);
                foreach (var assemblyTypeInfo in assembly.DefinedTypes.Where(x => !x.IsAbstract))
                {
                    var isAction = assemblyTypeInfo
                        .GetInterfaces()
                        .Any(x => x == actionInterfaceType);
                    if (!isAction)
                    {
                        continue;
                    }
                    if (assemblyTypeInfo.IsGenericType)
                    {
                        throw new WafActionContractException($"'{assemblyTypeInfo.FullName}' is generic. Action type should be either abstract or non-generic.");
                    }

                    var actionType = assemblyTypeInfo.AsType();

                    // check name uniqueness
                    var wafActionName = WafActionNameHelper.GetActionNameByType(actionType);
                    if (wafActionFullNames.Contains(wafActionName.FullName))
                    {
                        throw new WafActionContractException($"Actions with the same name are not allowed ('{wafActionName.FullName}').");
                    }
                    wafActionFullNames.Add(wafActionName.FullName);

                    // check for authorization attribute
                    var actionAuthorizeAttribute = actionType.GetCustomAttribute<ActionAuthorizeAttribute>();

                    // add action
                    WafActionTypes[actionType] = new WafActionMetadata()
                        {
                            WafActionName = wafActionName,
                            ActionAuthorizeAttribute = actionAuthorizeAttribute,
                        };

                    // add to DI
                    services.AddScoped(actionType);
                }
            }
        }

        public Dictionary<Type, WafActionMetadata> WafActionTypes { get; } = new Dictionary<Type, WafActionMetadata>();

        public async Task<object> ExecuteActionAsync(IServiceProvider serviceProvider, Type actionType, object request)
        {
            Assure.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Assure.ArgumentNotNull(actionType, nameof(actionType));
            Assure.ArgumentNotNull(request, nameof(request));

            // todo: transactions (wait for 2.0 with TransactionScope?)
            // todo: long action detection

            if (!(serviceProvider.GetService(actionType) is IWafActionInternal action))
            {
                throw new InvalidOperationException($"Action '{actionType.FullName}' is not registered or doesn't implement {nameof(IWafActionInternal)}.");
            }

            if (!action.AllowAnonymous)
            {
                var currentUser = serviceProvider.GetService<ICurrentUser>();
                if (currentUser == null)
                {
                    throw new AuthenticationRequiredException();
                }

                var actionMetadata = WafActionTypes[actionType];
                if (actionMetadata.ActionAuthorizeAttribute != null)
                {
                    var isAuthorized = actionMetadata.ActionAuthorizeAttribute.Authorize(currentUser);
                    if (!isAuthorized)
                    {
                        throw new ActionAuthorizationException($"Access to action '{actionMetadata.WafActionName.Name} ({actionMetadata.WafActionName.Method})' is not authorized ({currentUser.Id}).");
                    }
                }
            }

            var response = await action.ExecuteAsync(request);
            return response;
        }
    }
}