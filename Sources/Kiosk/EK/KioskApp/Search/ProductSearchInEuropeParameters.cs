using KioskBrains.Common.EK.Api;
using KioskBrains.Common.Logging;

namespace KioskApp.Search
{
    public class ProductSearchInEuropeParameters : ILoggableObject
    {
        public string Term { get; set; }

        public string CategoryId { get; set; }

        public EkProductStateEnum? State { get; set; }

        public EkProductSearchSortingEnum Sorting { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public object GetLogObject()
        {
            return new
                {
                    Term,
                    CategoryId,
                    State,
                    Sorting = Sorting.ToString(),
                    PageNumber,
                    PageSize,
                };
        }
    }
}