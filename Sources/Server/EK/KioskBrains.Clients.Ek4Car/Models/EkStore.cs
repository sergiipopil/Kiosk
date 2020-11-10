using KioskBrains.Common.EK.Api;

namespace KioskBrains.Clients.Ek4Car.Models
{
    /// <summary>
    /// Not used at the moment but planned.
    /// </summary>
    public class EkStore
    {
        public string StoreId { get; set; }

        public Address Address { get; set; }

        public MultiLanguageString DisplayName { get; set; }

        public MultiLanguageString DescriptionAndSchedule { get; set; }
    }
}