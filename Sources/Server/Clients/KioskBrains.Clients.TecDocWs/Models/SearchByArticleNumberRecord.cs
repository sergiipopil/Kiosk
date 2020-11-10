namespace KioskBrains.Clients.TecDocWs.Models
{
    public class SearchByArticleNumberRecord
    {
        public long ArticleId { get; set; }

        public string ArticleName { get; set; }

        /// <summary>
        /// Part Number.
        /// </summary>
        public string ArticleNo { get; set; }

        public string ArticleSearchNo { get; set; }

        public int ArticleStateId { get; set; }

        public string BrandName { get; set; }

        public int BrandNo { get; set; }

        public int GenericArticleId { get; set; }

        public ArticleNumberTypeEnum NumberType { get; set; }

        public override string ToString()
        {
            return $"{NumberType} {BrandName} {ArticleNo} {ArticleName} ({ArticleId})";
        }
    }
}