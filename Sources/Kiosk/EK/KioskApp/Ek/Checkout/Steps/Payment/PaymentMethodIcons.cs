using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace KioskApp.Ek.Checkout.Steps.Payment
{
    public static class PaymentMethodIcons
    {
        private static readonly Dictionary<(PaymentMethodEnum, bool), ImageSource> CachedIcons;

        static PaymentMethodIcons()
        {
            CachedIcons = new Dictionary<(PaymentMethodEnum, bool), ImageSource>();
            foreach (var paymentMethod in Enum.GetValues(typeof(PaymentMethodEnum))
                .Cast<PaymentMethodEnum>())
            {
                CachedIcons[(paymentMethod, false)] = new BitmapImage(new Uri($"ms-appx:///Themes/Assets/Images/Checkout/Payment/{paymentMethod}Small.png"));
                CachedIcons[(paymentMethod, true)] = new BitmapImage(new Uri($"ms-appx:///Themes/Assets/Images/Checkout/Payment/{paymentMethod}Big.png"));
            }
        }

        public static ImageSource GetIcon(PaymentMethodEnum paymentMethod, bool isBig)
        {
            var icon = CachedIcons.TryGetValue((paymentMethod, isBig), out var image)
                ? image
                : null;

            return icon;
        }
    }
}