using System;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateUpdateLinkedSearch
{
    public class PortalCentralBankExchangeRateUpdateLinkedSearchRecord
    {
        public int Id { get; set; }

        public DateTime StartTime { get; set; }

        public decimal? Rate { get; set; }
    }
}