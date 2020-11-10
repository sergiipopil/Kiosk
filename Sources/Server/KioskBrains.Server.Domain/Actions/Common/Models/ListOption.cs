namespace KioskBrains.Server.Domain.Actions.Common.Models
{
    public class ListOption<TValue>
    {
        public TValue Value { get; set; }

        public string DisplayName { get; set; }
    }
}