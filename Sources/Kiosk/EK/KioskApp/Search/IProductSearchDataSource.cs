using System.Collections.Generic;
using System.ComponentModel;

namespace KioskApp.Search
{
    public interface IProductSearchDataSource : IEnumerable<Product>, INotifyPropertyChanged
    {
        bool IsNextPageLoading { get; set; }

        long? TotalCount { get; set; }
    }
}