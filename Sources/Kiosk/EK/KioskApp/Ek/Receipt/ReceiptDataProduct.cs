using KioskApp.Search;
using KioskBrains.Common.Contracts;

namespace KioskApp.Ek.Receipt
{
    public class ReceiptDataProduct
    {
        public ReceiptDataProduct(Product product, int quantity)
        {
            Assure.ArgumentNotNull(product, nameof(product));

            Name = product.Name;
            Quantity = quantity;
            Price = product.Price;
            PriceCurrencyCode = product.EkProduct?.PriceCurrencyCode;
        }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string PriceCurrencyCode { get; set; }
    }
}