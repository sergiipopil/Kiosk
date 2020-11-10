using System;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using KioskApp.Ek.Catalog.Categories;

namespace KioskApp.Ek.Catalog.AutoParts.Europe
{
    public sealed partial class EuropeInitialRightView : UserControl
    {
        public EuropeInitialRightView()
        {
            BackCommand = EkContext.Current.GoToMainCommand;

            InitializeComponent();
        }

        public ICommand BackCommand { get; }

        private void TopCategoryPresenter_OnClick(object sender, EventArgs e)
        {
            var categoryId = ((TopCategoryPresenter)sender).Tag as string;
            if (categoryId == null)
            {
                return;
            }

            OnTopCategorySelected(categoryId);
        }

        public event EventHandler<string> TopCategorySelected;

        private void OnTopCategorySelected(string e)
        {
            TopCategorySelected?.Invoke(this, e);
        }
    }
}