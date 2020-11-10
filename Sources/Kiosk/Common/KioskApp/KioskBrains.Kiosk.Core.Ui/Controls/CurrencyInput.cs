using Windows.UI.Xaml;

namespace KioskBrains.Kiosk.Core.Ui.Controls
{
    public class CurrencyInput : IntegerInput
    {
        public CurrencyInput()
        {
            DefaultStyleKey = typeof(CurrencyInput);
        }

        #region CurrencySymbolMargin

        public static readonly DependencyProperty CurrencySymbolMarginProperty = DependencyProperty.Register(
            nameof(CurrencySymbolMargin), typeof(Thickness), typeof(CurrencyInput), new PropertyMetadata(default(Thickness)));

        public Thickness CurrencySymbolMargin
        {
            get => (Thickness)GetValue(CurrencySymbolMarginProperty);
            set => SetValue(CurrencySymbolMarginProperty, value);
        }

        #endregion  

        #region CurrencySymbol

        public static readonly DependencyProperty CurrencySymbolProperty = DependencyProperty.Register(
            nameof(CurrencySymbol), typeof(string), typeof(CurrencyInput), new PropertyMetadata(default(string)));

        public string CurrencySymbol
        {
            get => (string)GetValue(CurrencySymbolProperty);
            set => SetValue(CurrencySymbolProperty, value);
        }

        #endregion  
    }
}