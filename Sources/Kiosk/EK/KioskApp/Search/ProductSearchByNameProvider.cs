using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KioskBrains.Common.EK.Api;
using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Search
{
    public class ProductSearchByNameProvider : SearchProviderBase, ISearchStateProvider
    {
        public ProductSearchByNameProvider()
        {
            RetryOnErrorCommand = new RelayCommand(
                nameof(RetryOnErrorCommand),
                parameter => UpdateProductSearchDataSource());
        }

        public override bool IsAutocompleteSupported => true;

        public override bool IsSearchManualRun => true;

        #region SearchState

        private SearchStateEnum _SearchState = SearchStateEnum.TermIsRequired;

        public SearchStateEnum SearchState
        {
            get => _SearchState;
            set => SetProperty(ref _SearchState, value);
        }

        #endregion

        public ICommand RetryOnErrorCommand { get; }

        #region TermFilter

        private string _TermFilter;

        public string TermFilter
        {
            get => _TermFilter;
            set => SetProperty(ref _TermFilter, value);
        }

        #endregion

        #region Sorting

        private EkProductSearchSortingEnum _Sorting = EkProductSearchSortingEnum.Default;

        public EkProductSearchSortingEnum Sorting
        {
            get => _Sorting;
            set => SetProperty(ref _Sorting, value);
        }

        #endregion

        public const int MinTermLength = 3;

        public EkProductStateEnum State { get; set; }

        protected override void OnOwnPropertyChanged(string propertyName)
        {
            base.OnOwnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(TermFilter):
                case nameof(Sorting):
                    UpdateProductSearchDataSource();
                    break;
            }
        }

        protected override void UpdateSearchResults(DateTime termTime, string term)
        {
            // just update term filter
            TermFilter = term;
        }

        #region ProductSearchDataSource

        private ProductSearchByNameDataSource _productSearchDataSource;

        public ProductSearchByNameDataSource ProductSearchDataSource
        {
            get => _productSearchDataSource;
            private set => SetProperty(ref _productSearchDataSource, value);
        }

        #endregion

        private readonly object _productSearchDataSourceLocker = new object();

        private void UpdateProductSearchDataSource()
        {
            lock (_productSearchDataSourceLocker)
            {
                ProductSearchDataSource?.CancelCurrentRequest();

                var term = TermFilter?.Trim();
                if (term == null
                    || term.Length < MinTermLength)
                {
                    ProductSearchDataSource = null;
                    SearchState = SearchStateEnum.TermIsRequired;
                }
                else
                {
                    var searchParameters = new ProductSearchByNameParameters()
                        {
                            Term = term,
                            State = State,
                            Sorting = Sorting,
                            PageNumber = 0,
                            PageSize = 20,
                        };
                    ProductSearchDataSource = new ProductSearchByNameDataSource(searchParameters, this);
                }
            }
        }

        protected override async Task<string[]> GetAutocompleteOptionsAsync(string term, CancellationToken cancellationToken)
        {
            var lastWord = GetLastWord(term);
            if (lastWord == null
                || lastWord.Length < 2)
            {
                return new string[0];
            }

            // todo: don't send new requests if previous last word (substring) returned no results

            return await ServerApiHelper.GetAutocompleteOptionsAsync(
                new EkKioskAutocompleteOptionsGetRequest()
                    {
                        Term = term,
                        LanguageCode = LanguageManager.Current.CurrentAppLanguageCode,
                        SearchType = EkSearchTypeEnum.Name,
                    },
                cancellationToken);
        }
    }
}