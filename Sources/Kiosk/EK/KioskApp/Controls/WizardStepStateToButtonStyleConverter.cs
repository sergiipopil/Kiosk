using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Controls
{
    public class WizardStepStateToButtonStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = value as WizardStepStateEnum?;
            string styleKey;
            switch (state)
            {
                case WizardStepStateEnum.Completed:
                    styleKey = "WizardStepCompletedButtonStyle";
                    break;
                case WizardStepStateEnum.Active:
                    styleKey = "WizardStepActiveButtonStyle";
                    break;
                case WizardStepStateEnum.Next:
                default:
                    styleKey = "WizardStepNextButtonStyle";
                    break;
            }

            return ResourceHelper.GetStaticResourceFromUIThread<Style>(styleKey);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}