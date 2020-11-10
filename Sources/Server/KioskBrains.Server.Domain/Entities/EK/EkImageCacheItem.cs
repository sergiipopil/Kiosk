using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities.EK
{
    public class EkImageCacheItem : EntityBase
    {
        [Required]
        [StringLength(500)]
        public string ImageKey { get; set; }

        public string ImageUrl { get; set; }
    }
}