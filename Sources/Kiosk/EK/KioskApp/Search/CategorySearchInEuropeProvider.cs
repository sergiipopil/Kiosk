using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KioskApp.CoreExtension.Application;
using KioskApp.Ek.Catalog.Categories;
using KioskBrains.Common.EK.Api;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Search
{
    /// <summary>
    /// Should be used from UI thread.
    /// </summary>
    public class CategorySearchInEuropeProvider : SearchProviderBase, ISearchStateProvider, ICategorySearchProvider
    {
        public CategorySearchInEuropeProvider(
            string initialCategoryId,
            Action onBackToRoot)
        {
            _onBackToRoot = onBackToRoot;
            SearchTitle = "Выберите группу";

            RetryOnErrorCommand = new RelayCommand(
                nameof(RetryOnErrorCommand),
                parameter =>
                    {
                        // not supported
                    });

            InitCategories(initialCategoryId);
        }

        private async void InitCategories(string initialCategoryId)
        {
            // free UI thread
            await ThreadHelper.RunInBackgroundThreadAsync(() =>
                {
                    // build categories
                    var rootCategories = EkSettingsHelper.GetEuropeCategories();
                    if (rootCategories == null
                        || rootCategories.Length == 0)
                    {
                        return Task.CompletedTask;
                    }

                    BuildCategories(null, rootCategories);

                    Category selectedCategory = null;
                    if (initialCategoryId != null)
                    {
                        selectedCategory = _allCategories.GetValueOrDefault(initialCategoryId);

                        // if initial category is leaf then change category
                        if (selectedCategory != null
                            && !selectedCategory.IsGroup)
                        {
                            ChangeCategory(selectedCategory);
                            return Task.CompletedTask;
                        }
                    }

                    if (selectedCategory == null)
                    {
                        selectedCategory = _allCategories.GetValueOrDefault(rootCategories[0].CategoryId);
                    }

                    SelectCategory(selectedCategory);

                    return Task.CompletedTask;
                });
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

        private readonly object _stateLocker = new object();

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

        private readonly Dictionary<string, Category> _allCategories = new Dictionary<string, Category>();

        private void BuildCategories(string parentCategoryId, EkProductCategory[] productCategories)
        {
            if (productCategories == null)
            {
                return;
            }

            foreach (var productCategory in productCategories)
            {
                var category = new Category(productCategory)
                    {
                        ParentCategoryId = parentCategoryId,
                    };

                if (category.IsGroup)
                {
                    // group is not selectable for search - add special leaf category with group name and id
                    var specialCategory = new Category(
                        new EkProductCategory()
                            {
                                CategoryId = productCategory.CategoryId,
                                Name = productCategory.Name,
                                Children = null,
                            },
                        CategorySpecialTypeEnum.ProductCategoryGroupSelector);

                    // modify group id to avoid conflict with special category
                    category.Id = "GROUP_" + category.Id;
                    specialCategory.ParentCategoryId = category.Id;

                    // register group and special category
                    _allCategories[category.Id] = category;
                    _allCategories[specialCategory.Id] = specialCategory;

                    // build children
                    BuildCategories(category.Id, productCategory.Children);
                }
                else
                {
                    // just register leaf
                    _allCategories[category.Id] = category;
                }
            }
        }

        public void ChangeCategory(Category category)
        {
            lock (_stateLocker)
            {
                if (category.ParentCategoryId == null)
                {
                    OnBackToRoot();
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
                if (category == null)
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
                    parentCategoryId = parentCategory?.ParentCategoryId;
                }

                var orderedBreadcrumbs = breadcrumbs
                    .Where(x => x != null)
                    .Reverse()
                    .ToList();

                // check if leaf
                if (!category.IsGroup)
                {
                    // leaf
                    OnCategorySelected(new SelectedCategoryValue()
                        {
                            Name1 = string.Join(" - ", orderedBreadcrumbs.Select(x => x.Name)),
                            Name2 = category.Name,
                            Id = category.Id,
                        });
                    return;
                }

                // add selected category to breadcrumbs
                orderedBreadcrumbs.Add(category);
                Breadcrumbs = orderedBreadcrumbs.ToArray();
                // show children (calculate children by registered categories, ProductCategory.Children can't be used since tree is modified by special categories)
                Categories = _allCategories.Values
                    .Where(x => x.ParentCategoryId == category.Id)
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