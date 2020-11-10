using System;
using Windows.UI.Xaml.Data;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard
{
    public class VirtualSpecialKeyToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var virtualKey = value as VirtualKey;
            if (virtualKey?.Type != VirtualKeyTypeEnum.Special
                || virtualKey.SpecialKeyType == null)
            {
                return null;
            }

            switch (virtualKey.SpecialKeyType.Value)
            {
                case VirtualSpecialKeyTypeEnum.Space:
                    // no symbol
                    return null;
                case VirtualSpecialKeyTypeEnum.Backspace:
                    return "\xE750";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}