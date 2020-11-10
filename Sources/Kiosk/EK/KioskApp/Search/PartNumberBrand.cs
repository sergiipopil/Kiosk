using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Search
{
    public class PartNumberBrand : UiBindableObject
    {
        public PartNumberBrand(EkPartNumberBrand ekPartNumberBrand)
        {
            Assure.ArgumentNotNull(ekPartNumberBrand, nameof(ekPartNumberBrand));

            EkPartNumberBrand = ekPartNumberBrand;
            Name = ekPartNumberBrand.Name?.GetValue(Languages.RussianCode);
        }

        public EkPartNumberBrand EkPartNumberBrand { get; }

        public string BrandName => EkPartNumberBrand.BrandName;

        public string PartNumber => EkPartNumberBrand.PartNumber;

        #region Name

        private string _Name;

        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        #endregion
    }
}