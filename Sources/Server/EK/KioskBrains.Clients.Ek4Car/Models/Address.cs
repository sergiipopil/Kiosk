namespace KioskBrains.Clients.Ek4Car.Models
{
    public class Address
    {
        /// <summary>
        /// 3-symbol alpha code.
        /// </summary>
        public string CountryCode { get; set; }

        public string RegionCode { get; set; }

        public string City { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }
    }
}