namespace KioskBrains.Server.Domain.Actions.Common.Models
{
    public class BaseSearchRequest<TForm>
    {
        public string SearchTerm { get; set; }

        public TForm SearchStruct { get; set; }

        public bool IsAdvancedSearch { get; set; }

        public SearchMetadata Metadata { get; set; }
    }
}