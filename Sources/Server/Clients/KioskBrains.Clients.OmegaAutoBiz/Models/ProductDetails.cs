namespace KioskBrains.Clients.OmegaAutoBiz.Models
{
    public class ProductDetails
    {
        public long ProductId { get; set; }

        public OeCodeInfo[] OECodeList { get; set; }

        public ProductResource[] ResourceList { get; set; }

        public KeyValue<string>[] SpecificationList { get; set; }
    }
}