using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.KioskConfiguration;
using KioskBrains.Common.KioskState;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Components.Contracts;
using KioskBrains.Kiosk.Core.Components.Initialization;
using KioskBrains.Kiosk.Core.Components.Operations;
using KioskBrains.Kiosk.Core.Components.States;
using KioskBrains.Kiosk.Core.Components.Statuses;
using KioskBrains.Kiosk.Helpers.Applications;

namespace KioskBrains.Kiosk.Core.Components
{
    public class ComponentManager
    {
        #region Singleton

        public static ComponentManager Current { get; } = new ComponentManager();

        private ComponentManager()
        {
        }

        #endregion

        private readonly Dictionary<string, ComponentBase> _componentsByRole = new Dictionary<string, ComponentBase>();

        private readonly Dictionary<Type, List<ComponentContractInfo>> _componentContracts = new Dictionary<Type, List<ComponentContractInfo>>();

        #region Initialization

        /// <summary>
        /// Special method to register contract directly.
        /// Should be used only by KioskApplicationBase.
        /// </summary>
        internal void RegisterContractInternal<TComponentContract>(string componentRole, ComponentContractInfo componentContractInfo)
        {
            Assure.ArgumentNotNull(componentRole, nameof(componentRole));
            Assure.ArgumentNotNull(componentContractInfo, nameof(componentContractInfo));

            var contactType = typeof(TComponentContract);
            Assure.CheckFlowState(contactType.IsInterface, $"{contactType.FullName} is not interface.");

            _componentsByRole[componentRole] = componentContractInfo.Component;
            AddContractInfo(contactType, componentContractInfo);
        }

        private readonly Dictionary<string, Type> _supportedComponentTypes = new Dictionary<string, Type>();

        // 'public' to be used in tests/POCs.
        public void Initialize(IEnumerable<Type> supportedComponentTypes)
        {
            Assure.ArgumentNotNull(supportedComponentTypes, nameof(supportedComponentTypes));

            foreach (var supportedComponentType in supportedComponentTypes)
            {
                var typeFullName = supportedComponentType.FullName;
                if (_supportedComponentTypes.ContainsKey(typeFullName))
                {
                    throw new ComponentConfigurationException($"Component type with name '{typeFullName}' is presented twice.");
                }

                _supportedComponentTypes[typeFullName] = supportedComponentType;
            }
        }

        internal ComponentInitializationLog InitializationLog { get; } = new ComponentInitializationLog();

        // 'public' to be used in tests/POCs.
        /// <summary>
        /// Configuration errors don't cause exceptions. Components with configuration errors are ignored.
        /// warn: not thread-safe.
        /// </summary>
        public async Task RegisterAndInitializeAsync(params ComponentConfiguration[] componentConfigurations)
        {
            if (componentConfigurations == null
                || componentConfigurations.Length == 0)
            {
                Log.Warning(LogContextEnum.Configuration, $"Empty '{nameof(componentConfigurations)}' was passed.");
                return;
            }

            // register new components
            var newComponentInfos = new List<(ComponentBase Component, ComponentConfiguration Configuration)>();
            foreach (var componentConfiguration in componentConfigurations)
            {
                if (_componentsByRole.ContainsKey(componentConfiguration.ComponentRole))
                {
                    Log.Error(LogContextEnum.Configuration, $"Component role '{componentConfiguration.ComponentRole}' is already registered.");
                    continue;
                }

                var component = CreateComponentInstance(componentConfiguration);
                if (component == null)
                {
                    continue;
                }

                _componentsByRole[component.ComponentRole] = component;
                newComponentInfos.Add((component, componentConfiguration));
            }

            var newComponents = newComponentInfos
                .Select(x => x.Component)
                .ToArray();

            // add to initialization log
            await InitializationLog.AddRangeAsync(newComponents);

            // initialize new components
            var initializationTasks = newComponentInfos
                .Select(x => x.Component.InitializeAndGetSupportedContractsAsync(x.Configuration.Settings));
            var supportedContracts = await Task.WhenAll(initializationTasks);

            // register contracts
            for (var i = 0; i < newComponents.Length; i++)
            {
                RegisterComponentContracts(newComponents[i], supportedContracts[i]);
            }

            // initialize dependencies
            var dependencyInitializationTasks = newComponents
                .Select(x => x.InitializeContractDependenciesInternalAsync());
            await Task.WhenAll(dependencyInitializationTasks);
        }

