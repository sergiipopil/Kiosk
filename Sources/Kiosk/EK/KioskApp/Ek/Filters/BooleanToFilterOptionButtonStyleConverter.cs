using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Ek.Filters
{
    public class BooleanToFilterOptionButtonStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isSelected = value as bool?;
            var styleKey = isSelected == true
                ? "FilterSelectedOptionButtonStyle"
                : "FilterOptionButtonStyle";
            return ResourceHelper.GetStaticResourceFromUIThread<Style>(styleKey);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}