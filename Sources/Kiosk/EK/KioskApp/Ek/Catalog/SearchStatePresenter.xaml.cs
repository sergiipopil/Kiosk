using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Search;

namespace KioskApp.Ek.Catalog
{
    public sealed partial class SearchStatePresenter : UserControl
    {
        public SearchStatePresenter()
        {
            InitializeComponent();

            OnSearchStateChanged();
        }

        #region SearchState

        public static readonly DependencyProperty SearchStateProperty = DependencyProperty.Register(
            nameof(SearchState), typeof(SearchStateEnum), typeof(SearchStatePresenter), new PropertyMetadata(SearchStateEnum.TermIsRequired, SearchStatePropertyChangedCallback));

        private static void SearchStatePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SearchStatePresenter)d).OnSearchStateChanged();
        }

        public SearchStateEnum SearchState
        {
            get => (SearchStateEnum)GetValue(SearchStateProperty);
            set => SetValue(SearchStateProperty, value);
        }

        #endregion

        #region TermIsRequired

        public static readonly DependencyProperty TermIsRequiredProperty = DependencyProperty.Register(
            nameof(TermIsRequired), typeof(bool), typeof(SearchStatePresenter), new PropertyMetadata(default(bool)));

        public bool TermIsRequired
        {
            get => (bool)GetValue(TermIsRequiredProperty);
            set => SetValue(TermIsRequiredProperty, value);
        }

        #endregion

        #region TermName

        public static readonly DependencyProperty TermNameProperty = DependencyProperty.Register(
            nameof(TermName), typeof(string), typeof(SearchStatePresenter), new PropertyMetadata(default(string)));

        public string TermName
        {
            get => (string)GetValue(TermNameProperty);
            set => SetValue(TermNameProperty, value);
        }

        #endregion

        #region WithAnotherTermPhrase

        public static readonly DependencyProperty WithAnotherTermPhraseProperty = DependencyProperty.Register(
            nameof(WithAnotherTermPhrase), typeof(string), typeof(SearchStatePresenter), new PropertyMetadata(default(string)));

        public string WithAnotherTermPhrase
        {
            get => (string)GetValue(WithAnotherTermPhraseProperty);
            set => SetValue(WithAnotherTermPhraseProperty, value);
        }

        #endregion

        #region Searching

        public static readonly DependencyProperty SearchingProperty = DependencyProperty.Register(
            nameof(Searching), typeof(bool), typeof(SearchStatePresenter), new PropertyMetadata(default(bool)));

        public bool Searching
        {
            get => (bool)GetValue(SearchingProperty);
            set => SetValue(SearchingProperty, value);
        }

        #endregion

        #region NoResults

        public static readonly DependencyProperty NoResultsProperty = DependencyProperty.Register(
            nameof(NoResults), typeof(bool), typeof(SearchStatePresenter), new PropertyMetadata(default(bool)));

        public bool NoResults
        {
            get => (bool)GetValue(NoResultsProperty);
            set => SetValue(NoResultsProperty, value);
        }

        #endregion

        #region Error

        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register(
            nameof(Error), typeof(bool), typeof(SearchStatePresenter), new PropertyMetadata(default(bool)));

        public bool Error
        {
            get => (bool)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        #endregion

        #region RetryOnErrorCommand

        public static readonly DependencyProperty RetryOnErrorCommandProperty = DependencyProperty.Register(
            nameof(RetryOnErrorCommand), typeof(ICommand), typeof(SearchStatePresenter), new PropertyMetadata(default(ICommand)));

        public ICommand RetryOnErrorCommand
        {
            get => (ICommand)GetValue(RetryOnErrorCommandProperty);
            set => SetValue(RetryOnErrorCommandProperty, value);
        }

        #endregion

        #region IsManualSearchRun

        public static readonly DependencyProperty IsManualSearchRunProperty = DependencyProperty.Register(
            nameof(IsManualSearchRun), typeof(bool), typeof(SearchStatePresenter), new PropertyMetadata(true));

        public bool IsManualSearchRun
        {
            get => (bool)GetValue(IsManualSearchRunProperty);
            set => SetValue(IsManualSearchRunProperty, value);
        }

        #endregion

        #region ShowAnotherSections

        public static readonly DependencyProperty ShowAnotherSectionsProperty = DependencyProperty.Register(
            nameof(ShowAnotherSections), typeof(bool), typeof(SearchStatePresenter), new PropertyMetadata(true));

        public bool ShowAnotherSections
        {
            get => (bool)GetValue(ShowAnotherSectionsProperty);
            set => SetValue(ShowAnotherSectionsProperty, value);
        }

        #endregion

        private void OnSearchStateChanged()
        {
            var searchState = SearchState;
            TermIsRequired = searchState == SearchStateEnum.TermIsRequired;
            Searching = searchState == SearchStateEnum.Searching;
            NoResults = searchState == SearchStateEnum.NoResults;
            Error = searchState == SearchStateEnum.Error;
            Visibility = searchState == SearchStateEnum.Results
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}