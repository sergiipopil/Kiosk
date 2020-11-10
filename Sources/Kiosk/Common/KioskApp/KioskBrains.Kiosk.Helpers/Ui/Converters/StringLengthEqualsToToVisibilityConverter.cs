using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KioskBrains.Kiosk.Helpers.Ui.Converters
{
    public class StringLengthEqualsToToVisibilityConverter : IValueConverter
    {
        public bool IsReversed { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var strValue = value as string;
            var stringLength = strValue?.Length;
            var strLengthParam = parameter as string;
            int.TryParse(strLengthParam, out var intLengthParam);

            var boolResult = stringLength == intLengthParam;
            if (IsReversed)
            {
                boolResult = !boolResult;
            }

            return boolResult ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}