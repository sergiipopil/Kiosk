using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;

namespace KioskBrains.Common.EK.Transactions
{
    public class EkTransactionProduct
    {
        public string Key { get; set; }

        public string BrandName { get; set; }

        public string PartNumber { get; set; }

        public EkProductSourceEnum Source { get; set; }

        public string SourceId { get; set; }

        /// <summary>
        /// Used for Allegro-based product price calculations.
        /// </summary>
        public string CategoryId { get; set; }

        public MultiLanguageString Name { get; set; }

        public MultiLanguageString Description { get; set; }

        public string ThumbnailUrl { get; set; }

        public decimal BasePrice { get; set; }

        public string BasePriceCurrencyCode { get; set; }

        public decimal DeliveryPrice { get; set; }

        public decimal Price { get; set; }

        public string PriceCurrencyCode { get; set; }

        public string PriceCalculationInfo { get; set; }

        public EkProductStateEnum State { get; set; }

        public int Quantity { get; set; }

        public static EkTransactionProduct FromProduct(EkProduct product, MultiLanguageString description, int quantity)
        {
            Assure.ArgumentNotNull(product, nameof(product));

            return new EkTransactionProduct()
                {
                    Key = product.Key,
                    Source = product.Source,
                    SourceId = product.SourceId,
                    BrandName = product.BrandName,
                    PartNumber = product.PartNumber,
                    CategoryId = product.CategoryId,
                    Name = product.Name,
                    Description = description,
                    ThumbnailUrl = product.GetThumbnailUrl(),
                    BasePrice = product.BasePrice,
                    BasePriceCurrencyCode = product.BasePriceCurrencyCode,
                    DeliveryPrice = product.DeliveryPrice,
                    Price = product.Price,
                    PriceCurrencyCode = product.PriceCurrencyCode,
                    PriceCalculationInfo = product.PriceCalculationInfo,
                    State = product.State,
                    Quantity = quantity,
                };
        }
    }
}