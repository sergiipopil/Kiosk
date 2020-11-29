using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Catalog.Categories
{
    public class CategoryDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ProductCategoryGroup { get; set; }

        public DataTemplate ProductCategoryLeaf { get; set; }

        public DataTemplate CarManufacturer { get; set; }

        public DataTemplate CarModel { get; set; }

        public DataTemplate CarModelModification { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is Category category)
            {
                switch (category.Type)
                {
                    case CategoryTypeEnum.ProductCategory:
                        return 
                             ProductCategoryGroup;

                    case CategoryTypeEnum.CarManufacturer:
                        return CarManufacturer;

                    case CategoryTypeEnum.CarModel:
                        return CarModel;

                    case CategoryTypeEnum.CarModelModification:
                        return CarModelModification;
                }
            }

            return base.SelectTemplateCore(item);
        }
    }
}