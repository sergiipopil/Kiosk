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
        public void SetActiveGroup(string tag) {
            categoryAutoParts.IsActive = false;
            categoryTruckModels.IsActive = false;
            categoryBusModels.IsActive = false;
            categorySpecialCar.IsActive = false;
            categoryDisks.IsActive = false;
            categorySTO.IsActive = false;
            categoryMoto.IsActive = false;

            if (tag == "Car") {
                categoryAutoParts.IsActive = true;
            }
            if (tag == "Truck")
            {
                categoryTruckModels.IsActive = true;
            }
            if (tag == "Bus")
            {
                categoryBusModels.IsActive = true;
            }
            if (tag == "Special")
            {
                categorySpecialCar.IsActive = true;
            }
            if (tag == "Moto")
            {
                categoryMoto.IsActive = true;
            }
            if (tag == "GROUP_99193")
            {
                categoryDisks.IsActive = true;
            }
            if (tag == "GROUP_18554")
            {
                categorySTO.IsActive = true;
            }
        }
        private void TopCategoryPresenter_OnClick(object sender, EventArgs e)
        {
            var categoryId = ((TopCategoryPresenter)sender).Tag as string;

            categoryAutoParts.IsActive = false;
            categoryTruckModels.IsActive = false;
            categoryBusModels.IsActive = false;
            categorySpecialCar.IsActive = false;
            categoryDisks.IsActive = false;
            categorySTO.IsActive = false;

            ((TopCategoryPresenter)sender).IsActive = true;
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