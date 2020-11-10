namespace KioskBrains.Clients.TecDocWs.Models
{
    public class OeNumberInfo
    {
        public int BlockNumber { get; set; }

        public string BrandName { get; set; }

        public string OeNumber { get; set; }

        public int SortNumber { get; set; }

        public override string ToString()
        {
            return $"{BrandName} {OeNumber}";
        }
    }
}