using KioskBrains.Common.EK.Api.CarTree;

namespace TecDocModels
{
    public class ModelRecord
    {
        public EkCarTypeEnum CarType { get; set; }

        public TecDocTypeEnum TecDocType { get; set; }

        public int ModelId { get; set; }

        public string BrandName { get; set; }

        public string ModelName { get; set; }
    }
}