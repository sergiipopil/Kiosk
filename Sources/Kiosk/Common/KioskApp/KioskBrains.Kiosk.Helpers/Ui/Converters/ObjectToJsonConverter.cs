using System;
using Windows.UI.Xaml.Data;
using Newtonsoft.Json;

namespace KioskBrains.Kiosk.Helpers.Ui.Converters
{
    /// <summary>
    /// Intended for usage in POC applications.
    /// Should be instantiated manually.
    /// </summary>
    public class ObjectToJsonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return JsonConvert.SerializeObject(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}