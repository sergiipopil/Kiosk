using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Ek.Products.Photos
{
    public class IsSelectedToThumbnailBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isSelected = (bool)value;
            return isSelected
                ? ResourceHelper.GetStaticResourceFromUIThread<Brush>("BlueButtonBrush")
                : new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}