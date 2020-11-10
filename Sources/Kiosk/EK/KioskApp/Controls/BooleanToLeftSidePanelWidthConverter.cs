using System;
using Windows.UI.Xaml.Data;

namespace KioskApp.Controls
{
    public class BooleanToLeftSidePanelWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isExtended = value as bool? == true;
            return isExtended ? 849d : 688d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}