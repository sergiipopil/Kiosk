namespace KioskBrains.Common.EK.Api
{
    public class EkKioskProductSearchByVinCodeGetResponse
    {
        /// <summary>
        /// 0..3
        /// </summary>
        public EkCarModelModification[] ModelModifications { get; set; }
    }
}