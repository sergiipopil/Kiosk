using System;

namespace KioskBrains.Server.Domain.Managers.Integration.Rates
{
    public class CurrencyPairDateRate
    {
        public string LocalCurrencyCode { get; set; }

        public string ForeignCurrencyCode { get; set; }

        public DateTime Date { get; set; }

        public decimal Rate { get; set; }
    }
}