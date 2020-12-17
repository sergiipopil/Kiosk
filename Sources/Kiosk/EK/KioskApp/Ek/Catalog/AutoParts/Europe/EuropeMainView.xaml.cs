using System.Linq;
using System.Windows.Input;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Ek.Catalog.Categories;
using KioskApp.CoreExtension.Application;
using KioskApp.Search;
using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui;
using KioskBrains.Common.EK.Api.CarTree;

namespace KioskApp.Ek.Catalog.AutoParts.Europe
{
    public sealed partial class EuropeMainView : UserControl
    {
        public EuropeMainView()
        {
            ShowCarsControl = true;
            SearchTypeSelectedCommand = new RelayCommand(
                 nameof(SearchTypeSelectedCommand),
                 parameter => OnSearchTypeSelected(parameter as SearchTypeEnum?));

            SetInitialViewCommand = new RelayCommand(
                nameof(SetInitialViewCommand),
                x => SetInitialViews());
            SetSearchByNameViewCommand = new RelayCommand(
                nameof(SetSearchByNameViewCommand),
                x => SetSearchByNameViews());
            BackToCategorySelectionCommand = new RelayCommand(
                nameof(BackToCategorySelectionCommand),
                x =>
                    {
                        if (SelectedCategory == null)
                        {
                            return;
                        }

                        SetSelectCategoryViews(SelectedCategory.Id);
                    });

            SetInitialViews();

            InitializeComponent();
        }

        private void EuropeMainView_OnLoaded(object sender, RoutedEventArgs e)
        {
            LanguageManager.Current.LanguageChanged += OnLanguageChanged;

            UpdateLabels();
        }

        private void EuropeMainView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            LanguageManager.Current.LanguageChanged -= OnLanguageChanged;
        }

        #region Globalization

        private void UpdateLabels()
        {
            ThreadHelper.EnsureUiThread();

            foreach (var item in SearchByAnotherTypeMenuItems)
            {
                item.UpdateText();
            }
        }

        private void OnLanguageChanged(object sender, Language e)
        {
            ThreadHelper.RunInUiThreadAsync(UpdateLabels);
        }

        #endregion

        #region IsLeftSidePanelWidthExtended

        public static readonly DependencyProperty IsLeftSidePanelWidthExtendedProperty = DependencyProperty.Register(
            nameof(IsLeftSidePanelWidthExtended), typeof(bool), typeof(EuropeMainView), new PropertyMetadata(default(bool)));

        public bool IsLeftSidePanelWidthExtended
        {
            get => (bool)GetValue(IsLeftSidePanelWidthExtendedProperty);
            set => SetValue(IsLeftSidePanelWidthExtendedProperty, value);
        }

        #endregion

        #region ShowSearchByAnotherTypeMenu

        public static readonly DependencyProperty ShowSearchByAnotherTypeMenuProperty = DependencyProperty.Register(
            nameof(ShowSearchByAnotherTypeMenu), typeof(bool), typeof(EuropeMainView), new PropertyMetadata(default(bool)));

        public bool ShowSearchByAnotherTypeMenu
        {
            get => (bool)GetValue(ShowSearchByAnotherTypeMenuProperty);
            set => SetValue(ShowSearchByAnotherTypeMenuProperty, value);
        }

        #endregion

        #region ShowCategorySelection

        public static readonly DependencyProperty ShowCategorySelectionProperty = DependencyProperty.Register(
            nameof(ShowCategorySelection), typeof(bool), typeof(EuropeMainView), new PropertyMetadata(default(bool)));

        public bool ShowCategorySelection
        {
            get => (bool)GetValue(ShowCategorySelectionProperty);
            set => SetValue(ShowCategorySelectionProperty, value);
        }

        #endregion

        #region ShowCarsControl

        public static readonly DependencyProperty ShowCarsControlProperty = DependencyProperty.Register(
            nameof(ShowCarsControl), typeof(bool), typeof(EuropeMainView), new PropertyMetadata(default(bool)));

        public bool ShowCarsControl
        {
            get => (bool)GetValue(ShowCarsControlProperty);
            set => SetValue(ShowCarsControlProperty, value);
        }

        #endregion

        #region LeftView

        public static readonly DependencyProperty LeftViewProperty = DependencyProperty.Register(
            nameof(LeftView), typeof(UserControl), typeof(EuropeMainView), new PropertyMetadata(default(UserControl)));

        public UserControl LeftView
        {
            get => (UserControl)GetValue(LeftViewProperty);
            set => SetValue(LeftViewProperty, value);
        }

        #endregion

        #region RightView

        public static readonly DependencyProperty RightViewProperty = DependencyProperty.Register(
            nameof(RightView), typeof(UserControl), typeof(EuropeMainView), new PropertyMetadata(default(UserControl)));

