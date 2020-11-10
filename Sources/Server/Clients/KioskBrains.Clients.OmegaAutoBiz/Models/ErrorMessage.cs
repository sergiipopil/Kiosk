namespace KioskBrains.Clients.OmegaAutoBiz.Models
{
    public class ErrorMessage
    {
        public string Error { get; set; }

        public string Description { get; set; }

        public override string ToString()
        {
            return $"{Error} ({Description})";
        }
    }
}