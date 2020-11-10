using System.ComponentModel.DataAnnotations;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateUpdateDetails
{
    public class PortalCentralBankExchangeRateUpdateDetailsPostRequest
    {
        [Required]
        public PortalCentralBankExchangeRateUpdateDetailsForm Form { get; set; }
    }
}