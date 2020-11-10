namespace KioskBrains.Server.Domain.Actions.Common.Models
{
    public class SearchMetadata
    {
        public int Start { get; set; }

        public int PageSize { get; set; }

        public string OrderBy { get; set; }

        public OrderDirectionEnum OrderDirection { get; set; }
    }
}