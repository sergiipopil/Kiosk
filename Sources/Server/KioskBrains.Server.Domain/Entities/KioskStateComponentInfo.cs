using System.ComponentModel.DataAnnotations;
using KioskBrains.Common.KioskState;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class KioskStateComponentInfo : EntityBase
    {
        public int KioskStateId { get; set; }

        public KioskState KioskState { get; set; }

        [Required]
        [StringLength(100)]
        public string ComponentName { get; set; }

        public ComponentStatusCodeEnum StatusCode { get; set; }

        public string StatusMessage { get; set; }

        public string SpecificMonitorableStateJson { get; set; }

        public static KioskStateComponentInfo FromComponentMonitorableState(ComponentMonitorableState componentState)
        {
            return new KioskStateComponentInfo
            {
                ComponentName = componentState.ComponentName,
                StatusCode = componentState.Status?.Code ?? ComponentStatusCodeEnum.Undefined,
                StatusMessage = componentState.Status?.Message,
                SpecificMonitorableStateJson = componentState.SpecificMonitorableStateJson,
            };
        }
    }
}