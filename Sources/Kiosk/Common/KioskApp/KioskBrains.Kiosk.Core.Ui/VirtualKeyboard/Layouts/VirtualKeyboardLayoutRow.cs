using System.Collections.Generic;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts
{
    public class VirtualKeyboardLayoutRow : List<VirtualKey>
    {
        public VirtualKeyboardLayoutRow()
        {
        }

        public VirtualKeyboardLayoutRow(IEnumerable<VirtualKey> virtualKeys)
            : base(virtualKeys)
        {
        }
    }
}