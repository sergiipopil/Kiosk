using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Keys;

namespace KioskBrains.Kiosk.Core.Ui.VirtualKeyboard
{
    public class VirtualKeyToButtonStyleConverter : DependencyObject, IValueConverter
    {
        #region KeyButtonStyleSelector

        public static readonly DependencyProperty KeyButtonStyleSelectorProperty = DependencyProperty.Register(
            nameof(KeyButtonStyleSelector), typeof(IVirtualKeyButtonStyleSelector), typeof(VirtualKeyToButtonStyleConverter), new PropertyMetadata(default(IVirtualKeyButtonStyleSelector)));

        public IVirtualKeyButtonStyleSelector KeyButtonStyleSelector
        {
            get => (IVirtualKeyButtonStyleSelector)GetValue(KeyButtonStyleSelectorProperty);
            set => SetValue(KeyButtonStyleSelectorProperty, value);
        }

        #endregion

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var virtualKey = value as VirtualKey;
            if (virtualKey == null)
            {
                return null;
            }

            return KeyButtonStyleSelector?.SelectStyle(virtualKey);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}