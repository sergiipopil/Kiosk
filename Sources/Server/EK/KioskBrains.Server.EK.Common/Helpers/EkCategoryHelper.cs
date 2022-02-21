using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        public static IList<string> GetAdvertCities() {
            return new List<string>() { "Мукачево", "Берегово", "Ужгород", "Свалява", "Днепр", "Запорожье", "Киев", "Мелитополь", "Харьков", "Суммы", "Мариуполь", "Житомир",
                "Иршава", "Кривой Рог", "Одесса", "Львов", "Винница", "Чернигов", "Бердянск", "Херсон", "Полтава", "Хмельницкий", "Ровно", "Черновцы", "Ивано-Франковск",
                "Каменское", "Кропивницкий", "Тернополь", "Кременчуг", "Луцк", "Белая Церковь", "Никополь", "Бровары", "Павлоград", "Северодонецк" };
        }
        public static IList<string> GetRandomParts()
        {
            //IList<string> testingList = System.IO.File.ReadAllText(@"c:\temp\partNumbers.txt").Split(";").ToList();
            IList<string> testingList = System.IO.File.ReadAllText(@"D:\Domains\bi-bi.com.ua\httpdocs\wwwroot\partNumbers.txt").Split(";").ToList();
            return testingList.AsEnumerable().OrderBy(n => Guid.NewGuid()).Take(100).ToList();
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