using System;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Common.EK.Api.CarTree;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Catalog.Categories
{
    public class Category : UiBindableObject
    {
        public Category(EkProductCategory productCategory, CategorySpecialTypeEnum? specialType = null)
        {
            Assure.ArgumentNotNull(productCategory, nameof(productCategory));

            ProductCategory = productCategory;
            Type = CategoryTypeEnum.ProductCategory;
            SpecialType = specialType;

            Id = productCategory.CategoryId;
            Name = productCategory.Name?.GetValue(Languages.RussianCode);
            IsGroup = productCategory.Children?.Length > 0;
        }

        public Category(EkCarTypeEnum carType)
        {
            CarType = carType;
            Type = CategoryTypeEnum.CarGroup;

            Id = carType.ToString();
            switch (carType)
            {
                case EkCarTypeEnum.Car:
                    Name = "Легковые";
                    break;
                case EkCarTypeEnum.Truck:
                    Name = "Грузовые";
                    break;
                case EkCarTypeEnum.Bus:
                    Name = "Автобусы";
                    break;
                case EkCarTypeEnum.Special:
                    Name = "Спецтехника";
                    break;
                case EkCarTypeEnum.Moto:
                    Name = "Мототехника";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(carType), carType, null);
            }

            IsGroup = true;
        }

        public Category(EkCarTypeEnum carType, EkCarManufacturer carManufacturer)
        {
            Assure.ArgumentNotNull(carManufacturer, nameof(carManufacturer));

            CarManufacturer = carManufacturer;
            Type = CategoryTypeEnum.CarManufacturer;

            Id = $"{carType}_{carManufacturer.Id}";
            Name = string.IsNullOrEmpty(carManufacturer.DisplayName)
                ? carManufacturer.Name
                : carManufacturer.DisplayName;
            IsGroup = true;
        }

        public Category(EkCarModel carModel)
        {
            Assure.ArgumentNotNull(carModel, nameof(carModel));

            CarModel = carModel;
            Type = CategoryTypeEnum.CarModel;

            Id = carModel.Id.ToString();
            Name = carModel.Name;
            IsGroup = true;
        }

        public Category(EkCarModelModification carModelModification)
        {
            Assure.ArgumentNotNull(carModelModification, nameof(carModelModification));

            CarModelModification = carModelModification;
            Type = CategoryTypeEnum.CarModelModification;

            Id = carModelModification.Id.ToString();
            Name = carModelModification.Name?.GetValue(Languages.RussianCode);
            IsGroup = false;
        }

        public EkProductCategory ProductCategory { get; }

        public EkCarTypeEnum? CarType { get; }

        public EkCarManufacturer CarManufacturer { get; }

        public EkCarModel CarModel { get; }

        public EkCarModelModification CarModelModification { get; }

        public CategoryTypeEnum Type { get; }

        public CategorySpecialTypeEnum? SpecialType { get; }

        public string Id { get; set; }

        #region Name

        private string _Name;

        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        public string CategoryUrl => $"/Themes/Assets/Images/Catalog/AutoParts/" + Id + ".png";

        #endregion

        public bool IsGroup { get; set; }

        public string ParentCategoryId { get; set; }

        public string ParentCategoryName { get; set; }
    }
}