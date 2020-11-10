using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class Country : EntityBase
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(2)]
        public string Alpha2 { get; set; }

        [Required]
        [StringLength(3)]
        public string Alpha3 { get; set; }

        [Required]
        [StringLength(3)]
        public string Code { get; set; }
    }
}