using KioskBrains.Common.EK.Api;
using KioskBrains.Common.EK.Api.CarTree;
using Newtonsoft.Json;

namespace KioskBrains.Server.EK.Common.Helpers
{
    public static class EkCategoryHelper
    {
        static EkCategoryHelper()
        {
            _europeCategories = JsonConvert.DeserializeObject<EkProductCategory[]>(Categories.EuropeCategoriesJson);
            _carModelTree = JsonConvert.DeserializeObject<EkCarGroup[]>(Categories.CarModelTreeJson);
        }

        private static readonly EkProductCategory[] _europeCategories;

        public static EkProductCategory[] GetEuropeCategories()
        {
            return _europeCategories;
        }

        private static readonly EkCarGroup[] _carModelTree;

        public static EkCarGroup[] GetCarModelTree()
        {
            return _carModelTree;
        }
    }
}