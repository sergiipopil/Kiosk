namespace KioskBrains.Clients.OmegaAutoBiz.Models
{
    public class ProductResource
    {
        public long ProductId { get; set; }

        public string DocType { get; set; }

        public string Description { get; set; }

        public int Number { get; set; }

        public override string ToString()
        {
            return $"{Number} {DocType} {Description}";
        }
    }
}