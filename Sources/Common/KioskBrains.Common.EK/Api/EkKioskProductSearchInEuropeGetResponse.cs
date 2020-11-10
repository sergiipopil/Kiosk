namespace KioskBrains.Common.EK.Api
{
    public class EkKioskProductSearchInEuropeGetResponse
    {
        public EkProduct[] Products { get; set; }

        public string TranslatedTerm { get; set; }

        public long Total { get; set; }
    }
}