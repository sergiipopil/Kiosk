using Windows.UI.Xaml;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard
{
    public class BasicVirtualKeyButtonStyleSelector : IVirtualKeyButtonStyleSelector
    {
        public Style Text { get; set; }

        public Style Special { get; set; }

        public Style Control { get; set; }

        public Style Placeholder { get; set; }

        public Style SelectStyle(VirtualKey virtualKey)
        {
            switch (virtualKey?.Type)
            {
                case VirtualKeyTypeEnum.Text:
                    return Text;
                case VirtualKeyTypeEnum.Special:
                    return Special;
                case VirtualKeyTypeEnum.Control:
                    return Control;
                case VirtualKeyTypeEnum.Placeholder:
                    return Placeholder;
                default:
                    return null;
            }
        }
    }
}