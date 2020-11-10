using Windows.UI.Xaml.Media;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys
{
    public class VirtualKey
    {
        public VirtualKeyTypeEnum Type { get; set; }

        public string Value { get; set; }

        public VirtualSpecialKeyTypeEnum? SpecialKeyType { get; set; }

        public string ControlCommand { get; set; }

        public ImageSource Icon { get; set; }
    }
}