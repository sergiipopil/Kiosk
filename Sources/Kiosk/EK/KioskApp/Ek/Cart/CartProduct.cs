using KioskApp.Search;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Cart
{
    /// <summary>
    /// Set <see cref="Quantity"/> to 0 to remove the <see cref="CartProduct"/> from the <see cref="Cart"/>.
    /// </summary>
    public class CartProduct : UiBindableObject
    {
        private readonly Cart _cart;

        public CartProduct(Cart cart, Product product)
        {
            Assure.ArgumentNotNull(cart, nameof(cart));
            Assure.ArgumentNotNull(product, nameof(product));

            _cart = cart;
            Product = product;
        }

        public Product Product { get; }

        private const int MaxQuantityPerProduct = 20;

        #region Quantity

        private int _Quantity;

        public int Quantity
        {
            get => _Quantity;
            set
            {
                if (value > MaxQuantityPerProduct)
                {
                    return;
                }

                SetProperty(ref _Quantity, value);
            }
        }

        #endregion

        #region PriceString

        private string _PriceString;

        public string PriceString
        {
            get => _PriceString;
            set => SetProperty(ref _PriceString, value);
        }

        #endregion

        protected override void OnOwnPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Quantity):
                    OnQuantityChanged();
                    break;
            }
        }

        private void OnQuantityChanged()
        {
            _cart.OnProductQuantityChanged(this);
        }
    }
}