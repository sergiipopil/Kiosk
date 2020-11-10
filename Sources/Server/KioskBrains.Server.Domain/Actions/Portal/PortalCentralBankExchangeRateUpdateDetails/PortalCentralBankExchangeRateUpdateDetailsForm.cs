using System;
using Newtonsoft.Json;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateUpdateDetails
{
    public class PortalCentralBankExchangeRateUpdateDetailsForm
    {
        public int? Id { get; set; }

        public int CentralBankExchangeRateId { get; set; }

        public DateTime StartDate { get; set; }

        public string StartTime { get; set; }

        [JsonIgnore]
        public DateTime StartDateTime => StartDate + TimeSpan.Parse(StartTime);

        public decimal? Rate { get; set; }
    }
}