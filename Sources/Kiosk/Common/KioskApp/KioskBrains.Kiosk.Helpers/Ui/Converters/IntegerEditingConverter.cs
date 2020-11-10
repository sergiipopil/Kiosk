using System;
using KioskBrains.Common.Helpers.Text;
using Windows.UI.Xaml.Data;

namespace KioskBrains.Kiosk.Helpers.Ui.Converters
{
    public class IntegerEditingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value?.ToString().ParseIntOrNull() ?? 0;
        }
    }
}