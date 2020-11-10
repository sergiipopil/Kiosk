using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KioskApp.Controls
{
    public class WizardStepStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var currentState = value as WizardStepStateEnum?;
            var parameterString = parameter as string;
            if (currentState == null
                || parameterString == null)
            {
                return Visibility.Collapsed;
            }

            var currentStateString = currentState.ToString();
            var targetStates = parameterString
                .Split('+');

            if (targetStates.Contains(currentStateString))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}