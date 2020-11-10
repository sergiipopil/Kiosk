using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class KioskState : EntityBase
    {
        public DateTime TimeUtc { get; set; }

        public DateTime LocalTime { get; set; }

        [StringLength(50)]
        public string KioskVersion { get; set; }

        public List<KioskStateComponentInfo> ComponentsStatuses { get; set; }
    }
}