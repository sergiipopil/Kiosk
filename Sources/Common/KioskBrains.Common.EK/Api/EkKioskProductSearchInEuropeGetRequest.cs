using KioskBrains.Common.Logging;

namespace KioskBrains.Common.EK.Api
{
    public class EkKioskProductSearchInEuropeGetRequest : ILoggableObject
    {
        public string Term { get; set; }

        public string TranslatedTerm { get; set; }

        public string CategoryId { get; set; }

        public string LanguageCode { get; set; }

        public EkProductStateEnum? State { get; set; }

        public int From { get; set; }

        public int Count { get; set; }

        public EkProductSearchSortingEnum Sorting { get; set; }

        public object GetLogObject()
        {
            return new
                {
                    Term,
                    TranslatedTerm,
                    LanguageCode,
                    State,
                    From,
                    Count,
                    Sorting = Sorting.ToString(),
                };
        }
    }
}