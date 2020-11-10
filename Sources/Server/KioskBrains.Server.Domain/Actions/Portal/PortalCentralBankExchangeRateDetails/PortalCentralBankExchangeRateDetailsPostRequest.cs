using System.ComponentModel.DataAnnotations;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateDetails
{
    public class PortalCentralBankExchangeRateDetailsPostRequest
    {
        [Required]
        public PortalCentralBankExchangeRateDetailsForm Form { get; set; }
    }
}