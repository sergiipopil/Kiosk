using KioskBrains.Common.EK.Api;

namespace KioskBrains.Clients.Ek4Car.Models
{
    public class Product
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public MultiLanguageString Name { get; set; }

        public decimal Price { get; set; }

        /// <summary>
        /// First photo is considered as main. [0..*]
        /// </summary>
        public Photo[] Photos { get; set; }

        public string PartNumber { get; set; }

        public ProductStateEnum State { get; set; }

        public MultiLanguageString Description { get; set; }

        public int? ProductionYear { get; set; }
    }
}