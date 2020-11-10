namespace KioskBrains.Common.KioskState
{
    public class ComponentMonitorableState
    {
        public int Id { get; set; }

        public string ComponentName { get; set; }

        public ComponentStatus Status { get; set; }

        public string SpecificMonitorableStateJson { get; set; }
    }
}