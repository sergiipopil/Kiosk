using KioskBrains.Common.Logging;

namespace KioskBrains.Common.EK.Api
{
    public class EkKioskProductSearchByNameGetRequest : ILoggableObject
    {
        public string Term { get; set; }

        public string LanguageCode { get; set; }

        public EkProductStateEnum? State { get; set; }

        public int From { get; set; }

        public int Count { get; set; }

        public bool IncludeTotal { get; set; }

        public EkProductSearchSortingEnum Sorting { get; set; }

        public object GetLogObject()
        {
            return new
                {
                    Term,
                    LanguageCode,
                    State,
                    From,
                    Count,
                    Sorting = Sorting.ToString(),
                };
        }
    }
}