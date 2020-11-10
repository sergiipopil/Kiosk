using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Catalog.AutoParts
{
    public sealed partial class SearchByAnotherTypeMenu : UserControl
    {
        public SearchByAnotherTypeMenu()
        {
            InitializeComponent();
        }

        #region Items

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            nameof(Items), typeof(SearchByAnotherTypeMenuItem[]), typeof(SearchByAnotherTypeMenu), new PropertyMetadata(default(SearchByAnotherTypeMenuItem[])));

        public SearchByAnotherTypeMenuItem[] Items
        {
            get => (SearchByAnotherTypeMenuItem[])GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        #endregion

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var clickedItem = (SearchByAnotherTypeMenuItem)((FrameworkElement)sender).DataContext;
            OnSearchByAnotherTypeSelected(new SearchTypeSelectedEventArgs(clickedItem.SearchType));
        }

        public event EventHandler<SearchTypeSelectedEventArgs> SearchByAnotherTypeSelected;

        private void OnSearchByAnotherTypeSelected(SearchTypeSelectedEventArgs e)
        {
            SearchByAnotherTypeSelected?.Invoke(this, e);
        }
    }
}