using KioskBrains.Server.Domain.Actions.Common.Models;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateDetails
{
    public class PortalCentralBankExchangeRateDetailsGetResponse
    {
        public PortalCentralBankExchangeRateDetailsForm Form { get; set; }

        public ListOptionString[] Currencies { get; set; }
    }
}