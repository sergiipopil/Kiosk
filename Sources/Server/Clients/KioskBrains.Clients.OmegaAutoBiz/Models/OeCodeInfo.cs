namespace KioskBrains.Clients.OmegaAutoBiz.Models
{
    public class OeCodeInfo
    {
        public string Code { get; set; }

        public string CarModel { get; set; }

        public override string ToString()
        {
            return $"{CarModel} {Code}";
        }
    }
}