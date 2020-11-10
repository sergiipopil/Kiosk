using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Search
{
    public class ProductSearchByNameDataSource : UiBindableObjectCollection<Product>, ISupportIncrementalLoading, IProductSearchDataSource
    {
        public ProductSearchByNameDataSource(ProductSearchByNameParameters searchParameters, ISearchStateProvider searchStateProvider)
        {
            Assure.ArgumentNotNull(searchParameters, nameof(searchParameters));
            Assure.ArgumentNotNull(searchStateProvider, nameof(searchStateProvider));

            _searchParameters = searchParameters;
            _searchStateProvider = searchStateProvider;

            // in order to trigger initial request
            HasMoreItems = true;
        }

        private readonly ProductSearchByNameParameters _searchParameters;

        private readonly ISearchStateProvider _searchStateProvider;

        #region IsNextPageLoading

        private bool _IsNextPageLoading;

        public bool IsNextPageLoading
        {
            get => _IsNextPageLoading;
            set => SetProperty(ref _IsNextPageLoading, value);
        }

        #endregion

        #region TotalCount

        private long? _TotalCount;

        public long? TotalCount
        {
            get => _TotalCount;
            set => SetProperty(ref _TotalCount, value);
        }

        #endregion

        public const int MaxRecordsLimit = 200;

        private CancellationTokenSource _currentRequestCancellationTokenSource;

        private readonly object _currentRequestCancellationTokenSourceLocker = new object();

        public void CancelCurrentRequest()
        {
            lock (_currentRequestCancellationTokenSourceLocker)
            {
                _currentRequestCancellationTokenSource?.Cancel();
            }
        }

        private Task<LoadMoreItemsResult> LoadNextPageAsync()
        {
            return ThreadHelper.RunInNewThreadAsync(async () =>
                {
                    var isFirstPage = _searchParameters.PageNumber == 0;

                    try
                    {
                        CancellationToken cancellationToken;
                        lock (_currentRequestCancellationTokenSourceLocker)
                        {
                            if (_currentRequestCancellationTokenSource != null)
                            {
                                Log.Error(LogContextEnum.Ui, $"Duplicate invocation of '{nameof(ProductSearchByNameDataSource)}.{nameof(LoadNextPageAsync)}'.");
                            }

                            _currentRequestCancellationTokenSource = new CancellationTokenSource();
                            cancellationToken = _currentRequestCancellationTokenSource.Token;
                        }

                        if (isFirstPage)
                        {
                            _searchStateProvider.SearchState = SearchStateEnum.Searching;
                        }
                        else
                        {
                            IsNextPageLoading = true;
                        }

                        // 2 tries
                        for (var i = 0; i < 2; i++)
                        {
                            try
                            {
                                var includeTotal = isFirstPage;

                                var response = await ServerApiHelper.ProductSearchByNameAsync(
                                    new EkKioskProductSearchByNameGetRequest()
                                        {
                                            Term = _searchParameters.Term,
                                            LanguageCode = LanguageManager.Current.CurrentAppLanguageCode,
                                            State = _searchParameters.State,
                                            From = _searchParameters.PageNumber*_searchParameters.PageSize,
                                            Count = _searchParameters.PageSize,
                                            IncludeTotal = includeTotal,
                                            Sorting = _searchParameters.Sorting,
                                        },
                                    cancellationToken);

                                if (includeTotal)
                                {
                                    TotalCount = response.Total;
                                }

                                if (response.Products == null
                                    || response.Products.Length == 0)
                                {
                                    if (isFirstPage)
                                    {
                                        _searchStateProvider.SearchState = SearchStateEnum.NoResults;
                                    }

                                    HasMoreItems = false;
                                    return new LoadMoreItemsResult();
                                }

                                if (isFirstPage)
                                {
                                    _searchStateProvider.SearchState = SearchStateEnum.Results;
                                }

                                await AddRangeAsync(response.Products
                                    .Select(x => new Product(x)));

                                // check if final page
                                var isFinalPage = response.Products.Length < _searchParameters.PageSize // returned less than page
                                                  || Count >= MaxRecordsLimit // limit exceeded
                                                  || (TotalCount != null && Count >= TotalCount.Value); // count exceeded total count

                                // prepare next page request
                                if (isFinalPage)
                                {
                                    HasMoreItems = false;
                                }
                                else
                                {
                                    _searchParameters.PageNumber++;
                                }

                                return new LoadMoreItemsResult()
                                    {
                                        Count = (uint)response.Products.Length
                                    };
                            }
                            catch (OperationCanceledException)
                            {
                                // rethrow cancellations
                                throw;
                            }
                            catch (Exception ex)
                            {
                                Log.Error(LogContextEnum.Communication, $"'{nameof(ProductSearchByNameDataSource)}.{nameof(LoadNextPageAsync)}' failed (try {i + 1}).",
                                    new
                                        {
                                            Parameters = _searchParameters.GetLogObject(),
                                            Exception = ex.ToString(),
                                        });
                            }
                        }

                        if (isFirstPage)
                        {
                            _searchStateProvider.SearchState = SearchStateEnum.Error;
                        }
                        else
                        {
                            // todo: show the error and retry button
                        }

                        HasMoreItems = false;
                        return new LoadMoreItemsResult();
                    }
                    catch (OperationCanceledException)
                    {
                        // cancelled
                        HasMoreItems = false;
                        return new LoadMoreItemsResult();
                    }
                    finally
                    {
                        if (!isFirstPage)
                        {
                            IsNextPageLoading = false;
                        }

                        lock (_currentRequestCancellationTokenSourceLocker)
                        {
                            _currentRequestCancellationTokenSource.DisposeSafe();
                            _currentRequestCancellationTokenSource = null;
                        }
                    }
                });
        }

        #region ISupportIncrementalLoading Implementation

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return LoadNextPageAsync().AsAsyncOperation();
        }

        public bool HasMoreItems { get; private set; }

        #endregion
    }
}