namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateDetails
{
    public class PortalCentralBankExchangeRateDetailsForm
    {
        public int? Id { get; set; }

        public string LocalCurrencyCode { get; set; }

        public string ForeignCurrencyCode { get; set; }

        public int? DefaultOrder { get; set; }
    }
}