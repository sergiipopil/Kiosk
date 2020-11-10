using KioskBrains.Common.EK.Api;

namespace KioskBrains.Clients.Ek4Car.Models
{
    /// <summary>
    /// Not used at the moment but planned.
    /// </summary>
    public class ServiceStation
    {
        public int Id { get; set; }

        public MultiLanguageString Name { get; set; }

        public Address Address { get; set; }

        public int? DiscountPercentage { get; set; }

        public decimal? DiscountAmount { get; set; }
    }
}