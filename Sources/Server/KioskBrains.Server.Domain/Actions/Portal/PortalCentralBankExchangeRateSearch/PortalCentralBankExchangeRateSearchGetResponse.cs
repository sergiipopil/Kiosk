using KioskBrains.Server.Domain.Actions.Common.Models;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateSearch
{
    public class PortalCentralBankExchangeRateSearchGetResponse : BaseSearchResponse<PortalCentralBankExchangeRateSearchRecord>
    {
        public ListOptionString[] Currencies { get; set; }

        // todo: remove after new authorization model
        public bool IsNewAllowed { get; set; }
    }
}