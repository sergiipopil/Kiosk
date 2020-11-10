using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Kiosk.Core.Modals;

namespace KioskApp.Ek.Cart.PromoCode
{
    public sealed partial class PromoCodePresenter : UserControl
    {
        public PromoCodePresenter()
        {
            InitializeComponent();
        }

        #region PromoCode

        public static readonly DependencyProperty PromoCodeProperty = DependencyProperty.Register(
            nameof(PromoCode), typeof(string), typeof(PromoCodePresenter), new PropertyMetadata(default(string)));

        public string PromoCode
        {
            get => (string)GetValue(PromoCodeProperty);
            set => SetValue(PromoCodeProperty, value);
        }

        #endregion

        private void EnterPromoCodeButton_OnClick(object sender, RoutedEventArgs e)
        {
#pragma warning disable 4014
            ModalManager.Current.ShowModalAsync(new ModalArgs(
#pragma warning restore 4014
                "PromoCodeEntry",
                context =>
                    {
                        var modal = new PromoCodeEntryModal(context)
                            {
                                PromoCode = PromoCode,
                            };
                        modal.PromoCodeConfirmed += (s, promoCode) => { PromoCode = promoCode; };
                        return modal;
                    },
                showCancelButton: false));
        }
    }
}