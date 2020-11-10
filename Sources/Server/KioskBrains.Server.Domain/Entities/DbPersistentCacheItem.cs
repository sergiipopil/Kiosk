using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class DbPersistentCacheItem : EntityBase
    {
        [Required]
        [StringLength(255)]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}