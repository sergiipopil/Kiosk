using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Cart
{
    public sealed partial class CartWidget : UserControl
    {
        public CartWidget()
        {
            InitializeComponent();
        }

        public EkContext EkContext => EkContext.Current;
    }
}