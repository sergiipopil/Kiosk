using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Helpers.Ui.Binding
{
    /// <summary>
    /// Based on https://github.com/Microsoft/Windows-appsample-networkhelper/blob/master/DemoApps/QuizGame/Common/BindableBase.cs
    /// </summary>
    public abstract class UiBindableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Event handler is run in UI thread.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<TValue>(ref TValue storage, TValue value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            try
            {
                OnOwnPropertyChanged(propertyName);
            }
            catch (Exception ex)
            {
                // log subscriber errors
                Log.Error(LogContextEnum.Application, $"'{GetType().FullName}'.{nameof(OnOwnPropertyChanged)}('{propertyName}') failed.", ex);
                throw;
            }
            OnPropertyChangedAsync(propertyName);
            return true;
        }

        protected virtual void OnOwnPropertyChanged(string propertyName)
        {
        }

        protected Task OnPropertyChangedAsync(string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                ThreadHelper.RunInUiThreadAsync(() =>
                    {
                        try
                        {
                            // we can't be sure that PropertyChanged is not null at this moment
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                        }
                        catch (Exception ex)
                        {
                            // log subscriber errors
                            Log.Error(LogContextEnum.Application, $"'{GetType().FullName}'.{nameof(PropertyChanged)}('{propertyName}') failed (UI thread).", ex);
                            throw;
                        }
                    });
            }
            return Task.CompletedTask;
        }
    }
}