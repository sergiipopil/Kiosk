using System;
using System.Linq;
using System.Windows.Input;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Clients.Ek4Car.Models
{
    public class Cart
    {
        //    public Cart()
        //    {
        //        Products = new UiBindableCollection<CartProduct>();

        //        AddToCartCommand = new RelayCommand(
        //            nameof(AddToCartCommand),
        //            parameter =>
        //            {
        //                var product = parameter as Product;
        //                if (product == null)
        //                {
        //                    return;
        //                }

        //                AddProduct(product);
        //            });

        //        OnCartChanged(isInit: true);
        //    }

        //    #region Products

        //    private UiBindableCollection<CartProduct> _Products;

        //    public UiBindableCollection<CartProduct> Products
        //    {
        //        get => _Products;
        //        set => SetProperty(ref _Products, value);
        //    }

        //    #endregion

        //    private readonly object _cartLocker = new object();

        //    public bool IsProductInCart(Product product)
        //    {
        //        Assure.ArgumentNotNull(product, nameof(product));

        //        return GetCartProductByKey(product.Key) != null;
        //    }

        //    private CartProduct GetCartProductByKey(string productKey)
        //    {
        //        lock (_cartLocker)
        //        {
        //            return Products.FirstOrDefault(x => x.Product.Key == productKey);
        //        }
        //    }

        //    private const int MaxProductsInCart = 20;

        //    private void AddProduct(Product product)
        //    {
        //        Assure.ArgumentNotNull(product, nameof(product));

        //        lock (_cartLocker)
        //        {
        //            var cartProduct = GetCartProductByKey(product.Key);
        //            if (cartProduct == null)
        //            {
        //                if (Products.Count + 1 > MaxProductsInCart)
        //                {
        //                    return;
        //                }

        //                cartProduct = new CartProduct(this, product)
        //                {
        //                    Quantity = 1,
        //                };
        //                UpdateCartProductPriceString(cartProduct);
        //                Products.Add(cartProduct);

        //                OnCartChanged();
        //            }
        //            else
        //            {
        //                cartProduct.Quantity++;

        //                // cart is updated by OnProductQuantityChanged
        //            }
        //        }
        //    }

        //    public void OnProductQuantityChanged(CartProduct cartProduct)
        //    {
        //        lock (_cartLocker)
        //        {
        //            if (cartProduct.Quantity <= 0)
        //            {
        //                Products.Remove(cartProduct);
        //            }
        //            else
        //            {
        //                UpdateCartProductPriceString(cartProduct);
        //            }

        //            OnCartChanged();
        //        }
        //    }

        //    private void UpdateCartProductPriceString(CartProduct cartProduct)
        //    {
        //        cartProduct.PriceString = (cartProduct.Quantity * cartProduct.Product.Price).ToAmountStringWithSpaces();
        //    }

        //    public decimal TotalPrice { get; set; }

        //    #region TotalPriceString

        //    private string _TotalPriceString;

        //    public string TotalPriceString
        //    {
        //        get => _TotalPriceString;
        //        set => SetProperty(ref _TotalPriceString, value);
        //    }

        //    #endregion

        //    #region TotalQuantity

        //    private int _TotalQuantity;

        //    public int TotalQuantity
        //    {
        //        get => _TotalQuantity;
        //        set => SetProperty(ref _TotalQuantity, value);
        //    }

        //    #endregion

        //    #region IsEmpty

        //    private bool _IsEmpty;

        //    public bool IsEmpty
        //    {
        //        get => _IsEmpty;
        //        set => SetProperty(ref _IsEmpty, value);
        //    }

        //    #endregion

        //    #region PromoCode

        //    private string _PromoCode;

        //    public string PromoCode
        //    {
        //        get => _PromoCode;
        //        set => SetProperty(ref _PromoCode, value);
        //    }

        //    #endregion

        //    protected override void OnOwnPropertyChanged(string propertyName)
        //    {
        //        switch (propertyName)
        //        {
        //            case nameof(PromoCode):
        //                OnPromoCodeChanged();
        //                break;
        //        }
        //    }

        //    private void OnCartChanged(bool isInit = false)
        //    {
        //        lock (_cartLocker)
        //        {
        //            IsEmpty = Products.Count == 0;
        //            var totalProductPrice = Products.Sum(x => x.Product.Price * x.Quantity);
        //            TotalPrice = totalProductPrice;
        //            TotalPriceString = TotalPrice.ToAmountStringWithSpaces();
        //            TotalQuantity = Products.Sum(x => x.Quantity);

        //            if (!isInit)
        //            {
        //                CartChanged?.Invoke(this, EventArgs.Empty);
        //            }
        //        }
        //    }

        //    public ICommand AddToCartCommand { get; }

        //    public event EventHandler CartChanged;

        //    public event EventHandler PromoCodeChanged;

        //    protected virtual void OnPromoCodeChanged()
        //    {
        //        PromoCodeChanged?.Invoke(this, EventArgs.Empty);
        //    }
        //}
    }
}
