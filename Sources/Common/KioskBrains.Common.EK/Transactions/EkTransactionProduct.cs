using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;

namespace KioskBrains.Common.EK.Transactions
{
    public class EkTransactionProduct
    {
        public string key { get; set; }

        public string brandName { get; set; }

        public string partNumber { get; set; }

        public EkProductSourceEnum source { get; set; }

        public string sourceId { get; set; }

        /// <summary>
        /// Used for Allegro-based product price calculations.
        /// </summary>
        public string categoryId { get; set; }

        public MultiLanguageString name { get; set; }

        public MultiLanguageString description { get; set; }

        public string thumbnailUrl { get; set; }

        public decimal basePrice { get; set; }

        public string basePriceCurrencyCode { get; set; }

        public decimal deliveryPrice { get; set; }

        public decimal price { get; set; }
        public decimal finalPrice { get; set; }
        public string priceCurrencyCode { get; set; }

        public string priceCalculationInfo { get; set; }

        public EkProductStateEnum state { get; set; }

        public int quantity { get; set; }

        public static EkTransactionProduct FromProduct(EkProduct product, MultiLanguageString description, int quantity)
        {
            Assure.ArgumentNotNull(product, nameof(product));

            return new EkTransactionProduct()
                {
                   key = product.Key,
                    source = product.Source,
                    sourceId = product.SourceId,
                    brandName = product.BrandName,
                    partNumber = product.PartNumber,
                    categoryId = product.CategoryId,
                    name = product.Name,
                    description = description,
                    thumbnailUrl = product.GetThumbnailUrl(),
                    basePrice = product.BasePrice,
                    basePriceCurrencyCode = product.BasePriceCurrencyCode,
                    deliveryPrice = product.DeliveryPrice,
                    price = product.Price,
                    priceCurrencyCode = product.PriceCurrencyCode,
                    priceCalculationInfo = product.PriceCalculationInfo,
                    state = product.State,
                    quantity = quantity,
                };
        }
    }
}