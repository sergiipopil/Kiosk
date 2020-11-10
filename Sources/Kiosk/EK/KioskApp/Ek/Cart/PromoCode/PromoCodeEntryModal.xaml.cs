using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Core.Modals;

namespace KioskApp.Ek.Cart.PromoCode
{
    public sealed partial class PromoCodeEntryModal : UserControl
    {
        private readonly ModalContext _modalContext;

        public PromoCodeEntryModal(ModalContext modalContext)
        {
            Assure.ArgumentNotNull(modalContext, nameof(modalContext));

            _modalContext = modalContext;

            InitializeComponent();
        }

        #region PromoCode

        public static readonly DependencyProperty PromoCodeProperty = DependencyProperty.Register(
            nameof(PromoCode), typeof(string), typeof(PromoCodeEntryModal), new PropertyMetadata(default(string)));

        public string PromoCode
        {
            get => (string)GetValue(PromoCodeProperty);
            set => SetValue(PromoCodeProperty, value);
        }

        #endregion

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var promoCode = PromoCode;
            if (string.IsNullOrWhiteSpace(promoCode))
            {
                promoCode = null;
            }

            OnPromoCodeConfirmed(promoCode);

            _modalContext.CloseModalAsync();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _modalContext.CloseModalAsync();
        }

        public event EventHandler<string> PromoCodeConfirmed;

        private void OnPromoCodeConfirmed(string promoCode)
        {
            PromoCodeConfirmed?.Invoke(this, promoCode);
        }
    }
}