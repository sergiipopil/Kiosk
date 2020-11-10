using System;
using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Common.Services;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class IntegrationLogRecord : EntityBase
    {
        public IntegrationRequestDirectionEnum Direction { get; set; }

        [StringLength(255)]
        [Required]
        public string ExternalSystem { get; set; }

        public DateTime RequestedOnUtc { get; set; }

        public TimeSpan ProcessingTime { get; set; }

        public string Request { get; set; }

        public string Response { get; set; }
    }
}