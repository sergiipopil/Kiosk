using System;
using Windows.UI.Xaml.Data;

namespace KioskBrains.Kiosk.Helpers.Ui.Converters
{
    public class BooleanNotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var typedValue = System.Convert.ToBoolean(value);
            return !typedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}