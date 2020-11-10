using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class CentralBankExchangeRate : EntityBase
    {
        [Required]
        [StringLength(3)]
        public string LocalCurrencyCode { get; set; }

        [Required]
        [StringLength(3)]
        public string ForeignCurrencyCode { get; set; }

        public int DefaultOrder { get; set; }

        public List<CentralBankExchangeRateUpdate> CentralBankExchangeRateUpdates { get; set; }
    }
}