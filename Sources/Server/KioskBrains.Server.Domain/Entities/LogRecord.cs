using System;
using System.ComponentModel.DataAnnotations;
using KioskBrains.Common.Logging;
using KioskBrains.Server.Domain.Entities.Common;
using ApiLogRecord = KioskBrains.Common.Logging.LogRecord;

namespace KioskBrains.Server.Domain.Entities
{
    public class LogRecord : EntityBase
    {
        public int KioskId { get; set; }

        public Kiosk Kiosk { get; set; }

        [Required]
        [StringLength(255)]
        public string UniqueId { get; set; }

        [StringLength(50)]
        public string KioskVersion { get; set; }

        public DateTime LocalTime { get; set; }

        public LogTypeEnum Type { get; set; }

        public LogContextEnum Context { get; set; }

        public string Message { get; set; }

        public string AdditionalDataJson { get; set; }

        public static LogRecord FromApiModel(int kioskId, ApiLogRecord apiModel)
        {
            return new LogRecord()
                {
                    KioskId = kioskId,
                    UniqueId = apiModel.UniqueId,
                    KioskVersion = apiModel.KioskVersion,
                    LocalTime = apiModel.LocalTime,
                    Type = apiModel.Type,
                    Context = apiModel.Context,
                    Message = apiModel.Message,
                    AdditionalDataJson = apiModel.AdditionalDataJson,
                };
        }
    }
}