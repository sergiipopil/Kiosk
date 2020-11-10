using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KioskBrains.Waf.Helpers.Contracts;

namespace KioskBrains.Server.Domain.Entities.Common
{
    public class EntityBase : IEntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }
    }
}