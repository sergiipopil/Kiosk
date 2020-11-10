namespace KioskBrains.Server.Domain.Actions.Common.Models
{
    public class BaseSearchResponse<TRecord>
    {
        public int Total { get; set; }

        public TRecord[] Records { get; set; }
    }
}
