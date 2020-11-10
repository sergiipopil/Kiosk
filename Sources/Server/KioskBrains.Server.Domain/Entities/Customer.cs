using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class Customer : EntityBase
    {
        public bool IsSystem { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(50)]
        public string SupportPhone { get; set; }

        [StringLength(50)]
        public string TimeZoneName { get; set; }
    }
}