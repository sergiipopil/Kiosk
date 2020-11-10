using System;
using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Domain.Entities.Common;
using KioskBrains.Server.Domain.Security;

namespace KioskBrains.Server.Domain.Entities
{
    public class Kiosk : EntityBase
    {
        public int CustomerId { get; set; }

        public Customer Customer { get; set; }

        public KioskStatusEnum Status { get; set; }

        public KioskApplicationTypeEnum ApplicationType { get; set; }

        [Required]
        [StringLength(KioskSecretKeyGenerator.KioskSerialKeyLength)]
        public string SerialKey { get; set; }

        [Required]
        public string WorkflowComponentConfigurationsJson { get; set; }

        public DateTime? LastPingedOnUtc { get; set; }

        public int? CurrentStateId { get; set; }

        public KioskState CurrentState { get; set; }

        [StringLength(50)]
        public string AssignedKioskVersion { get; set; }

        public int AddressId { get; set; }

        public Address Address { get; set; }

        [StringLength(255)]
        public string CommaSeparatedLanguageCodes { get; set; }

        [StringLength(1000)]
        public string CommunicationComments { get; set; }

        [StringLength(8)]
        public string AdminModePassword { get; set; }
    }
}