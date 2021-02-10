using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApplication.Models;
using KioskBrains.Server.EK.Common.Helpers;
using KioskBrains.Common.EK.Api.CarTree;
using KioskBrains.Clients.AllegroPl;
using Microsoft.Extensions.Options;
using KioskBrains.Clients.YandexTranslate;
using KioskBrains.Clients.AllegroPl.Rest.Models;
using KioskBrains.Clients.AllegroPl.Models;
using WebApplication.Classes;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private AllegroPlClient _allegroPlClient;
        private ILogger<AllegroPlClient> _logger;
        private IOptions<AllegroPlClientSettings> _settings;
        private IOptions<YandexTranslateClientSettings> _yandexSettings;
        TreeModel treeAutoParts = new TreeModel(){ };
        public HomeController(ILogger<AllegroPlClient> logger,
            IOptions<AllegroPlClientSettings> settings,
            IOptions<YandexTranslateClientSettings> yandexApiClientSettings)
        {
            _logger = logger;
            _settings = settings;
            _yandexSettings = yandexApiClientSettings;
            _allegroPlClient = new AllegroPlClient(
                settings,
                new YandexTranslateClient(yandexApiClientSettings),
                logger);
        }


        public IActionResult Index()
        {
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
            return View(new RightTreeViewModel() { manufacturer = carTree });
        }
        public IActionResult ShowMainView()
        {
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
            return View("_CarTree", carTree);
        }
        public IActionResult SelectManufactureAndModel(string carManufactureName)
        {
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
            var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault();
            treeAutoParts.Manufacturer = carManufactureName;
            treeAutoParts.Models = modelTree.Select(x => x.Name);
            //TreeModel treeModel = new TreeModel()
            //{
            //    Manufacturer = carManufactureName,
            //    Models = modelTree.Select(x => x.Name)
            //};

            return View("_CarModels", treeAutoParts);

        }
        public IActionResult ShowCategoryAutoParts(string carManufactureName, string carModel, string mainCategory)
        {
            var autoParts = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == "620").FirstOrDefault().Children;
            if (mainCategory != "undefined")
            {
                autoParts = autoParts.Where(x => x.CategoryId == mainCategory).Select(x => x.Children).FirstOrDefault();
                //if (subCategory != "undefined") {
                //    autoParts = autoParts.Where(x => x.CategoryId == subCategory).Select(x => x.Children).FirstOrDefault();
                //}
            }
            foreach (var item in autoParts)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP_"))
                {
                    item.CategoryId = "GROUP_" + item.CategoryId;
                }
            }
            TreeModel treeAutoParts = new TreeModel()
            {
                productCategory = autoParts
            };

            return View("_AutoPartsTree", treeAutoParts);
        }
        public IActionResult ShowMainSubcategories(string carManufactureName, string carModel, string mainCategory)
        {
            var autoPartsSubCategories = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == "620").FirstOrDefault().Children.Where(x => x.CategoryId == mainCategory).Select(x => x.Children).FirstOrDefault();

            foreach (var item in autoPartsSubCategories)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP_"))
                {
                    item.CategoryId = "GROUP_" + item.CategoryId;
                }
            }
            TreeModel treeAutoParts = new TreeModel()
            {
                productCategory = autoPartsSubCategories,
                MainCategoryId = mainCategory
            };

            return View("_AutoPartsSubTree", treeAutoParts);
        }
        public IActionResult ShowMainSubChildssCategories(string carManufactureName, string carModel, string mainCategory, string subCategory)
        {
            var autoPartsSubChildCategories = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == "620").FirstOrDefault().Children.Where(x => x.CategoryId == mainCategory).Select(x => x.Children).FirstOrDefault();
            autoPartsSubChildCategories = autoPartsSubChildCategories.Where(x => x.CategoryId == subCategory).Select(x => x.Children).FirstOrDefault();
            if (autoPartsSubChildCategories == null)
            {
                return View("_ProductsList", "Retrun allegro list from MainSubCategory");
            }
            foreach (var item in autoPartsSubChildCategories)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP_"))
                {
                    item.CategoryId = "GROUP_" + item.CategoryId;
                }
            }
            TreeModel treeAutoParts = new TreeModel()
            {
                productCategory = autoPartsSubChildCategories
            };
            return View("_AutoPartsSubChildsTree", treeAutoParts);

        }
        public IActionResult ShowProductList()
        {
            return View("_ProductsList", "Retrun allegro list from MainSubCategory");
        }
        // ============Block for left part of site
        public IActionResult PartNumberInput(string partNumber)
        {
            var apitest = TestGet(partNumber).Result;
            return Json(partNumber);
        }
        public async Task<SearchOffersResponse> TestGet(string partNumber)
        {
            var apiResponse = await _allegroPlClient.SearchOffersAsync(partNumber, partNumber, "3", KioskBrains.Clients.AllegroPl.Models.OfferStateEnum.All, KioskBrains.Clients.AllegroPl.Models.OfferSortingEnum.Relevance, 0, 10, System.Threading.CancellationToken.None);
            return apiResponse;
        }
        // ============Block for left part of site
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
