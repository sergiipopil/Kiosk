using System;

namespace KioskBrains.Clients.Ek4Car.Models
{
    public class Update
    {
        public DateTime CreatedOnUtc { get; set; }

        public UpdateTypeEnum Type { get; set; }

        public int EntityId { get; set; }

        public UpdateChangeEnum Change { get; set; }

        public Product Product { get; set; }

        public Category Category { get; set; }

        public EkStore EkStore { get; set; }

        public ServiceStation ServiceStation { get; set; }
    }
}