        public UserControl RightView
        {
            get => (UserControl)GetValue(RightViewProperty);
            set => SetValue(RightViewProperty, value);
        }

        #endregion

        #region RightBottomView

        public static readonly DependencyProperty RightBottomViewProperty = DependencyProperty.Register(
            nameof(RightBottomView), typeof(UserControl), typeof(EuropeMainView), new PropertyMetadata(default(UserControl)));

        public UserControl RightBottomView
        {
            get => (UserControl)GetValue(RightBottomViewProperty);
            set => SetValue(RightBottomViewProperty, value);
        }

        #endregion

        public SearchByAnotherTypeMenuItem[] SearchByAnotherTypeMenuItems { get; } = new[]
            {
                new SearchByAnotherTypeMenuItem("SearchTypeLink_SearchByPartNumber", SearchTypeEnum.ByName),
                //new SearchByAnotherTypeMenuItem("SearchTypeLink_SearchByCategory", SearchTypeEnum.ByCategory),
            };

        private void ShowOnlySearchByAnotherTypeMenuItems(params SearchTypeEnum[] searchTypes)
        {
            foreach (var item in SearchByAnotherTypeMenuItems)
            {
                item.IsVisible = searchTypes.Contains(item.SearchType);
            }
        }
        string _selectedMainCategoryId;
        private string _selectedCarType;
        private void SetInitialViews()
        {

            ShowCarsControl = true;
            IsLeftSidePanelWidthExtended = true;
            ShowSearchByAnotherTypeMenu = false;
            ShowCategorySelection = false;
            LeftView = new EuropeInitialLeftView()
            {
                SearchTypeSelectedCommand = SearchTypeSelectedCommand,
            };


            //carModelTreeSearchProvider.CategorySelected += (sender, selectedCategory) =>
            //{
            //    _searchByCategoryContext.SelectedModification = selectedCategory;
            //    _searchByCategoryContext.SelectedCategory = null;
            //    SetSelectCategoryViews(NavigationType.NewSearch);
            //};
            var carModelTreeSearchProvider = new CarModelTreeSearchProvider(EkCarTypeEnum.Car, _modelId.ToString(), SetInitialViews)
            {
                OnModelSelected = (sender, model) =>
                {
                    _modelId = model.Id; _selectedCarType = model.CarType.ToString(); SetSelectCategoryViews(GetCategoryByCarType(model.CarType));
                    SelectedCategory = new SelectedCategoryValue()
                    {
                        Name1 = model.Path,
                        SelectedCarModelURL = model.SelectedModelURL,
                        Id = _modelId.ToString()
                    };
                },
                OnManufacturerSelected = (sender, name) =>
                ThreadHelper.RunInUiThreadAsync(() =>
                {
                    SelectedCategory = new SelectedCategoryValue()
                    {                        
                        SelectedManufactureURL = name.SelectedManufactureURL,
                        Name1 = name.Path,
                        Id = ""
                    };
                    ShowCategorySelection = !string.IsNullOrEmpty(name.ManufacturerName);
                    if (!string.IsNullOrEmpty(name.ManufacturerName))
                    {
                        LeftView = null;
                    }
                    else
                    {
                        LeftView = new EuropeInitialLeftView()
                        {
                            SearchTypeSelectedCommand = SearchTypeSelectedCommand,
                        };
                    }

                })
            };

            var initialRightView = new EuropeInitialRightView();
            if (SelectedCategory != null)
                initialRightView.SetActiveGroup(_selectedCarType);
            initialRightView.TopCategorySelected += (sender, categoryId) => SetSelectCategoryViews(categoryId);
            RightView = initialRightView;




            _modelId = 0;

            carProvider = carModelTreeSearchProvider;
            RightBottomView = new CategorySearchRightView()
            {
                SearchProvider = carModelTreeSearchProvider,
                //OnModelSelected = (sender, categoryId) => SetSelectCategoryViews(categoryId)
            };

        }
        CarModelTreeSearchProvider carProvider;

        private void SetSelectManufacturerViews(string categoryId)
        {
            EkCarTypeEnum typeTransport = EkCarTypeEnum.Car;
            switch (categoryId)
            {
                case "620":
                    typeTransport = EkCarTypeEnum.Car;
                    break;
                case "621":
                    typeTransport = EkCarTypeEnum.Truck;
                    break;
                case "622":
                    typeTransport = EkCarTypeEnum.Bus;
                    break;
                case "156":
                    typeTransport = EkCarTypeEnum.Moto;
                    break;
                case "99022":
                    typeTransport = EkCarTypeEnum.Special;
                    break;
                default: break;
            }
            carProvider.InitModelTree(typeTransport, null, null);
        }

