using System;
using Windows.UI.Xaml.Data;

namespace KioskBrains.Kiosk.Helpers.Ui.Converters
{
    public class IsNullToBooleanConverter : IValueConverter
    {
        public bool IsReversed { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (this.IsReversed)
            {
                return value != null;
            }
            else
            {
                return value == null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}