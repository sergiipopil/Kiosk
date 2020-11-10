using System;
using Windows.UI.Xaml.Data;
using KioskBrains.Kiosk.Helpers.Text;

namespace KioskBrains.Kiosk.Helpers.Ui.Converters
{
    public class StringToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var type = parameter as string;
            switch (type)
            {
                case "first-char-only":
                    return value?.ToString()?.FirstCharToUpper();
                default:
                    return value?.ToString()?.ToUpper();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}