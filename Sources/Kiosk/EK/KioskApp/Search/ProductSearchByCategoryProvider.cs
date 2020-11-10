using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Search
{
    public sealed class ProductSearchByCategoryProvider : SearchProviderBase, ISearchStateProvider
    {
        public ProductSearchByCategoryProvider(string modificationId, string categoryId)
        {
            Assure.ArgumentNotNull(modificationId, nameof(modificationId));
            Assure.ArgumentNotNull(categoryId, nameof(categoryId));

            if (!int.TryParse(modificationId, out int modificationIdInt))
            {
                throw new ArgumentException($"{nameof(modificationId)} '{modificationId}' is not integer.");
            }

            if (!int.TryParse(categoryId, out int categoryIdInt))
            {
                throw new ArgumentException($"{nameof(categoryId)} '{categoryId}' is not integer.");
            }

            _modificationId = modificationIdInt;
            _categoryId = categoryIdInt;
            RetryOnErrorCommand = new RelayCommand(
                nameof(RetryOnErrorCommand),
                parameter => UpdateSearchResults(_termUpdatedOn, _termValue));

            UpdateSearchResults(DateTime.Now, null);
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
                    UpdateProducts();
                    break;
            }
        }

        private readonly int _modificationId;

        private readonly int _categoryId;

        private readonly object _stateLocker = new object();

        private DateTime _termUpdatedOn;

        // term is not supported at the moment - kept for extensibility
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

                _currentApiRequest = new SearchProviderRequest(nameof(ProductsRequestAsync), ProductsRequestAsync);
                _currentApiRequest.RunRequest(_termUpdatedOn, _termValue);
            }
        }

        private void ResetState()
        {
            OriginalProducts = null;
            Sorting = EkProductSearchSortingEnum.Default;
        }

        private Product[] _originalProducts;

        private Product[] OriginalProducts
        {
            get => _originalProducts;
            set
            {
                _originalProducts = value;
                UpdateProducts();
            }
        }

        #region Products

        private Product[] _products;

        public Product[] Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        #endregion

        private void UpdateProducts()
        {
            lock (_stateLocker)
            {
                if (OriginalProducts == null)
                {
                    Products = null;
                    return;
                }

                switch (Sorting)
                {
                    case EkProductSearchSortingEnum.PriceAscending:
                        Products = OriginalProducts
                            .OrderBy(x => x.Price)
                            .ToArray();
                        break;
                    case EkProductSearchSortingEnum.PriceDescending:
                        Products = OriginalProducts
                            .OrderByDescending(x => x.Price)
                            .ToArray();
                        break;
                    default:
                    case EkProductSearchSortingEnum.Default:
                        Products = OriginalProducts;
                        break;
                }
            }
        }

        private async Task ProductsRequestAsync(DateTime termTime, string term, CancellationToken cancellationToken)
        {
            try
            {
                SearchState = SearchStateEnum.Searching;

                var response = await ServerApiHelper.ProductSearchByCategoryAsync(
                    new EkKioskProductSearchByCategoryGetRequest()
                        {
                            ModificationId = _modificationId,
                            CategoryId = _categoryId,
                        },
                    cancellationToken);
                var ekProducts = response.Products;
                if (ekProducts != null
                    && ekProducts.Length == 0)
                {
                    ekProducts = null;
                }

                lock (_stateLocker)
                {
                    if (termTime != _termUpdatedOn)
                    {
                        // request is expired
                        return;
                    }

                    OriginalProducts = ekProducts?
                        .Select(x => new Product(x))
                        .ToArray();

                    SearchState = OriginalProducts == null
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
    }
}