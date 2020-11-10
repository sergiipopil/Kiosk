namespace KioskBrains.Clients.TecDocWs.Models
{
    public class DirectArticleInfo
    {
        /// <summary>
        /// ???
        /// </summary>
        public string ArticleAddName { get; set; }

        public long ArticleId { get; set; }

        /// <summary>
        /// Product type name.
        /// </summary>
        public string ArticleName { get; set; }

        /// <summary>
        /// Part Number.
        /// </summary>
        public string ArticleNo { get; set; }

        /// <summary>
        /// Normal (1), Out of production, etc.
        /// </summary>
        public int ArticleState { get; set; }

        public string ArticleStateName { get; set; }

        public string BrandName { get; set; }

        public int BrandNo { get; set; }

        public bool FlagAccessories { get; set; }

        public bool FlagCertificationCompulsory { get; set; }

        public bool FlagRemanufacturedPart { get; set; }

        public bool FlagSuitedforSelfService { get; set; }

        public int GenericArticleId { get; set; }

        public bool HasAppendage { get; set; }

        public bool HasAxleLink { get; set; }

        public bool HasCsGraphics { get; set; }

        public bool HasDocuments { get; set; }

        public bool HasLessDiscount { get; set; }

        public bool HasMarkLink { get; set; }

        public bool HasMotorLink { get; set; }

        public bool HasOEN { get; set; }

        public bool HasPartList { get; set; }

        public bool HasPrices { get; set; }

        public bool HasSecurityInfo { get; set; }

        public bool HasUsage { get; set; }

        public bool HasVehicleLink { get; set; }

        public int? PackingUnit { get; set; }

        public int? QuantityPerPackingUnit { get; set; }

        public override string ToString()
        {
            return $"{BrandName} {ArticleNo} {ArticleName} ({ArticleId})";
        }
    }
}