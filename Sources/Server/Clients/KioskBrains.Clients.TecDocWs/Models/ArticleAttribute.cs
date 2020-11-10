namespace KioskBrains.Clients.TecDocWs.Models
{
    public class ArticleAttribute
    {
        public int AttrBlockNo { get; set; }

        public long AttrId { get; set; }

        public bool AttrIsConditional { get; set; }

        public bool AttrIsInterval { get; set; }

        public bool AttrIsLinked { get; set; }

        public string AttrName { get; set; }

        public string AttrShortName { get; set; }

        public string AttrType { get; set; }

        public string AttrValue { get; set; }

        public long AttrValueId { get; set; }

        public override string ToString()
        {
            return $"{AttrName}: {AttrValue}";
        }
    }
}