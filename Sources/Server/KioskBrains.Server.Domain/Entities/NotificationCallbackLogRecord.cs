using System;
using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class NotificationCallbackLogRecord : EntityBase
    {
        public DateTime ReceivedOnUtc { get; set; }

        [Required]
        [StringLength(50)]
        public string Channel { get; set; }

        public string MessageJson { get; set; }
    }
}