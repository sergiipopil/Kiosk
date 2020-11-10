namespace KioskBrains.Clients.TecDocWs.Models
{
    public class ArticleInfoRecord
    {
        public long InfoId { get; set; }

        public string InfoText { get; set; }

        public int InfoType { get; set; }

        public string InfoTypeName { get; set; }

        public override string ToString()
        {
            return $"{InfoText} ({InfoTypeName})";
        }
    }
}