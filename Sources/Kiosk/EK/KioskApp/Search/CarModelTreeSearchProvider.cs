using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KioskApp.CoreExtension.Application;
using KioskApp.Ek.Catalog.Categories;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Common.EK.Api.CarTree;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Search
{
    public class CarModelTreeSearchProvider : SearchProviderBase, ISearchStateProvider, ICategorySearchProvider
    {
        public CarModelTreeSearchProvider(
            EkCarTypeEnum? carType,
            string modelId,
            Action onBackToRoot)
        {
            _onBackToRoot = onBackToRoot;

            RetryOnErrorCommand = new RelayCommand(
                nameof(RetryOnErrorCommand),
                parameter =>
                    {
                        // only modifications request fail is possible
                        UpdateModifications();
                    });

            InitModelTree(carType, modelId);
        }
        public EventHandler<EkCarModel> OnModelSelected { get; set; }

        

        public async void InitModelTree(EkCarTypeEnum? carType, string modelId)
        {
            Assure.CheckFlowState(carType != null || modelId != null, $"Either {nameof(carType)} or {nameof(modelId)} should not be null.");

            // free UI thread
            await ThreadHelper.RunInBackgroundThreadAsync(() =>
                {
                    // build model tree
                    var carModelTree = EkSettingsHelper.GetCarModelTree();
                    if (carModelTree == null
                        || carModelTree.Length == 0)
                    {
                        return Task.CompletedTask;
                    }

                    foreach (var ekCarGroup in carModelTree)
                    {
                        var groupCategory = new Category(ekCarGroup.CarType);
                        _allGroups[groupCategory.Id] = groupCategory;

                        foreach (var manufacturer in ekCarGroup.Manufacturers)
                        {
                            var manufacturerCategory = new Category(ekCarGroup.CarType, manufacturer)
                                {
                                    ParentCategoryId = groupCategory.Id,
                                };
                            _allManufacturers[manufacturerCategory.Id] = manufacturerCategory;

                            foreach (var carModel in manufacturer.CarModels)
                            {
                                var modelCategory = new Category(carModel)
                                    {
                                        ParentCategoryId = manufacturerCategory.Id,
                                    };
                                _allModels[modelCategory.Id] = modelCategory;
                            }
                        }
                    }

                    Category selectedCategory = null;

                    if (modelId != null)
                    {
                        selectedCategory = _allModels.GetValueOrDefault(modelId);
                    }
                    else if (carType != null)
                    {
                        selectedCategory = _allGroups.GetValueOrDefault(carType.ToString());
                    }

                    if (selectedCategory == null)
                    {
                        selectedCategory = _allGroups.Values.First();
                    }

                    SelectCategory(selectedCategory);

                    return Task.CompletedTask;
                });
        }

        private readonly Dictionary<string, Category> _allGroups = new Dictionary<string, Category>();

        private readonly Dictionary<string, Category> _allManufacturers = new Dictionary<string, Category>();

        private readonly Dictionary<string, Category> _allModels = new Dictionary<string, Category>();

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

        public void ChangeCategory(Category category)
        {
            if (category == null)
            {
                return;
            }

            switch (category.Type)
            {
                case CategoryTypeEnum.CarGroup:
                    OnBackToRoot();
                    return;
                case CategoryTypeEnum.CarManufacturer:
                    SelectCategory(_allGroups.GetValueOrDefault(category.ParentCategoryId));
                    return;
                case CategoryTypeEnum.CarModel:
                    SelectCategory(_allManufacturers.GetValueOrDefault(category.ParentCategoryId));
                    return;
            }
        }

        public void SelectCategory(Category category)
        {
            if (category == null)
            {
                return;
            }

            lock (_stateLocker)
            {
                _currentApiRequest?.Cancel();

                var orderedBreadcrumbs = ProcessSelectionTree(category);

                switch (category.Type)
                {
                    case CategoryTypeEnum.CarGroup:
                        SearchTitle = "ВЫБЕРИТЕ МАРКУ АВТОМОБИЛЯ";
                        // it's more efficient to run through all values
                        SearchState = SearchStateEnum.Results;
                        Categories = _allManufacturers.Values
                            .Where(x => x.ParentCategoryId == category.Id)
                            .ToArray();
                        break;
                    case CategoryTypeEnum.CarManufacturer:
                        SearchTitle = "ВЫБЕРИТЕ МОДЕЛЬ";
                        //add categoryName for model pics
                        SearchState = SearchStateEnum.Results;
                         Categories = category.CarManufacturer?.CarModels
                            ?.Select(x => _allModels.GetValueOrDefault(x.Id.ToString()))
                            .Where(x => x != null)
                            .ToArray();
                        break;
                    case CategoryTypeEnum.CarModel:
                        SearchTitle = "ВЫБЕРИТЕ МОДИФИКАЦИЮ";
                        //OnTopCategorySelected("628");                        
                        OnModelSelected(this, new EkCarModel() { Id = this._selectedModelId.Value,  ManufacturerId = this._selectedManufacturerId.Value });
                        //UpdateModifications();
                        break;
                    case CategoryTypeEnum.CarModelModification:
                        ProcessModificationSelection(category, orderedBreadcrumbs);
                        return;
                    default:
                        return;
                }

                Breadcrumbs = orderedBreadcrumbs;
            }
        }
        public event EventHandler<string> TopCategorySelected;
        private void OnTopCategorySelected(string e)
        {
            TopCategorySelected?.Invoke(this, e);
        }
        private TecDocTypeEnum? _selectedTecDocType;

        private int? _selectedManufacturerId;

        private int? _selectedModelId;

        private readonly object _stateLocker = new object();

        private SearchProviderRequest _currentApiRequest;

        private DateTime _modificationsRequestedOn;

        private void UpdateModifications()
        {
            lock (_stateLocker)
            {
                if (_selectedTecDocType == null
                    || _selectedManufacturerId == null
                    || _selectedModelId == null)
                {
                    return;
                }



               
                _modificationsRequest = new EkKioskCarModelModificationsGetRequest()
                    {
                        ManufacturerId = _selectedManufacturerId.Value,
                        ModelId = _selectedModelId.Value,
                        TecDocType = _selectedTecDocType.Value,
                    };

                Categories = null;
                _currentApiRequest?.Cancel();

                _modificationsRequestedOn = DateTime.Now;
                _currentApiRequest = new SearchProviderRequest(nameof(ModificationsRequestAsync), ModificationsRequestAsync);
                _currentApiRequest.RunRequest(_modificationsRequestedOn, null);
            }
        }

        private EkKioskCarModelModificationsGetRequest _modificationsRequest;

        private async Task ModificationsRequestAsync(DateTime termTime, string term, CancellationToken cancellationToken)
        {
            try
            {
                // todo: pass via parameter
                var request = _modificationsRequest;
                SearchState = SearchStateEnum.Searching;

                var response = await ServerApiHelper.CarModelModificationsAsync(
                    request,
                    cancellationToken);
                var modifications = response.ModelModifications;
                if (modifications != null
                    && modifications.Length == 0)
                {
                    modifications = null;
                }

                lock (_stateLocker)
                {
                    if (termTime != _modificationsRequestedOn)
                    {
                        // request is expired
                        return;
                    }

                    Categories = modifications?
                        .Select(x => new Category(x)
                            {
                                ParentCategoryId = request.ModelId.ToString(),
                            })
                        .ToArray();

                    SearchState = Categories == null
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

        private Category[] ProcessSelectionTree(Category selectedCategory)
        {
            _selectedTecDocType = null;
            _selectedManufacturerId = null;
            _selectedModelId = null;
            var breadcrumbs = new List<Category>();
            var parentCategory = selectedCategory;
            do
            {
                breadcrumbs.Add(parentCategory);
                switch (parentCategory.Type)
                {
                    case CategoryTypeEnum.CarGroup:
                        parentCategory = null;
                        break;
                    case CategoryTypeEnum.CarManufacturer:
                        _selectedManufacturerId = parentCategory.CarManufacturer.Id;
                        parentCategory = _allGroups.GetValueOrDefault(parentCategory.ParentCategoryId);
                        break;
                    case CategoryTypeEnum.CarModel:
                        _selectedModelId = parentCategory.CarModel.Id;
                        _selectedTecDocType = parentCategory.CarModel.TecDocType;
                        parentCategory = _allManufacturers.GetValueOrDefault(parentCategory.ParentCategoryId);
                        break;
                    case CategoryTypeEnum.CarModelModification:
                        parentCategory = _allModels.GetValueOrDefault(parentCategory.ParentCategoryId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parentCategory.Type), parentCategory.Type, null);
                }
            }
            while (parentCategory != null);

            return breadcrumbs
                .AsEnumerable()
                .Reverse()
                .ToArray();
        }

        private void ProcessModificationSelection(Category category, Category[] orderedBreadcrumbs)
        {
            Assure.ArgumentNotNull(category, nameof(category));
            Assure.ArgumentNotNull(orderedBreadcrumbs, nameof(orderedBreadcrumbs));
            Assure.CheckFlowState(orderedBreadcrumbs.Length == 4, $"{nameof(orderedBreadcrumbs)} should contain 4 elements (including modification).");
            Assure.CheckFlowState(orderedBreadcrumbs[2].Type == CategoryTypeEnum.CarModel, $"{nameof(orderedBreadcrumbs)} 3rd element is not {nameof(CategoryTypeEnum.CarModel)}.");

            OnCategorySelected(new SelectedCategoryValue()
                {
                    Name1 = string.Join(
                        " - ",
                        // group - manufacturer
                        orderedBreadcrumbs
                            .Take(2)
                            .Select(x => x.Name)),
                    Name2 = string.Join(
                        " ",
                        // model modification
                        orderedBreadcrumbs
                            .Skip(2)
                            .Take(2)
                            .Select(x => x.Name)),
                    Id = category.Id,
                    ContextCarModelId = category.CarModelModification.ModelId.ToString(),
                });
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