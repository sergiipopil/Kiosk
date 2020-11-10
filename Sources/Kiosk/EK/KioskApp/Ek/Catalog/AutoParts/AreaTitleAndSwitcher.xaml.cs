using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Ek.Catalog.AutoParts.Europe;

namespace KioskApp.Ek.Catalog.AutoParts
{
    public sealed partial class AreaTitleAndSwitcher : UserControl
    {
        public AreaTitleAndSwitcher()
        {
            OnIsLocalAreaChanged();

            InitializeComponent();
        }

        #region IsLocalArea

        public static readonly DependencyProperty IsLocalAreaProperty = DependencyProperty.Register(
            nameof(IsLocalArea), typeof(bool), typeof(AreaTitleAndSwitcher), new PropertyMetadata(true, IsLocalAreaPropertyChangedCallback));

        private static void IsLocalAreaPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AreaTitleAndSwitcher)d).OnIsLocalAreaChanged();
        }

        public bool IsLocalArea
        {
            get => (bool)GetValue(IsLocalAreaProperty);
            set => SetValue(IsLocalAreaProperty, value);
        }

        #endregion

        #region AreaTitle

        public static readonly DependencyProperty AreaTitleProperty = DependencyProperty.Register(
            nameof(AreaTitle), typeof(string), typeof(AreaTitleAndSwitcher), new PropertyMetadata(default(string)));

        public string AreaTitle
        {
            get => (string)GetValue(AreaTitleProperty);
            set => SetValue(AreaTitleProperty, value);
        }

        #endregion

        #region AnotherAreaTitle

        public static readonly DependencyProperty AnotherAreaTitleProperty = DependencyProperty.Register(
            nameof(AnotherAreaTitle), typeof(string), typeof(AreaTitleAndSwitcher), new PropertyMetadata(default(string)));

        public string AnotherAreaTitle
        {
            get => (string)GetValue(AnotherAreaTitleProperty);
            set => SetValue(AnotherAreaTitleProperty, value);
        }

        #endregion

        private void OnIsLocalAreaChanged()
        {
            const string LocalAreaTitle = "в Украине";
            const string EuropeAreaTitle = "из Европы";

            if (IsLocalArea)
            {
                AreaTitle = LocalAreaTitle;
                AnotherAreaTitle = EuropeAreaTitle;
            }
            else
            {
                AreaTitle = EuropeAreaTitle;
                AnotherAreaTitle = LocalAreaTitle;
            }
        }

        // disabled - Europe only
        //private void SwitchAreaButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    var anotherAreaView = IsLocalArea
        //        ? (UserControl)new EuropeMainView()
        //        : new MainView();
        //    EkContext.Current.SetViewCommand.Execute(anotherAreaView);
        //}
    }
}