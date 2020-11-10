using System;
using System.Windows.Input;
using Windows.Globalization;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts
{
    /// <summary>
    /// Provides keyboard layout and modifies it on control commands.
    /// </summary>
    public interface IVirtualKeyboardLayoutProvider
    {
        VirtualKeyboardLayout Layout { get; }

        event EventHandler<VirtualKeyboardLayout> LayoutChanged;

        void InitLayout(Language currentAppLanguage);

        void ProcessControlCommand(string controlCommand, ICommand controlKeyCommand);

        void OnAppLanguageChanged(Language currentAppLanguage);
    }
}