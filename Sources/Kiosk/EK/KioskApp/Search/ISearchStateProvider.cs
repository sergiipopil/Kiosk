using System.ComponentModel;
using System.Windows.Input;

namespace KioskApp.Search
{
    public interface ISearchStateProvider : INotifyPropertyChanged
    {
        SearchStateEnum SearchState { get; set; }

        ICommand RetryOnErrorCommand { get; }
    }
}