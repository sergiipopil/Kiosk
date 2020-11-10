using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KioskBrains.Kiosk.Helpers.Ui.Converters
{
    public class IsEqualByStringToVisibilityConverter : IValueConverter
    {
        public bool IsReversed { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isEqualConverter = new IsEqualByStringConverter()
                {
                    IsReversed = IsReversed
                };
            var comparisonResult = isEqualConverter.Convert(value, targetType, parameter, language);
            return (bool)comparisonResult ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}