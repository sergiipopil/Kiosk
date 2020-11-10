using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using KioskApp.Search;

namespace KioskApp.Ek.Catalog
{
    public sealed partial class CarModelModificationPresenter : UserControl
    {
        public CarModelModificationPresenter()
        {
            InitializeComponent();
        }

        #region CarModelModification

        public static readonly DependencyProperty CarModelModificationProperty = DependencyProperty.Register(
            nameof(CarModelModification), typeof(CarModelModification), typeof(CarModelModificationPresenter), new PropertyMetadata(default(CarModelModification)));

        public CarModelModification CarModelModification
        {
            get => (CarModelModification)GetValue(CarModelModificationProperty);
            set => SetValue(CarModelModificationProperty, value);
        }

        #endregion

        #region ModificationSelectedCommand

        public static readonly DependencyProperty ModificationSelectedCommandProperty = DependencyProperty.Register(
            nameof(ModificationSelectedCommand), typeof(ICommand), typeof(CarModelModificationPresenter), new PropertyMetadata(default(ICommand)));

        public ICommand ModificationSelectedCommand
        {
            get => (ICommand)GetValue(ModificationSelectedCommandProperty);
            set => SetValue(ModificationSelectedCommandProperty, value);
        }

        #endregion

        private void RootElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            ModificationSelectedCommand?.Execute(CarModelModification);
        }
    }
}