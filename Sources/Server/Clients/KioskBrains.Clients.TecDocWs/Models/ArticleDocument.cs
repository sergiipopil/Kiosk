namespace KioskBrains.Clients.TecDocWs.Models
{
    public class ArticleDocument
    {
        public string DocFileName { get; set; }

        public string DocFileTypeName { get; set; }

        public string DocId { get; set; }

        public int DocTypeId { get; set; }

        public string DocTypeName { get; set; }

        public override string ToString()
        {
            return $"{DocFileName} ({DocId}) {DocTypeName} ({DocTypeId})";
        }
    }
}