        private string GetCategoryByCarType(EkCarTypeEnum type)
        {
            switch (type)
            {
                case EkCarTypeEnum.Car:
                    return "620";
                case EkCarTypeEnum.Truck:
                    return "620";
                case EkCarTypeEnum.Bus:
                    return "620";
                case EkCarTypeEnum.Moto:
                    return "156";
                case EkCarTypeEnum.Special:

                    return "99022";
                default: return "620";
            }
        }


        private void SetSearchByNameViews()
        {
            EkContext.Current.EkProcess?.OnViewChanged("Europe.SearchByName", false);
            ShowCarsControl = false;
            IsLeftSidePanelWidthExtended = false;
            ShowSearchByAnotherTypeMenu = true;
            ShowCategorySelection = SelectedCategory != null;//fix show car control !!!ShowCarsControl   
            ShowOnlySearchByAnotherTypeMenuItems(SearchTypeEnum.ByName, SearchTypeEnum.ByCategory);

            if (SelectedCategory != null)
            {
                SelectedCategory.ContextCarModelId = _modelId.ToString();
            }
            var productSearchInEuropeProvider = new ProductSearchInEuropeProvider()
            {
                SelectedCategory = SelectedCategory,
            };
            LeftView = new EuropeSearchLeftView(SelectedCategory != null)
            {
                SearchProvider = productSearchInEuropeProvider,
            };
            RightView = new EuropeSearchRightView()
            {
                SearchProvider = productSearchInEuropeProvider,
                BackCommand = SelectedCategory == null
                        ? SetInitialViewCommand
                        : BackToCategorySelectionCommand,
            };
        }

        #region SelectedCategory

        public static readonly DependencyProperty SelectedCategoryProperty = DependencyProperty.Register(
            nameof(SelectedCategory), typeof(SelectedCategoryValue), typeof(EuropeMainView), new PropertyMetadata(default(SelectedCategoryValue)));

        public SelectedCategoryValue SelectedCategory
        {
            get => (SelectedCategoryValue)GetValue(SelectedCategoryProperty);
            set => SetValue(SelectedCategoryProperty, value);
        }

        #endregion

        private int _manufacturerId;
        private int _modelId;

        private void SetSelectCategoryViews(string initialCategoryId)
        {
            if ((initialCategoryId == "621" || initialCategoryId == "620" || initialCategoryId == "622" || initialCategoryId == "99022" || initialCategoryId == "156") && _modelId == 0)
            {
                SetSelectManufacturerViews(initialCategoryId);
                return;
            }
            {
                ShowCarsControl = false;
            }


            EkContext.Current.EkProcess?.OnViewChanged("Europe.SelectCategory", false);

            //SelectedCategory = null;
            IsLeftSidePanelWidthExtended = false;
            _selectedMainCategoryId = initialCategoryId;
            ShowSearchByAnotherTypeMenu = true;
            ShowCategorySelection = true;
            ShowOnlySearchByAnotherTypeMenuItems(SearchTypeEnum.ByName, SearchTypeEnum.ByCategory);

            var categorySearchInEuropeProvider = new CategorySearchInEuropeProvider(initialCategoryId, _modelId, SetInitialViews);
            categorySearchInEuropeProvider.CategorySelected += (sender, selectedCategory) =>
                {
                    if (selectedCategory.Name2.Contains("Легковые и микроавтобусы"))
                    {
                        selectedCategory.Name2 = selectedCategory.Name2.Replace("Легковые и микроавтобусы", "");
                    }
                    SelectedCategory = selectedCategory;
                    if (!selectedCategory.Id.Contains("GROUP_") && selectedCategory.Id != "621" && selectedCategory.Id != "620" && selectedCategory.Id != "622" && selectedCategory.Id != "99022" && selectedCategory.Id != "156")
                        SetSearchByNameViews();
                };
            LeftView = null;
            //if (initialCategoryId != "628"&&initialCategoryId != "621" && initialCategoryId != "620" && initialCategoryId != "622") { 
            RightView = new CategorySearchRightView()
            {
                SearchProvider = categorySearchInEuropeProvider,
            };

            // }
        }

        public ICommand SearchTypeSelectedCommand { get; }

        private void OnSearchTypeSelected(SearchTypeEnum? searchType)
        {
            SelectedCategory = null;

            switch (searchType)
            {
                case SearchTypeEnum.ByName:
                    SetSearchByNameViews();
                    break;
                case SearchTypeEnum.ByCategory:
                    SetInitialViews();
                    break;
            }
        }

        private void SearchByAnotherTypeMenu_OnSearchByAnotherTypeSelected(object sender, SearchTypeSelectedEventArgs e)
        {
            OnSearchTypeSelected(e.SearchType);
        }

        public ICommand SetInitialViewCommand { get; }

        public ICommand SetSearchByNameViewCommand { get; }

        public ICommand BackToCategorySelectionCommand { get; }
    }
}