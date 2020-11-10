using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KioskApp.Ek.Catalog.Categories;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Search
{
    public class CategorySearchProvider : SearchProviderBase, ISearchStateProvider, ICategorySearchProvider
    {
        public CategorySearchProvider(
            string initialCategoryId,
            string modificationId,
            Action onBackToRoot)
        {
            Assure.ArgumentNotNull(modificationId, nameof(modificationId));
            if (!int.TryParse(modificationId, out int modificationIdInt))
            {
                throw new ArgumentException($"{nameof(modificationId)} '{modificationId}' is not integer.");
            }

            _initialCategoryId = initialCategoryId;
            _modificationId = modificationIdInt;
            _onBackToRoot = onBackToRoot;

            SearchTitle = "Выберите категорию";

            RetryOnErrorCommand = new RelayCommand(
                nameof(RetryOnErrorCommand),
                parameter =>
                    {
                        UpdateCategories();
                    });

            UpdateCategories();
        }

        public override bool IsAutocompleteSupported => false;

        public override bool IsSearchManualRun => false;

        #region SearchState

        private SearchStateEnum _SearchState = SearchStateEnum.Results;

        public SearchStateEnum SearchState
        {
            get => _SearchState;
            set => SetProperty(ref _SearchState, value);
        }

        #endregion

        public ICommand RetryOnErrorCommand { get; }

        protected override void UpdateSearchResults(DateTime termTime, string term)
        {
            // not supported
        }

        protected override Task<string[]> GetAutocompleteOptionsAsync(string term, CancellationToken cancellationToken)
        {
            return Task.FromResult((string[])null);
        }

        private readonly string _initialCategoryId;

        private readonly int _modificationId;

        private readonly object _stateLocker = new object();

        private SearchProviderRequest _currentApiRequest;

        private DateTime _categoriesRequestedOn;

        private void UpdateCategories()
        {
            lock (_stateLocker)
            {
                Breadcrumbs = null;
                Categories = null;

                _currentApiRequest?.Cancel();

                _categoriesRequestedOn = DateTime.Now;
                _currentApiRequest = new SearchProviderRequest(nameof(CategoriesRequestAsync), CategoriesRequestAsync);
                _currentApiRequest.RunRequest(_categoriesRequestedOn, null);
            }
        }

        private async Task CategoriesRequestAsync(DateTime termTime, string term, CancellationToken cancellationToken)
        {
            try
            {
                SearchState = SearchStateEnum.Searching;

                var response = await ServerApiHelper.ProductCategoriesByCarModelModificationAsync(
                    new EkKioskProductCategoriesByCarModelModificationGetRequest()
                        {
                            ModificationId = _modificationId,
                        },
                    cancellationToken);
                var productCategories = response.Categories;
                if (productCategories != null
                    && productCategories.Length == 0)
                {
                    productCategories = null;
                }

                lock (_stateLocker)
                {
                    if (termTime != _categoriesRequestedOn)
                    {
                        // request is expired
                        return;
                    }

                    InitCategories(productCategories);
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

        private void InitCategories(EkProductCategory[] productCategories)
        {
            if (productCategories == null
                || productCategories.Length == 0)
            {
                SearchState = SearchStateEnum.NoResults;
                return;
            }

            SearchState = SearchStateEnum.Results;

            // build categories
            var rootCategories = productCategories;

            BuildCategories(null, rootCategories);

            if (_initialCategoryId != null)
            {
                var initialCategory = _allCategories.GetValueOrDefault(_initialCategoryId);
                // if initial category is set, change it
                if (initialCategory != null)
                {
                    ChangeCategory(initialCategory);
                    return;
                }
            }

            Breadcrumbs = null;
            Categories = _rootCategories;
        }

        #region SearchTitle

        private string _SearchTitle;

        public string SearchTitle
        {
            get => _SearchTitle;
            set => SetProperty(ref _SearchTitle, value);
        }

        #endregion

        #region Breadcrumbs

        private Category[] _Breadcrumbs;

        public Category[] Breadcrumbs
        {
            get => _Breadcrumbs;
            set => SetProperty(ref _Breadcrumbs, value);
        }

        #endregion

        #region Categories

        private Category[] _Categories;

        public Category[] Categories
        {
            get => _Categories;
            set => SetProperty(ref _Categories, value);
        }

        #endregion

        private Category[] _rootCategories;

        private readonly Dictionary<string, Category> _allCategories = new Dictionary<string, Category>();

        private void BuildCategories(string parentCategoryId, EkProductCategory[] productCategories)
        {
            if (productCategories == null)
            {
                return;
            }

            var categories = productCategories
                .Select(productCategory => new Category(productCategory)
                    {
                        ParentCategoryId = parentCategoryId,
                    })
                .ToArray();
            if (parentCategoryId == null)
            {
                _rootCategories = categories;
            }

            foreach (var category in categories)
            {
                _allCategories[category.Id] = category;
                if (category.IsGroup)
                {
                    BuildCategories(category.Id, category.ProductCategory.Children);
                }
            }
        }

        public void ChangeCategory(Category category)
        {
            lock (_stateLocker)
            {
                if (category.ParentCategoryId == null)
                {
                    Breadcrumbs = null;
                    Categories = _rootCategories;
                    return;
                }

                var parentCategory = _allCategories.GetValueOrDefault(category.ParentCategoryId);
                SelectCategory(parentCategory);
            }
        }

        public void SelectCategory(Category category)
        {
            lock (_stateLocker)
            {
                if (category?.ProductCategory == null)
                {
                    return;
                }

                // calculate parent nodes
                var breadcrumbs = new List<Category>();
                var parentCategoryId = category.ParentCategoryId;
                while (parentCategoryId != null)
                {
                    var parentCategory = _allCategories.GetValueOrDefault(parentCategoryId);
                    breadcrumbs.Add(parentCategory);
                    parentCategoryId = parentCategory.ParentCategoryId;
                }

                var orderedBreadcrumbs = breadcrumbs
                    .Where(x => x != null)
                    .Reverse()
                    .ToList();

                // check if leaf
                var productCategory = category.ProductCategory;
                if (productCategory.Children == null
                    || productCategory.Children.Length == 0)
                {
                    // leaf
                    OnCategorySelected(new SelectedCategoryValue()
                        {
                            Name1 = string.Join(" - ", orderedBreadcrumbs.Select(x => x.Name)),
                            Name2 = category.Name,
                            Id = category.Id,
                            ContextCarModelModificationId = _modificationId.ToString(),
                        });
                    return;
                }

                // add selected category to breadcrumbs
                orderedBreadcrumbs.Add(category);
                Breadcrumbs = orderedBreadcrumbs.ToArray();
                // show children
                Categories = productCategory.Children
                    .Select(x => _allCategories.GetValueOrDefault(x.CategoryId))
                    .Where(x => x != null)
                    .ToArray();
            }
        }

        private readonly Action _onBackToRoot;

        private void OnBackToRoot()
        {
            ThreadHelper.RunInUiThreadAsync(() =>
                {
                    _onBackToRoot?.Invoke();
                });
        }

        public void GoBack()
        {
            lock (_stateLocker)
            {
                var selectedCategory = Breadcrumbs?.LastOrDefault();
                if (selectedCategory == null)
                {
                    OnBackToRoot();
                    return;
                }

                ChangeCategory(selectedCategory);
            }
        }

        public event EventHandler<SelectedCategoryValue> CategorySelected;

        protected virtual void OnCategorySelected(SelectedCategoryValue eventArgs)
        {
            ThreadHelper.RunInUiThreadAsync(() =>
                {
                    CategorySelected?.Invoke(this, eventArgs);
                });
        }
    }
}