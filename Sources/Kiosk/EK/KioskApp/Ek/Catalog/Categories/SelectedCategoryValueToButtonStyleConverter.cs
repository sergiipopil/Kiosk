using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Ek.Catalog.Categories
{
    public class SelectedCategoryValueToButtonStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var styleKey = value == null
                ? "SelectedCategoryEmptyValueButtonStyle"
                : "SelectedCategoryValueButtonStyle";

            return ResourceHelper.GetStaticResourceFromUIThread<Style>(styleKey);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}