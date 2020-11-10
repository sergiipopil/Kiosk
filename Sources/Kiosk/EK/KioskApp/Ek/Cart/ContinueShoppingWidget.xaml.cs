using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Cart
{
    public sealed partial class ContinueShoppingWidget : UserControl
    {
        public ContinueShoppingWidget()
        {
            InitializeComponent();
        }

        public EkContext EkContext => EkContext.Current;
    }
}