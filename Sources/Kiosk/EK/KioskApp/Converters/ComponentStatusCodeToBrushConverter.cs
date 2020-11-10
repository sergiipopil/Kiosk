using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using KioskBrains.Common.KioskState;

namespace KioskApp.Converters
{
    public class ComponentStatusCodeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var statusCode = value as ComponentStatusCodeEnum?;
            Color color;
            switch (statusCode)
            {
                case ComponentStatusCodeEnum.Ok:
                    color = Colors.Green;
                    break;
                case ComponentStatusCodeEnum.Warning:
                    color = Colors.Orange;
                    break;
                case ComponentStatusCodeEnum.Error:
                    color = Colors.Red;
                    break;
                case ComponentStatusCodeEnum.Disabled:
                    color = Colors.DarkRed;
                    break;
                case ComponentStatusCodeEnum.Undefined:
                default:
                    color = Colors.DimGray;
                    break;
            }
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}