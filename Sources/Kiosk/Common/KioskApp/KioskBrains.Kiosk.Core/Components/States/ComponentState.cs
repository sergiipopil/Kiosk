using System;
using System.Collections.Generic;
using System.Linq;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskBrains.Kiosk.Core.Components.States
{
    public abstract class ComponentState : UiBindableObject
    {
        private readonly object _stateListenersLocker = new object();

        private readonly Dictionary<Guid, ComponentStateChangedHandler> _stateListeners = new Dictionary<Guid, ComponentStateChangedHandler>();

        /// <summary>
        /// Add state change listener. Invoked in separate safe thread.
        /// </summary>
        /// <returns>
        /// Listener key that should be passed to <see cref="RemoveStateListener"/>.
        /// Listener keys are used to allow adding/removing of anonymous methods.
        /// </returns>
        public Guid AddStateListener(ComponentStateChangedHandler stateChangedHandler)
        {
            Assure.ArgumentNotNull(stateChangedHandler, nameof(stateChangedHandler));

            lock (_stateListenersLocker)
            {
                var listenerKey = Guid.NewGuid();
                _stateListeners[listenerKey] = stateChangedHandler;
                return listenerKey;
            }
        }

        public void RemoveStateListener(Guid listenerKey)
        {
            lock (_stateListenersLocker)
            {
                if (!_stateListeners.ContainsKey(listenerKey))
                {
                    Log.Warning(LogContextEnum.Component, $"Non-registered listener key was passed to {nameof(RemoveStateListener)}.");
                    return;
                }
                _stateListeners.Remove(listenerKey);
            }
        }

        /// <summary>
        /// Don't forget to run base.OnOwnPropertyChanged if it overridden on specific State class.
        /// </summary>
        protected override void OnOwnPropertyChanged(string propertyName)
        {
            ComponentStateChangedHandler[] stateListeners;
            lock (_stateListenersLocker)
            {
                if (_stateListeners.Count == 0)
                {
                    return;
                }
                stateListeners = _stateListeners.Values.ToArray();
            }
            foreach (var stateListener in stateListeners)
            {
                ThreadHelper.RunInNewThreadAsync(async () =>
                    {
                        try
                        {
                            await stateListener(propertyName);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(LogContextEnum.Component, $"State '{GetType().FullName}' listener failed.", ex);
                        }
                    });
            }
        }
    }
}