namespace KioskBrains.Clients.TecDocWs.Models
{
    public class ArticleThumbnail
    {
        public string ThumbDocId { get; set; }

        public string ThumbFileName { get; set; }

        public int ThumbTypeId { get; set; }

        public override string ToString()
        {
            return $"{ThumbFileName} ({ThumbDocId}/{ThumbTypeId})";
        }
    }
}