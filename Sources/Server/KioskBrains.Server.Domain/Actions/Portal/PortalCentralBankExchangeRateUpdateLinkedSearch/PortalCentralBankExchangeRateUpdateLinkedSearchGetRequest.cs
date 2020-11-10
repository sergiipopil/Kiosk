using KioskBrains.Server.Domain.Actions.Common.Models;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateUpdateLinkedSearch
{
    public class PortalCentralBankExchangeRateUpdateLinkedSearchGetRequest : BaseSearchRequest<EmptyForm>
    {
        public int CentralBankExchangeRateId { get; set; }
    }
}