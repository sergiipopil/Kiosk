using System;
using System.Collections.Generic;
using KioskBrains.Common.KioskState;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalKioskStateSearch
{
    public class PortalKioskStateDetailsForm
    {
        public int Id { get; set; }

        public int? KioskStateId { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string AssignedKioskVersion { get; set; }

        public string KioskStateVersion { get; set; }

        public DateTime? LastPingedOnUtc { get; set; }

        public decimal? LastPingedMinutesAgo => LastPingedOnUtc != null
            ? Math.Round((decimal)(DateTime.UtcNow - LastPingedOnUtc.Value).TotalMinutes, 1)
            : (decimal?)null;

        public List<ComponentMonitorableState> ComponentsStatuses { get; set; }
    }
}