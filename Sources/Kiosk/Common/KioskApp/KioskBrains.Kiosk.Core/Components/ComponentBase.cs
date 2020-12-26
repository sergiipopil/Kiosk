using System;
using System.Threading.Tasks;
using KioskBrains.Common.Api;
using KioskBrains.Common.KioskConfiguration;
using KioskBrains.Common.KioskState;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Components.Dependencies;
using KioskBrains.Kiosk.Core.Components.Initialization;
using KioskBrains.Kiosk.Core.Components.Operations;
using KioskBrains.Kiosk.Core.Components.Statuses;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui.Binding;
using Newtonsoft.Json;

namespace KioskBrains.Kiosk.Core.Components
{
    public abstract class ComponentBase : UiBindableObject
    {
        public string FullName => $"{ComponentRole} ({ComponentName})";

        public string ComponentRole { get; internal set; }

        public string ComponentName { get; internal set; }

        public BindableComponentStatus Status { get; } = new BindableComponentStatus();

        public const string InitializeOperationName = "Initialize";

        internal Task<Type[]> InitializeAndGetSupportedContractsAsync(ComponentSettings settings)
        {
            return ThreadHelper.RunInNewThreadAsync(async () =>
            {
                // operation context emulation, since InitializeAsync often uses context-dependent methods
                using (var context = new ComponentOperationContext(this, InitializeOperationName))
                {
                    try
                    {
                        Status.SetSelfStatus(ComponentStatusCodeEnum.Disabled, "Initializing...");
                        await InitializeAsync(new ComponentInitializeRequest(settings), context);
                    }
                    catch (Exception ex)
                    {
                        Status.SetSelfStatus(ComponentStatusCodeEnum.Error, $"Initialization failed: {ex.Message + "//Source:" + ex.Source + "//InnerException:" + ex.InnerException + "//StackTrace:" + ex.StackTrace + "//HelpLink:" + ex.HelpLink}");
                        context.Log.Error(LogContextEnum.Configuration, "Unhandled exception.", ex, callerName: null);
                    }

                    try
                    {
                        return GetSupportedContracts();
                    }
                    catch (Exception ex)
                    {
                        context.Log.Error(LogContextEnum.Configuration, "Unhandled exception.", ex, callerName: nameof(GetSupportedContracts));
                        return null;
                    }
                }
            });
        }

        /// <summary>
        /// Initialize component and set initial self status.
        /// </summary>
        protected abstract Task<ComponentInitializeResponse> InitializeAsync(ComponentInitializeRequest request, ComponentOperationContext context);

        /// <summary>
        /// Provides a list of supported contracts.
        /// </summary>
        protected abstract Type[] GetSupportedContracts();

        public const string InitializeContractDependenciesOperationName = "InitializeContractDependencies";

        internal Task InitializeContractDependenciesInternalAsync()
        {
            if (this is IDependentComponent dependentComponent)
            {
                return ThreadHelper.RunInNewThreadAsync(async () =>
                {
                    // operation context emulation, since InitializeContractDependenciesAsync can use context-dependent methods
                    using (var context = new ComponentOperationContext(this, InitializeContractDependenciesOperationName))
                    {
                        try
                        {
                            await dependentComponent.InitializeContractDependenciesAsync(new EmptyRequest(), context);
                        }
                        catch (Exception ex)
                        {
                            Status.SetDependencyBasedStatus(ComponentStatusCodeEnum.Error, $"Initialization of dependencies failed: {ex.Message}");
                            context.Log.Error(LogContextEnum.Configuration, "Unhandled exception.", ex, callerName: null);
                        }
                    }
                });
            }

            return Task.CompletedTask;
        }

        #region Operation Start/End Handlers

        internal Task RunOnComponentOperationStartAsync(ComponentOperationContext context)
        {
            return OnComponentOperationStartAsync(context);
        }

        protected virtual Task OnComponentOperationStartAsync(ComponentOperationContext context)
        {
            return Task.CompletedTask;
        }

        internal Task RunOnComponentOperationEndAsync(ComponentOperationContext context)
        {
            return OnComponentOperationEndAsync(context);
        }

        protected virtual Task OnComponentOperationEndAsync(ComponentOperationContext context)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Monitorable State

        public virtual bool IsStateMonitorable { get; } = true;

        #region SpecificMonitorableState

        private object _SpecificMonitorableState;

        /// <summary>
        /// Only for testing purposes.
        /// </summary>
        public object SpecificMonitorableState
        {
            get => _SpecificMonitorableState;
            private set => SetProperty(ref _SpecificMonitorableState, value);
        }

        #endregion

        protected void SetSpecificMonitorableState(object specificMonitorableState)
        {
            SpecificMonitorableState = specificMonitorableState;
        }

        internal ComponentMonitorableState GetComponentMonitorableState()
        {
            return new ComponentMonitorableState()
            {
                ComponentName = FullName,
                Status = new ComponentStatus()
                {
                    Code = Status.Code,
                    Message = Status.Message,
                    StatusLocalTime = Status.StatusTime,
                },
                SpecificMonitorableStateJson = SpecificMonitorableState == null
                    ? null
                    : JsonConvert.SerializeObject(SpecificMonitorableState),
            };
        }

        #endregion

        private readonly object _componentLockLocker = new object();

        private ComponentLock _componentLock;

        //  todo: refactor - add more enhanced locking mechanism
        public ComponentLock Lock()
        {
            lock (_componentLockLocker)
            {
                if (_componentLock != null)
                {
                    throw new ComponentLockedException(FullName);
                }

                _componentLock = new ComponentLock(ReleaseLock);
                return _componentLock;
            }
        }

        private void ReleaseLock()
        {
            lock (_componentLockLocker)
            {
                _componentLock = null;
            }
        }
    }
}