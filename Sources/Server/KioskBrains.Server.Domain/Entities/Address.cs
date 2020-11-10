using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class Address : EntityBase
    {
        [StringLength(255)]
        public string AddressLine1 { get; set; }

        [StringLength(255)]
        public string AddressLine2 { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(10)]
        public string ZipCode { get; set; }

        public int? StateId { get; set; }

        public State State { get; set; }

        public int? CountryId { get; set; }

        public Country Country { get; set; }
    }
}