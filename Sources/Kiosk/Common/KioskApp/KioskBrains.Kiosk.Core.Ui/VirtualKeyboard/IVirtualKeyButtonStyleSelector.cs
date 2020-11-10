using Windows.UI.Xaml;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard
{
    public interface IVirtualKeyButtonStyleSelector
    {
        /// <summary>
        /// Invoked in UI thread.
        /// </summary>
        Style SelectStyle(VirtualKey virtualKey);
    }
}