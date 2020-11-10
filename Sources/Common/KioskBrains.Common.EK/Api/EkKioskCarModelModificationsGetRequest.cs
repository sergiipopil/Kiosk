using KioskBrains.Common.EK.Api.CarTree;

namespace KioskBrains.Common.EK.Api
{
    public class EkKioskCarModelModificationsGetRequest
    {
        public TecDocTypeEnum TecDocType { get; set; }

        public int ManufacturerId { get; set; }

        public int ModelId { get; set; }
    }
}