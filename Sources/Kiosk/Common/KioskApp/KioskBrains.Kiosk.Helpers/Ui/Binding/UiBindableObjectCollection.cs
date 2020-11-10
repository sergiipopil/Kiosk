using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Helpers.Ui.Binding
{
    /// <summary>
    /// Derives <see cref="UiBindableCollection{TItem}"/> and adds <see cref="UiBindableObject"/> behavior.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class UiBindableObjectCollection<TItem> : UiBindableCollection<TItem>
    {
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

            OnBindablePropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnOwnPropertyChanged(string propertyName)
        {
        }

        protected void OnBindablePropertyChanged(string propertyName = null)
        {
            ThreadHelper.RunInUiThreadAsync(() =>
                {
                    try
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
                    }
                    catch (Exception ex)
                    {
                        // log subscriber errors
                        Log.Error(LogContextEnum.Application, $"'{GetType().FullName}'.{nameof(PropertyChanged)}('{propertyName}') failed (UI thread).", ex);
                        throw;
                    }
                });
        }
    }
}