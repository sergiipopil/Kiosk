using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Controls
{
    public class LanguageSelectorItemStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isSelected = (bool)value;
            return ResourceHelper.GetStaticResourceFromUIThread<Style>(
                isSelected
                    ? "SelectedLanguageSelectorItemButtonStyle"
                    : "LanguageSelectorItemButtonStyle");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}