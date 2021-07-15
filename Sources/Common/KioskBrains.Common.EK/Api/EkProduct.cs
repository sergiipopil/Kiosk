using System.Linq;

namespace KioskBrains.Common.EK.Api
{
    public class EkProduct
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

        public MultiLanguageString SpecificationsJson { get; set; }

        public EkProductPhoto[] Photos { get; set; }

        public string GetThumbnailUrl()
        {
            var firstPhoto = Photos?.FirstOrDefault();
            if (firstPhoto == null)
            {
                return null;
            }

            return firstPhoto.ThumbnailUrl ?? firstPhoto.Url;
        }

        public decimal BasePrice { get; set; }

        public string BasePriceCurrencyCode { get; set; }

        public decimal DeliveryPrice { get; set; }

        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }

        public string PriceCurrencyCode { get; set; }

        public string PriceCalculationInfo { get; set; }

        public EkProductStateEnum State { get; set; }

        public string ProductionYear { get; set; }
    }
}