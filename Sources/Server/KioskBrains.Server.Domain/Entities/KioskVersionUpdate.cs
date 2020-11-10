using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class KioskVersionUpdate : EntityBase
    {
        public KioskApplicationTypeEnum ApplicationType { get; set; }

        [Required]
        [StringLength(255)]
        public string VersionName { get; set; }

        [Required]
        [StringLength(1000)]
        public string UpdateUrl { get; set; }

        public string ReleaseNotes { get; set; }
    }
}