using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KioskApp.Controls
{
    public class WizardStepStateToShadowVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = value as WizardStepStateEnum?;
            switch (state)
            {
                case WizardStepStateEnum.Completed:
                case WizardStepStateEnum.Active:
                    return Visibility.Visible;

                case WizardStepStateEnum.Next:
                default:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}