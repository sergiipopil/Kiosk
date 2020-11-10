namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateSearch
{
    public class PortalCentralBankExchangeRateSearchRecord
    {
        public int Id { get; set; }

        public string LocalCurrencyCode { get; set; }

        public string ForeignCurrencyCode { get; set; }

        public decimal? Rate { get; set; }

        public int? DefaultOrder { get; set; }
    }
}