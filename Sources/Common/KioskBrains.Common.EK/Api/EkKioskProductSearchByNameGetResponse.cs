namespace KioskBrains.Common.EK.Api
{
    public class EkKioskProductSearchByNameGetResponse
    {
        public EkProduct[] Products { get; set; }

        public long Total { get; set; }
    }
}