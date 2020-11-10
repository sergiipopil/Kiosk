using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KioskBrains.Kiosk.Helpers.Ui.Converters
{
    public class IsStringNotEmptyToVisibilityConverter : IValueConverter
    {
        public bool IsReversed { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var valueString = value as string;
            var isVisible = !string.IsNullOrEmpty(valueString);
            if (IsReversed)
            {
                isVisible = !isVisible;
            }
            return isVisible
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}