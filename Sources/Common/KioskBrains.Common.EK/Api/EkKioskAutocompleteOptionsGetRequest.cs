using KioskBrains.Common.Logging;

namespace KioskBrains.Common.EK.Api
{
    public class EkKioskAutocompleteOptionsGetRequest : ILoggableObject
    {
        public string Term { get; set; }

        public string LanguageCode { get; set; }

        public EkSearchTypeEnum SearchType { get; set; }

        public object GetLogObject()
        {
            return new
                {
                    Term,
                    LanguageCode,
                    SearchType = SearchType.ToString(),
                };
        }
    }
}