        private ComponentBase CreateComponentInstance(ComponentConfiguration componentConfiguration)
        {
            try
            {
                Assure.ArgumentNotNull(componentConfiguration, nameof(componentConfiguration));
                Assure.ArgumentNotNull(componentConfiguration.ComponentName, nameof(componentConfiguration.ComponentName));
                Assure.ArgumentNotNull(componentConfiguration.ComponentRole, nameof(componentConfiguration.ComponentRole));
                Assure.CheckFlowState(_supportedComponentTypes != null, $"{nameof(ComponentManager)} is not initialized.");

                // ReSharper disable PossibleNullReferenceException
                var componentTypeNames = _supportedComponentTypes.Keys
                    // ReSharper restore PossibleNullReferenceException
                    .Where(x => x.EndsWith(componentConfiguration.ComponentName))
                    .ToArray();
                if (componentTypeNames.Length > 1)
                {
                    throw new ComponentConfigurationException($"More than 1 app component type matches to component name '{componentConfiguration.ComponentName}': {string.Join(", ", componentTypeNames)}.");
                }

                if (componentTypeNames.Length == 0)
                {
                    throw new ComponentConfigurationException($"There is no app component type that matches to component name '{componentConfiguration.ComponentName}'.");
                }

                var componentType = _supportedComponentTypes[componentTypeNames[0]];
                object componentInstance;
                try
                {
                    componentInstance = Activator.CreateInstance(componentType);
                }
                catch (Exception ex)
                {
                    throw new ComponentConfigurationException($"Instantiation of '{componentType.FullName}' failed with '{ex.Message}'.");
                }

                if (!(componentInstance is ComponentBase typedComponentInstance))
                {
                    throw new ComponentConfigurationException($"Component type '{componentType.FullName}' doesn't inherit {nameof(ComponentBase)}.");
                }

                var contractStateType = typeof(ComponentState);
                // get all properties including private and explicit interface implementations
                // it's done on component level since it's much simpler than to check an hierarchy of contact interfaces
                // TBD: move all checks to contract registration - check entire hierarchy of contact interfaces
                var componentProperties = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var componentProperty in componentProperties)
                {
                    var isOperationOrState = false;
                    // check if operation property
                    if (componentProperty.PropertyType.IsConstructedGenericType
                        && componentProperty.PropertyType.GetGenericTypeDefinition() == typeof(ComponentOperation<,>))
                    {
                        isOperationOrState = true;
                        var operationValue = componentProperty.GetValue(typedComponentInstance);
                        if (operationValue == null)
                        {
                            throw new ComponentConfigurationException($"Component operation '{componentType.FullName}'.{componentProperty.Name} is null after instantiation.");
                        }
                    }

                    // check if state property
                    if (contractStateType.IsAssignableFrom(componentProperty.PropertyType))
                    {
                        isOperationOrState = true;
                        var contractStateValue = componentProperty.GetValue(typedComponentInstance);
                        if (contractStateValue == null)
                        {
                            throw new ComponentConfigurationException($"Component contract state '{componentType.FullName}'.{componentProperty.Name} is null after instantiation.");
                        }

                        var statePropertyName = componentProperty.Name;
                        if (statePropertyName != ContractStatePropertyName
                            // for explicit contract implementations
                            && !statePropertyName.EndsWith("." + ContractStatePropertyName))
                        {
                            throw new ComponentConfigurationException($"Component contract state '{componentType.FullName}'.{componentProperty.Name} is not named '{ContractStatePropertyName}' (required by convention).");
                        }
                    }

                    if (isOperationOrState)
                    {
                        if (componentProperty.CanWrite)
                        {
                            throw new ComponentConfigurationException($"Component operation/state '{componentType.FullName}'.{componentProperty.Name} must be readonly.");
                        }
                    }
                }

                typedComponentInstance.ComponentRole = componentConfiguration.ComponentRole;
                typedComponentInstance.ComponentName = componentConfiguration.ComponentName;
                return typedComponentInstance;
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Configuration, $"Component '{componentConfiguration?.ComponentRole} ({componentConfiguration?.ComponentName})' instantiation failed.", ex);
                return null;
            }
        }

        private const string ContractStatusPropertyName = "Status";

        private const string ContractStatePropertyName = "State";

        private void RegisterComponentContracts(ComponentBase component, Type[] componentContracts)
        {
            if (componentContracts == null || componentContracts.Length == 0)
            {
                // component doesn't implement any contracts
                return;
            }

            var componentType = component.GetType();
            foreach (var componentContractType in componentContracts
                // ignore contract duplicates
                .Distinct())
            {
                if (!componentContractType.IsInterface
                    || !typeof(IComponentContract).IsAssignableFrom(componentContractType))
                {
                    Log.Error(LogContextEnum.Configuration, $"Component contract '{componentContractType.FullName}' is not an interface or doesn't inherit {nameof(IComponentContract)}.");
                    continue;
                }

                if (!componentContractType.IsAssignableFrom(componentType))
                {
                    Log.Error(LogContextEnum.Configuration, $"Component '{componentType.FullName}' returned contract '{componentContractType.FullName}' that is not implemented by the component.");
                    continue;
                }

                var contractStatusProperty = componentContractType.GetProperty(ContractStatusPropertyName);
                if (contractStatusProperty == null
                    || !contractStatusProperty.CanRead
                    || contractStatusProperty.CanWrite
                    || contractStatusProperty.PropertyType != typeof(BindableComponentStatus))
                {
                    Log.Error(LogContextEnum.Configuration, $"Component contract '{componentContractType.FullName}' doesn't declare readable-only {ContractStatusPropertyName} property of type {nameof(BindableComponentStatus)} (required by convention).");
                    continue;
                }

                var contractStatus = (BindableComponentStatus)contractStatusProperty.GetValue(component);
                // check if separate contract status
                if (contractStatus != component.Status)
                {
                    if (contractStatus == null)
                    {
                        Log.Error(LogContextEnum.Configuration, $"Component contract '{componentContractType.FullName}' status is null.");
                        continue;
                    }

                    // todo: add support for a separate contract statuses (see architecture docs)
                    // bind separate contract status to component status (status inheritance)
                    // implement appropriate monitoring representation
                }

                ComponentState contractState = null;
                var contractStateProperty = componentContractType.GetProperty(ContractStatePropertyName);
                if (contractStateProperty != null
                    && contractStateProperty.CanRead)
                {
                    // no need to check on null - it's already checked
                    contractState = (ComponentState)contractStateProperty.GetValue(component);
                }

                var componentContractInfo = new ComponentContractInfo(component, contractStatus, contractState);

                AddContractInfo(componentContractType, componentContractInfo);
            }
        }

        private void AddContractInfo(Type componentContractType, ComponentContractInfo componentContractInfo)
        {
            List<ComponentContractInfo> contractList;
            if (_componentContracts.ContainsKey(componentContractType))
            {
                contractList = _componentContracts[componentContractType];
            }
            else
            {
                contractList = new List<ComponentContractInfo>();
                _componentContracts[componentContractType] = contractList;
            }

            contractList.Add(componentContractInfo);
        }

        #endregion

        #region Monitorable State

        internal KioskMonitorableState GetKioskMonitorableState()
        {
            try
            {
                return new KioskMonitorableState()
                    {
                        KioskVersion = VersionHelper.AppVersionString,
                        LocalTime = DateTime.Now,
                        ComponentMonitorableStates = _componentsByRole.Values
                            .Where(x => x.IsStateMonitorable)
                            .Select(x => x.GetComponentMonitorableState())
                            .ToArray(),
                    };
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Component, nameof(GetKioskMonitorableState), ex);
                return null;
            }
        }

        #endregion

        #region Contract Obtaining

        public TContractType[] GetComponents<TContractType>()
            where TContractType : IComponentContract
        {
            var contactComponents = _componentContracts.GetValueOrDefault(typeof(TContractType));
            if (contactComponents == null)
            {
                return new TContractType[0];
            }

            return contactComponents
                .Select(x => x.Component)
                .Cast<TContractType>()
                .ToArray();
        }

        public TContractType GetComponent<TContractType>(string componentRole = null, bool mandatory = true)
            where TContractType : class, IComponentContract
        {
            var contractComponents = GetComponents<TContractType>();
            TContractType component;
            if (componentRole != null)
            {
                component = contractComponents
                    .Where(x => x.ComponentRole == componentRole)
                    .FirstOrDefault();
                if (component == null)
                {
                    if (!mandatory)
                    {
                        return null;
                    }

                    throw new ComponentConfigurationException($"Component with role '{componentRole}' was not registered or doesn't implement '{typeof(TContractType).FullName}' contract.");
                }
            }
            else
            {
                if (contractComponents.Length == 0)
                {
                    if (!mandatory)
                    {
                        return null;
                    }

                    throw new ComponentConfigurationException($"Component that implements '{typeof(TContractType).Name}' contract was not registered.");
                }

                if (contractComponents.Length > 1)
                {
                    Log.Warning(LogContextEnum.Configuration, $"More than 1 component that implements '{typeof(TContractType).Name}' contract were registered: {string.Join(", ", contractComponents.Select(x => $"'{x.FullName}'"))}. Specify '{nameof(componentRole)}'.");
                }

                component = contractComponents[0];
            }

            return component;
        }

        internal ComponentState GetContractState(ComponentStatePropertyLinkWrapper componentStatePropertyLink)
        {
            Assure.ArgumentNotNull(componentStatePropertyLink, nameof(componentStatePropertyLink));
            var contractTypes = _componentContracts
                .Keys
                .Where(x => x.FullName.EndsWith(componentStatePropertyLink.ContractTypeName))
                .ToArray();
            if (contractTypes.Length == 0)
            {
                throw new ComponentConfigurationException($"No contracts were found by contract type name (link '{componentStatePropertyLink}').");
            }

            if (contractTypes.Length > 1)
            {
                Log.Warning(LogContextEnum.Configuration, $"More than 1 contract matches contract name (link '{componentStatePropertyLink}'): {string.Join(", ", contractTypes.Select(x => $"'{x.FullName}'"))}. Specify namespace.");
            }

            var contractComponents = _componentContracts[contractTypes[0]].ToArray();
            if (!string.IsNullOrEmpty(componentStatePropertyLink.ComponentRole))
            {
                // filter by component role
                contractComponents = contractComponents
                    .Where(x => x.Component.ComponentRole == componentStatePropertyLink.ComponentRole)
                    .ToArray();
            }

            if (contractComponents.Length == 0)
            {
                throw new ComponentConfigurationException($"Component was not found for link '{componentStatePropertyLink}'.");
            }

            if (contractComponents.Length > 1)
            {
                Log.Warning(LogContextEnum.Configuration, $"More than 1 component matches link '{componentStatePropertyLink}': {string.Join(", ", contractComponents.Select(x => $"'{x.Component.FullName}'"))}. Specify component role.");
            }

            var componentState = contractComponents[0].ContractState;
            if (componentState == null)
            {
                throw new ComponentConfigurationException($"Contract state was not found or is not implemented (link '{componentStatePropertyLink}').");
            }

            return componentState;
        }

        #endregion

        #region Service Helpers

        public ServiceMonitorableComponentInfo[] Service_GetMonitorableComponents()
        {
            return _componentsByRole.Values
                .Where(x => x.IsStateMonitorable)
                .OrderBy(x => x.FullName)
                .Select(x => new ServiceMonitorableComponentInfo()
                    {
                        FullName = x.FullName,
                        Status = x.Status,
                    })
                .ToArray();
        }

        #endregion
    }
}