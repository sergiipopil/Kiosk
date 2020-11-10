using System.Globalization;
using KioskApp.Clients.NovaPoshtaUkraine.Models;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Search
{
    public class NovaPoshtaUkraineBranch : UiBindableObject
    {
        public NovaPoshtaUkraineBranch(WarehouseSearchItem warehouse)
        {
            Assure.ArgumentNotNull(warehouse, nameof(warehouse));

            StoreId = warehouse.SiteKey.ToString(CultureInfo.InvariantCulture);
            Number = warehouse.Number.ToString();
            _City = warehouse.CityDescriptionRu;
            _AddressLine1 = warehouse.DescriptionRu;
            if (warehouse.TotalMaxWeightAllowed > 0)
            {
                _MaxWeight = $"до {warehouse.TotalMaxWeightAllowed} кг";
            }
        }

        public string StoreId { get; set; }

        public string Number { get; set; }

        #region City

        private string _City;

        public string City
        {
            get => _City;
            set => SetProperty(ref _City, value);
        }

        #endregion

        #region AddressLine1

        private string _AddressLine1;

        public string AddressLine1
        {
            get => _AddressLine1;
            set => SetProperty(ref _AddressLine1, value);
        }

        #endregion

        #region MaxWeight

        private string _MaxWeight;

        public string MaxWeight
        {
            get => _MaxWeight;
            set => SetProperty(ref _MaxWeight, value);
        }

        #endregion
    }
}