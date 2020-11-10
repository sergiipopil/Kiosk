using KioskBrains.Common.EK.Api;
using KioskBrains.Common.EK.Api.CarTree;

namespace KioskBrains.Common.EK.KioskConfiguration
{
    public class EkSettings
    {
        public EkProductCategory[] EuropeCategories { get; set; }

        public EkCarGroup[] CarModelTree { get; set; }
    }
}