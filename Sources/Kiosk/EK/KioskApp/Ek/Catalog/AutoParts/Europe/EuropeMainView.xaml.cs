using System.Linq;
using System.Windows.Input;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Ek.Catalog.Categories;
using KioskApp.Search;
using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui;

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
                new SearchByAnotherTypeMenuItem("SearchTypeLink_SearchByCategory", SearchTypeEnum.ByCategory),
            };

        private void ShowOnlySearchByAnotherTypeMenuItems(params SearchTypeEnum[] searchTypes)
        {
            foreach (var item in SearchByAnotherTypeMenuItems)
            {
                item.IsVisible = searchTypes.Contains(item.SearchType);
            }
        }

        private void SetInitialViews()
        {           
            _modelId = 0;
            ShowCarsControl = true;
            IsLeftSidePanelWidthExtended = true;
            ShowSearchByAnotherTypeMenu = false;
            ShowCategorySelection = false;
            LeftView = new EuropeInitialLeftView()
                {
                    SearchTypeSelectedCommand = SearchTypeSelectedCommand,
                };

            var carModelTreeSearchProvider = new CarModelTreeSearchProvider(KioskBrains.Common.EK.Api.CarTree.EkCarTypeEnum.Car, null, SetInitialViews) {
                OnModelSelected = (sender, model) => { _modelId = model.Id; SetSelectCategoryViews("620"); }
            };
            //carModelTreeSearchProvider.CategorySelected += (sender, selectedCategory) =>
            //{
            //    _searchByCategoryContext.SelectedModification = selectedCategory;
            //    _searchByCategoryContext.SelectedCategory = null;
            //    SetSelectCategoryViews(NavigationType.NewSearch);
            //};


            var initialRightView = new EuropeInitialRightView();
            initialRightView.TopCategorySelected += (sender, categoryId) => SetSelectCategoryViews(categoryId);
            RightView = initialRightView;

            RightBottomView = new CategorySearchRightView()
            {
                SearchProvider = carModelTreeSearchProvider,
                //OnModelSelected = (sender, categoryId) => SetSelectCategoryViews(categoryId)
        };
        }


        private void SetSearchByNameViews()
        {
            EkContext.Current.EkProcess?.OnViewChanged("Europe.SearchByName", false);

            IsLeftSidePanelWidthExtended = false;
            ShowSearchByAnotherTypeMenu = true;
            ShowCategorySelection = SelectedCategory != null;
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
            EkContext.Current.EkProcess?.OnViewChanged("Europe.SelectCategory", false);
            ShowCarsControl = false;
            SelectedCategory = null;
            IsLeftSidePanelWidthExtended = false;
            ShowSearchByAnotherTypeMenu = true;
            ShowCategorySelection = true;
            ShowOnlySearchByAnotherTypeMenuItems(SearchTypeEnum.ByName, SearchTypeEnum.ByCategory);
            var categorySearchInEuropeProvider = new CategorySearchInEuropeProvider(initialCategoryId, SetInitialViews);
            categorySearchInEuropeProvider.CategorySelected += (sender, selectedCategory) =>
                {
                    SelectedCategory = selectedCategory;
                    SetSearchByNameViews();
                };
            LeftView = null;
            RightView = new CategorySearchRightView()
                {
                    SearchProvider = categorySearchInEuropeProvider,
                };
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