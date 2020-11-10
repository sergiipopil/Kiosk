namespace KioskBrains.Clients.OmegaAutoBiz.Models
{
    public class KeyValue<TValue>
    {
        public string Key { get; set; }

        public TValue Value { get; set; }

        public override string ToString()
        {
            return $"{Key}: {Value}";
        }
    }
}