using System;
using System.Windows.Input;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;

namespace KioskBrains.Kiosk.Helpers.Ui
{
    public class RelayCommand : ICommand
    {
        public RelayCommand(string name, Action<object> handler)
        {
            Assure.ArgumentNotNull(handler, nameof(handler));

            _name = name;
            _handler = handler;
        }

        private readonly string _name;

        private readonly Action<object> _handler;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                _handler(parameter);
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Ui, $"{nameof(RelayCommand)} '{_name}' handler failed.", ex);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}