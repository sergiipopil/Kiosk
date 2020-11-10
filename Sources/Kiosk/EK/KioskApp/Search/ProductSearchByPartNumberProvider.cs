using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KioskBrains.Common.EK.Api;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Search
{
    public class ProductSearchByPartNumberProvider : SearchProviderBase, ISearchStateProvider
    {
        public ProductSearchByPartNumberProvider()
        {
            BrandSelectedCommand = new RelayCommand(
                nameof(BrandSelectedCommand),
                parameter => OnBrandSelectedCommand(parameter as PartNumberBrand));

            RetryOnErrorCommand = new RelayCommand(
                nameof(RetryOnErrorCommand),
                parameter => UpdateSearchResults(_termUpdatedOn, _termValue));

            ResetState();
        }

        public override bool IsAutocompleteSupported => false;

        protected override Task<string[]> GetAutocompleteOptionsAsync(string term, CancellationToken cancellationToken)
        {
            return null;
        }

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

        #region Sorting

        private EkProductSearchSortingEnum _Sorting = EkProductSearchSortingEnum.Default;

        public EkProductSearchSortingEnum Sorting
        {
            get => _Sorting;
            set => SetProperty(ref _Sorting, value);
        }

        #endregion

        protected override void OnOwnPropertyChanged(string propertyName)
        {
            base.OnOwnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Sorting):
                    UpdateReplacements();
                    break;
            }
        }

        private readonly object _stateLocker = new object();

        private DateTime _termUpdatedOn;

        private string _termValue;

        private SearchProviderRequest _currentApiRequest;

        protected override void UpdateSearchResults(DateTime termTime, string term)
        {
            lock (_stateLocker)
            {
                _termUpdatedOn = termTime;
                _termValue = term;
                ResetState();
                _currentApiRequest?.Cancel();

                if (string.IsNullOrEmpty(term))
                {
                    SearchState = SearchStateEnum.TermIsRequired;
                }
                else
                {
                    _currentApiRequest = new SearchProviderRequest(nameof(BrandsToSelectRequestAsync), BrandsToSelectRequestAsync);
                    _currentApiRequest.RunRequest(_termUpdatedOn, _termValue);
                }
            }
        }

        private void ResetState()
        {
            _originalBrandsToSelect = null;
            BrandsToSelect = null;
            Product = null;
            OriginalReplacements = null;
            Sorting = EkProductSearchSortingEnum.Default;
        }

        private PartNumberBrand[] _originalBrandsToSelect;

        #region BrandsToSelect

        private PartNumberBrand[] _BrandsToSelect;

        public PartNumberBrand[] BrandsToSelect
        {
            get => _BrandsToSelect;
            set => SetProperty(ref _BrandsToSelect, value);
        }

        #endregion

        #region Product

        private Product _Product;

        public Product Product
        {
            get => _Product;
            set => SetProperty(ref _Product, value);
        }

        #endregion

        private Product[] _originalReplacements;

        private Product[] OriginalReplacements
        {
            get => _originalReplacements;
            set
            {
                _originalReplacements = value;
                UpdateReplacements();
            }
        }

        #region Replacements

        private Product[] _Replacements;

        public Product[] Replacements
        {
            get => _Replacements;
            set => SetProperty(ref _Replacements, value);
        }

        #endregion

        private void UpdateReplacements()
        {
            lock (_stateLocker)
            {
                if (OriginalReplacements == null)
                {
                    Replacements = null;
                    return;
                }

                switch (Sorting)
                {
                    case EkProductSearchSortingEnum.PriceAscending:
                        Replacements = OriginalReplacements
                            .OrderBy(x => x.Price)
                            .ToArray();
                        break;
                    case EkProductSearchSortingEnum.PriceDescending:
                        Replacements = OriginalReplacements
                            .OrderByDescending(x => x.Price)
                            .ToArray();
                        break;
                    default:
                    case EkProductSearchSortingEnum.Default:
                        Replacements = OriginalReplacements;
                        break;
                }
            }
        }

        private async Task BrandsToSelectRequestAsync(DateTime termTime, string term, CancellationToken cancellationToken)
        {
            try
            {
                SearchState = SearchStateEnum.Searching;

                var response = await ServerApiHelper.ProductSearchByPartNumberAsync(
                    new EkKioskProductSearchByPartNumberGetRequest()
                        {
                            PartNumber = term,
                        },
                    cancellationToken);
                var brandsToSelect = response.Brands;
                if (brandsToSelect != null
                    && brandsToSelect.Length == 0)
                {
                    brandsToSelect = null;
                }

                lock (_stateLocker)
                {
                    if (termTime != _termUpdatedOn)
                    {
                        // request is expired
                        return;
                    }

                    BrandsToSelect = brandsToSelect?
                        .Select(x => new PartNumberBrand(x))
                        .ToArray();
                    _originalBrandsToSelect = BrandsToSelect;

                    SearchState = BrandsToSelect == null
                        ? SearchStateEnum.NoResults
                        : SearchStateEnum.Results;

                    if (BrandsToSelect?.Length == 1)
                    {
                        OnBrandSelectedCommand(BrandsToSelect[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    SearchState = SearchStateEnum.Error;
                }

                throw;
            }
        }

        public ICommand BrandSelectedCommand { get; }

        private EkPartNumberBrand _selectedPartNumberBrand;

        private DateTime _partNumberBrandSelectedOn;

        private void OnBrandSelectedCommand(PartNumberBrand partNumberBrand)
        {
            if (partNumberBrand == null)
            {
                return;
            }

            lock (_stateLocker)
            {
                BrandsToSelect = null;
                _selectedPartNumberBrand = partNumberBrand.EkPartNumberBrand;
                _partNumberBrandSelectedOn = DateTime.Now;
                _currentApiRequest?.Cancel();
                _currentApiRequest = new SearchProviderRequest(nameof(ProductReplacementsRequestAsync), ProductReplacementsRequestAsync);
                _currentApiRequest.RunRequest(_partNumberBrandSelectedOn, _termValue);
            }
        }

        private async Task ProductReplacementsRequestAsync(DateTime termTime, string term, CancellationToken cancellationToken)
        {
            try
            {
                SearchState = SearchStateEnum.Searching;

                var response = await ServerApiHelper.ProductAndReplacementsByPartNumberAsync(
                    new EkKioskProductAndReplacementsByPartNumberGetRequest()
                        {
                            PartNumberBrand = _selectedPartNumberBrand,
                        },
                    cancellationToken);
                var replacements = response.Replacements;
                if (replacements != null
                    && replacements.Length == 0)
                {
                    replacements = null;
                }

                lock (_stateLocker)
                {
                    if (termTime != _partNumberBrandSelectedOn)
                    {
                        // request is expired
                        return;
                    }

                    Product = response.Product != null
                        ? new Product(response.Product)
                        : null;
                    OriginalReplacements = replacements?
                        .Select(x => new Product(x))
                        .ToArray();

                    SearchState = Product == null
                        ? SearchStateEnum.NoResults
                        : SearchStateEnum.Results;
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                {
                    SearchState = SearchStateEnum.Error;
                }

                throw;
            }
        }

        public void BackToBrandSelection(ICommand mainBackCommand)
        {
            lock (_stateLocker)
            {
                _currentApiRequest?.Cancel();

                if (_originalBrandsToSelect?.Length > 1)
                {
                    Product = null;
                    BrandsToSelect = _originalBrandsToSelect;
                    return;
                }
            }

            mainBackCommand?.Execute(null);
        }
    }
}