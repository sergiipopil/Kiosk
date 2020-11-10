using System.Linq;
using Newtonsoft.Json;

namespace KioskBrains.Clients.TecDocWs.Models
{
    public class ArticleExtendedInfo
    {
        /// <summary>
        /// All attributes.
        /// </summary>
        public ArticleAttribute[] ArticleAttributes { get; set; }

        public ArticleDocument[] ArticleDocuments { get; set; }

        /// <summary>
        /// All info.
        /// </summary>
        public ArticleInfoRecord[] ArticleInfo { get; set; }

        public ArticleThumbnail[] ArticleThumbnails { get; set; }

        public DirectArticleInfo DirectArticle { get; set; }

        public EanNumberInfo[] EanNumber { get; set; }

        public string[] GetEanNumbers()
        {
            return EanNumber?
                       .Select(x => x.EanNumber)
                       .ToArray()
                   ?? new string[0];
        }

        /// <summary>
        /// Main attributes.
        /// </summary>
        [JsonProperty("immediateAttributs")]
        public ArticleAttribute[] ImmediateAttributes { get; set; }

        /// <summary>
        /// Main info.
        /// </summary>
        public ArticleInfoRecord[] ImmediateInfo { get; set; }

        /// <summary>
        /// Link to parent article (for which current article is a part).
        /// </summary>
        public RelatedArticleInfo[] MainArticle { get; set; }

        public OeNumberInfo[] OenNumbers { get; set; }

        public override string ToString()
        {
            return $"{DirectArticle} (OEM: {string.Join(", ", OenNumbers?.Select(x => x.ToString()) ?? new string[0])}) {string.Join("; ", ArticleAttributes.Select(x => x.ToString()))}";
        }
    }
}