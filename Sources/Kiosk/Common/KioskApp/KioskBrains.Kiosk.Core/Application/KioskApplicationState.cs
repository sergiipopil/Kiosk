using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Components.States;

namespace KioskBrains.Kiosk.Core.Application
{
    public class KioskApplicationState : ComponentState
    {
        private readonly object _applicationStateLocker = new object();

        #region ApplicationState

        private KioskApplicationStateEnum _ApplicationState;

        public KioskApplicationStateEnum ApplicationState
        {
            get => _ApplicationState;
            set
            {
                lock (_applicationStateLocker)
                {
                    SetProperty(ref _ApplicationState, value);
                }
            }
        }

        #endregion

        protected override void OnOwnPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(ApplicationState):
                    OnApplicationStateChanged(ApplicationState);
                    break;
            }
        }

        private readonly Dictionary<KioskApplicationStateEnum, List<TaskCompletionSource<KioskApplicationStateEnum>>> _applicationStateWaiters
            = new Dictionary<KioskApplicationStateEnum, List<TaskCompletionSource<KioskApplicationStateEnum>>>();

        private void OnApplicationStateChanged(KioskApplicationStateEnum newApplicationState)
        {
            try
            {
                // just is case - redundant since locker is set around ApplicationState setter and the method is invoked inside the setter
                lock (_applicationStateLocker)
                {
                    if (!_applicationStateWaiters.ContainsKey(newApplicationState))
                    {
                        return;
                    }

                    var waitersList = _applicationStateWaiters[newApplicationState];
                    if (waitersList.Count == 0)
                    {
                        return;
                    }

                    foreach (var taskCompletionSource in waitersList)
                    {
                        taskCompletionSource.SetResult(newApplicationState);
                    }

                    waitersList.Clear();
                }
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Application, $"{nameof(OnApplicationStateChanged)} failed.", ex);
            }
        }

        public Task WaitForApplicationStateAsync(KioskApplicationStateEnum applicationState)
        {
            lock (_applicationStateLocker)
            {
                if (ApplicationState == applicationState)
                {
                    return Task.CompletedTask;
                }

                if (!_applicationStateWaiters.ContainsKey(applicationState))
                {
                    _applicationStateWaiters[applicationState] = new List<TaskCompletionSource<KioskApplicationStateEnum>>();
                }

                var waitersList = _applicationStateWaiters[applicationState];
                var taskCompletionSource = new TaskCompletionSource<KioskApplicationStateEnum>();
                waitersList.Add(taskCompletionSource);

                return taskCompletionSource.Task;
            }
        }
    }
}