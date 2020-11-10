using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Cart
{
    public sealed partial class CartView : UserControl
    {
        public CartView()
        {
            Cart = EkContext.EkProcess?.Cart;

            InitializeComponent();
        }

        public EkContext EkContext => EkContext.Current;

        public Cart Cart { get; }

        private void CartView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Cart != null)
            {
                Cart.PropertyChanged += Cart_PropertyChanged;
            }
        }

        private void CartView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (Cart != null)
            {
                Cart.PropertyChanged -= Cart_PropertyChanged;
            }
        }

        private void Cart_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Cart.IsEmpty):
                    if (Cart?.IsEmpty == true)
                    {
                        EkContext.ContinueShoppingCommand?.Execute(null);
                    }

                    break;
            }
        }
    }
}