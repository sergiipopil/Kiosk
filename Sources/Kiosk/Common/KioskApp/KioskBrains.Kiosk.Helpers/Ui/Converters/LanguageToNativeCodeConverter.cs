using System;
using Windows.Globalization;
using Windows.UI.Xaml.Data;
using KioskBrains.Kiosk.Helpers.Text;

namespace KioskBrains.Kiosk.Helpers.Ui.Converters
{
    public class LanguageToNativeCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var languageValue = value as Language;
            return languageValue?.NativeName.FirstNSymbols(3).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}