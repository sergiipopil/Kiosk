using System;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class CentralBankExchangeRateUpdate : EntityBase
    {
        public int CentralBankExchangeRateId { get; set; }

        public CentralBankExchangeRate CentralBankExchangeRate { get; set; }

        public DateTime StartTime { get; set; }

        public decimal Rate { get; set; }
    }
}