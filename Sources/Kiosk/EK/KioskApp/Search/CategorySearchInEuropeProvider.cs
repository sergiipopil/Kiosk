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
        int _modelId;
        string _rootCategorieName = "";
        string _rootCategorieID = "";
        public CategorySearchInEuropeProvider(
            string initialCategoryId, int modelId,
            Action onBackToRoot)
        {
            _onBackToRoot = onBackToRoot;
            SearchTitle = "ВИБЕРІТЬ ГРУПУ";



            _modelId = modelId;

            /*if (_modelId!=0)
            {
                SearchTitle += EkSettingsHelper.GetModelFullNameByModelId(_modelId.ToString());
            }*/

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
            List<string> availableCategories = null;
            /*if (_modelId != 0)
            {
                try
                {
                    var response = await ServerApiHelper.ProductCategoriesByCarModelModificationAsync(
                       new EkKioskProductCategoriesByCarModelModificationGetRequest()
                       {
                           FullModelName = EkSettingsHelper.GetModelFullNameByModelId(_modelId.ToString()),
                       },
                       CancellationToken.None);
                    availableCategories = response.CategoriesIds.Select(x => x.ToString()).ToList();
                }
                catch(Exception)
                {
                }
            }*/

            // free UI thread
            await ThreadHelper.RunInBackgroundThreadAsync(() =>
                {

                    // build categories

                    var rootCategories = EkSettingsHelper.GetEuropeCategories().Where(x => availableCategories == null || availableCategories.Contains(x.CategoryId)).ToArray();
                    if (rootCategories == null
                        || rootCategories.Length == 0)
                    {
                        return Task.CompletedTask;
                    }

                    BuildCategories(null, null, rootCategories, availableCategories); ;

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
        public class BreadcrumbsTree : IBreadcrumbsTree
        {
            public string RootLevel { get; set; }
            public string SecondLevel { get; set; }
            public string ThirdLevel { get; set; }
        }
        public interface IBreadcrumbsTree
        {
            string RootLevel { get; set; }
            string SecondLevel { get; set; }
            string ThirdLevel { get; set; }
        }
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

        private void BuildCategories(string parentCategoryName, string parentCategoryId, EkProductCategory[] productCategories, IList<string> availableCategories)
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
                    ParentCategoryName = parentCategoryName
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
                    BuildCategories(category.Name, category.Id, productCategory.Children.Where(x => availableCategories == null || availableCategories.Contains(x.CategoryId)).ToArray(), availableCategories);
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
        public event EventHandler<SelectedCategoryValue> CategorySelected;
        private bool SecondLevelCategory { get; set; }
        private bool ThirdLevelCategory { get; set; }
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
                var breadcrumbsTree = new BreadcrumbsTree();
                var parentCategoryId = category.ParentCategoryId;
                var parentCategoryName = category.ParentCategoryName;
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

                int index = orderedBreadcrumbs.FindIndex(x => x.Name.Contains("Легковые и микроавтобусы"));
                if (index != -1)
                    orderedBreadcrumbs.Remove(orderedBreadcrumbs[index]);
                // check if leaf
                //EkSettingsHelper.GetModelAndNameByModelId(_modelId.ToString()).Name integra
                //EkSettingsHelper.GetModelManufacturerNameByModelId(_modelId.ToString()) acura
                SelectedCategoryValue tempCategory = new SelectedCategoryValue();
                if (_modelId != 0)
                {
                    string manufactureName = EkSettingsHelper.GetModelManufacturerNameByModelId(_modelId.ToString());
                    string manufactureURL = $"/Themes/Assets/Images/Catalog/Model_Logo/{manufactureName}.png";
                    string carModelName = EkSettingsHelper.GetModelAndNameByModelId(_modelId.ToString()).Name;
                    tempCategory = new SelectedCategoryValue()
                    {
                        Name1 = EkSettingsHelper.GetModelFullNameByModelId(_modelId.ToString()) + " - " + string.Join(" - ", orderedBreadcrumbs.Select(x => x.Name)),
                        Name2 = category.Name,
                        SelectedManufactureURL = manufactureURL,
                        SelectedCarModelText = carModelName,
                        SelectedCarModelURL = $"/Themes/Assets/Images/Catalog/CarModel/{manufactureName}/{carModelName}.png",
                        Id = category.Id
                    };
                    
                    if (category.Id != "GROUP_620")
                    {
                        if (category.IsGroup)
                        {
                            if (category.ParentCategoryId == "GROUP_620")
                            {
                                tempCategory.SelectedGroupURL = $"/Themes/Assets/Images/Catalog/AutoParts/" + category.Id + ".png";
                                tempCategory.SelectedGroupText = category.Name == "Легковые и микроавтобусы" ? "" : category.Name;
                                breadcrumbsTree.RootLevel = category.Name;
                                _rootCategorieName = tempCategory.SelectedGroupText;
                                _rootCategorieID = category.Id;
                            }
                            else {
                                tempCategory.SelectedGroupURL = $"/Themes/Assets/Images/Catalog/AutoParts/" + category.ParentCategoryId + ".png";
                                tempCategory.SelectedGroupText = category.Name == "Легковые и микроавтобусы" ? "" : category.ParentCategoryName;
                                breadcrumbsTree.RootLevel = category.ParentCategoryName;
                                tempCategory.SelectedSubGroupURL = $"/Themes/Assets/Images/Catalog/AutoParts/" + category.Id + ".png";
                                tempCategory.SelectedSubGroupText = category.Name == "Легковые и микроавтобусы" ? "" : category.Name;
                                breadcrumbsTree.SecondLevel = category.Name;
                            }
                        }
                        else
                        {
                            if (breadcrumbs.Count() > 1)
                            {
                                if (breadcrumbs[1].Name != "Легковые и микроавтобусы")
                                {
                                    tempCategory.SelectedGroupURL = $"/Themes/Assets/Images/Catalog/AutoParts/" + breadcrumbs[1].Id + ".png";
                                    tempCategory.SelectedGroupText = category.Name == "Легковые и микроавтобусы" ? "" : breadcrumbs[1].Name;
                                }
                                                            tempCategory.SelectedSubGroupURL = $"/Themes/Assets/Images/Catalog/AutoParts/" + breadcrumbs[0].Id + ".png";
                            tempCategory.SelectedSubGroupText = category.Name == "Легковые и микроавтобусы" ? "" : breadcrumbs[0].Name;
                            tempCategory.SelectedSecondSubGroupURL = $"/Themes/Assets/Images/Catalog/AutoParts/" + category.Id + ".png";
                            tempCategory.SelectedSecondSubGroupText = category.Name == "Легковые и микроавтобусы" ? "" : category.Name;
                            }
                            else
                            {
                                tempCategory.SelectedGroupURL = $"/Themes/Assets/Images/Catalog/AutoParts/" + category.ParentCategoryId + ".png";
                                tempCategory.SelectedGroupText = category.Name == "Легковые и микроавтобусы" ? "" : category.ParentCategoryName;
                                tempCategory.SelectedSubGroupURL = $"/Themes/Assets/Images/Catalog/AutoParts/" + category.Id + ".png";
                                tempCategory.SelectedSubGroupText = category.Name == "Легковые и микроавтобусы" ? "" : category.Name;
                            }
                        }
                    }
                }
                else
                {
                    tempCategory = new SelectedCategoryValue()
                    {
                        Name1 = string.Join(" - ", orderedBreadcrumbs.Select(x => x.Name)),
                        Name2 = category.Name,
                        Id = category.Id
                    };
                }
                OnCategorySelected(tempCategory);
                //SelectedModelURL = $"/Themes/Assets/Images/Catalog/CarModel/{EkSettingsHelper.GetModelManufacturerNameByModelId(_selectedModelId.ToString())}/{category.Name}.png"
                if (!category.IsGroup)
                {
                    // leaf

                    return;
                }

                // add selected category to breadcrumbs
                orderedBreadcrumbs.Add(category);
                Breadcrumbs = orderedBreadcrumbs.ToArray();
                // show children (calculate children by registered categories, ProductCategory.Children can't be used since tree is modified by special categories)
                Categories = _allCategories.Values
                    .Where(x => x.ParentCategoryId == category.Id && x.Id != category.Id.Replace("GROUP_", ""))
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



        protected virtual void OnCategorySelected(SelectedCategoryValue eventArgs)
        {
            ThreadHelper.RunInUiThreadAsync(() =>
                {
                    CategorySelected?.Invoke(this, eventArgs);
                });
        }
    }
}