namespace KioskBrains.Clients.OmegaAutoBiz.Models
{
    public class ProductSearchRecord
    {
        public long ProductId { get; set; }

        public string Card { get; set; }

        public string Number { get; set; }

        public string BrandDescription { get; set; }

        public string Description { get; set; }

        public string DescriptionUkr { get; set; }

        public string Info { get; set; }

        public decimal Weight { get; set; }

        public decimal Price { get; set; }

        public decimal CustomerPrice { get; set; }

        public KeyValue<string>[] Rests { get; set; }

        public override string ToString()
        {
            return $"{BrandDescription} {Description?.Trim()} {(CustomerPrice > 0 ? CustomerPrice : Price)} грн. ({Number.Trim()})";
        }
    }
}