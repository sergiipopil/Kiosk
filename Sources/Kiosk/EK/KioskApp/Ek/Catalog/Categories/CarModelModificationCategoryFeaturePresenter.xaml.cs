using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Catalog.Categories
{
    public sealed partial class CarModelModificationCategoryFeaturePresenter : UserControl
    {
        public CarModelModificationCategoryFeaturePresenter()
        {
            InitializeComponent();
        }

        #region Label

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(CarModelModificationCategoryFeaturePresenter), new PropertyMetadata(default(string)));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        #endregion

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(string), typeof(CarModelModificationCategoryFeaturePresenter), new PropertyMetadata(default(string)));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        #endregion

        #region StartSeparator

        public static readonly DependencyProperty StartSeparatorProperty = DependencyProperty.Register(
            nameof(StartSeparator), typeof(bool), typeof(CarModelModificationCategoryFeaturePresenter), new PropertyMetadata(default(bool)));

        public bool StartSeparator
        {
            get => (bool)GetValue(StartSeparatorProperty);
            set => SetValue(StartSeparatorProperty, value);
        }

        #endregion
    }
}