using System;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.KioskConfiguration;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Components.Initialization;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Core.Components.States
{
    public class ComponentStatePropertyLinkWrapper
    {
        private readonly ComponentStatePropertyLink _propertyLink;

        public ComponentStatePropertyLinkWrapper(ComponentStatePropertyLink propertyLink)
        {
            Assure.ArgumentNotNull(propertyLink, nameof(propertyLink));
            if (string.IsNullOrEmpty(propertyLink.ContractTypeName))
            {
                throw new ComponentConfigurationException($"Invalid state - {nameof(ComponentStatePropertyLink)}.{nameof(ComponentStatePropertyLink.ContractTypeName)} is not specified.");
            }
            if (string.IsNullOrEmpty(propertyLink.PropertyName))
            {
                throw new ComponentConfigurationException($"Invalid state - {nameof(ComponentStatePropertyLink)}.{nameof(ComponentStatePropertyLink.PropertyName)} is not specified.");
            }

            _propertyLink = propertyLink;
        }

        #region Wrapper

        public string ComponentRole => _propertyLink.ComponentRole;

        public string ContractTypeName => _propertyLink.ContractTypeName;

        public string PropertyName => _propertyLink.PropertyName;

        public override string ToString()
        {
            return _propertyLink.ToString();
        }

        #endregion

        private Guid? _listenerKey;

        /// <summary>
        /// Not thread-safe.
        /// </summary>
        public void AddStatePropertyListener<TProperty>(Func<TProperty, Task> propertyChangeHandler, bool runHandlerWithCurrentValue = true)
        {
            Assure.ArgumentNotNull(propertyChangeHandler, nameof(propertyChangeHandler));
            // not thread safe since await-s are used below (thread safe behavior would make the implementation more complicated)
            Assure.CheckFlowState(_listenerKey == null, "Listener was already added.");

            var componentState = ComponentManager.Current.GetContractState(this);
            var componentStateType = componentState.GetType();
            var propertyInfo = componentStateType.GetProperty(PropertyName);
            if (propertyInfo == null
                || !propertyInfo.CanRead
                || propertyInfo.PropertyType != typeof(TProperty))
            {
                throw new ComponentConfigurationException($"'{componentStateType.FullName}' doesn't contain public readable property '{PropertyName}' of type '{typeof(TProperty).FullName}'.");
            }

            // TBD: probably it's faster to read the value via compiled expressions
            TProperty ValueReader() => (TProperty)propertyInfo.GetValue(componentState);

            async Task StateChangeHandler(string propertyName)
            {
                if (propertyName == PropertyName)
                {
                    try
                    {
                        var value = ValueReader();
                        await propertyChangeHandler(value);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogContextEnum.Component, $"Property change listener of link '{this}' failed.", ex);
                    }
                }
            }

            _listenerKey = componentState.AddStateListener(StateChangeHandler);

            if (runHandlerWithCurrentValue)
            {
                ThreadHelper.RunInNewThreadAsync(() => StateChangeHandler(PropertyName));
            }
        }
    }
}