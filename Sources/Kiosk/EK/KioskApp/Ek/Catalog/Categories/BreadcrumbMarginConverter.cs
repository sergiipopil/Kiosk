using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KioskApp.Ek.Catalog.Categories
{
    public class BreadcrumbMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var index = (int)value;
            return new Thickness(index*18, 15, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}