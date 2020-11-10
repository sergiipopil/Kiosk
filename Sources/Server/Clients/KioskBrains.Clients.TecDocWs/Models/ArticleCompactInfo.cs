namespace KioskBrains.Clients.TecDocWs.Models
{
    /// <summary>
    /// Specific product (auto part, etc.).
    /// </summary>
    public class ArticleCompactInfo
    {
        public long ArticleId { get; set; }

        /// <summary>
        /// Id of link between <see cref="ModelType"/> and <see cref="ArticleCompactInfo"/>.
        /// </summary>
        public long ArticleLinkId { get; set; }

        /// <summary>
        /// Part Number.
        /// </summary>
        public string ArticleNo { get; set; }

        public string BrandName { get; set; }

        public int BrandNo { get; set; }

        /// <summary>
        /// Product type Id.
        /// </summary>
        public int GenericArticleId { get; set; }

        /// <summary>
        /// Product type name.
        /// </summary>
        public string GenericArticleName { get; set; }

        public long? SortNo { get; set; }

        public override string ToString()
        {
            return $"{BrandName} {ArticleNo} {GenericArticleName} ({ArticleId})";
        }
    }
}