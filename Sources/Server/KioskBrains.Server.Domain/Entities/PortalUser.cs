using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class PortalUser : EntityBase
    {
        public int CustomerId { get; set; }

        public Customer Customer { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        public UserRoleEnum Role { get; set; }

        [Required]
        [StringLength(255)]
        public string FullName { get; set; }
    }
}