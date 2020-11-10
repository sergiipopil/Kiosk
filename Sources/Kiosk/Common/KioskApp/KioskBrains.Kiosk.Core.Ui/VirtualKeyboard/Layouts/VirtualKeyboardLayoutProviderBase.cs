using System;
using System.Windows.Input;
using Windows.Globalization;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts
{
    public abstract class VirtualKeyboardLayoutProviderBase : IVirtualKeyboardLayoutProvider
    {
        private VirtualKeyboardLayout _layout;

        public VirtualKeyboardLayout Layout
        {
            get => _layout;
            protected set
            {
                _layout = value;
                OnLayoutChanged(value);
            }
        }

        public event EventHandler<VirtualKeyboardLayout> LayoutChanged;

        protected virtual void OnLayoutChanged(VirtualKeyboardLayout layout)
        {
            LayoutChanged?.Invoke(this, layout);
        }

        public abstract void InitLayout(Language currentAppLanguage);

        public abstract void ProcessControlCommand(string controlCommand, ICommand controlKeyCommand);

        public abstract void OnAppLanguageChanged(Language currentAppLanguage);
    }
}