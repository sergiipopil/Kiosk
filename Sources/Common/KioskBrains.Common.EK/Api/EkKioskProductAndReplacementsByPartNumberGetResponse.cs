namespace KioskBrains.Common.EK.Api
{
    public class EkKioskProductAndReplacementsByPartNumberGetResponse
    {
        public EkProduct Product { get; set; }

        public EkProduct[] Replacements { get; set; }
    }
}