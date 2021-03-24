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
using KioskBrains.Clients.AllegroPl.ServiceInterfaces;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Domain.Helpers.Dates;
using KioskBrains.Server.Domain.Actions.EkKiosk;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using X.PagedList;

namespace WebApplication.Controllers
{

    public class HomeController : Controller
    {
        public static EkCarTypeEnum _topCategoryCarType;
        public static int pageSize=10;
        private AllegroPlClient _allegroPlClient;
        private ILogger<AllegroPlClient> _logger;
        private IOptions<AllegroPlClientSettings> _settings;
        private ITranslateService _translateService;

        private readonly CentralBankExchangeRateManager _centralBankExchangeRateManager;
        private IOptions<YandexTranslateClientSettings> _yandexSettings;

             
        public HomeController(ILogger<AllegroPlClient> logger,
            IOptions<AllegroPlClientSettings> settings,
            IOptions<YandexTranslateClientSettings> yandexApiClientSettings,
            ITranslateService translateService, CentralBankExchangeRateManager centralBankExchangeRateManager)
        {
            _logger = logger;
            _settings = settings;
            _yandexSettings = yandexApiClientSettings;
            _allegroPlClient = new AllegroPlClient(
                settings,
                new YandexTranslateClient(yandexApiClientSettings),
                logger);
            _translateService = translateService;
            _centralBankExchangeRateManager = centralBankExchangeRateManager;
        }


        public IActionResult Index()
        {
            var kioskId = "111";
            HttpContext.Session.SetString("kioskId", kioskId==null ? "116" : kioskId);
            ViewData["CartWidgetPrice"] = HttpContext.Session.GetString("cartWidgetPrice");
            _topCategoryCarType = EkCarTypeEnum.Car;
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
            return View(new RightTreeViewModel() { ManufacturerList = carTree });
        }
        //====================METHOD TO SWITCH TYPE CAR TOP CATEGORY ======================================
        public IActionResult ShowMainView(string topCategoryId)
        {
            _topCategoryCarType = new EkSiteFactory().GetCarTypeEnum(topCategoryId);
            if (topCategoryId == "99193" || topCategoryId== "18554") {
                var tempC = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == topCategoryId).FirstOrDefault().Children;
                return View("_AutoPartsTree", new RightTreeViewModel() { ProductCategoryList = tempC });
            }
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            return View("_CarTree", new RightTreeViewModel() { ManufacturerList = carTree });
        }
        //====================METHOD TO SHOW CAR MODELS ======================================
        public IActionResult SelectManufactureAndModel(string carManufactureName)
        {
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault();

            return View("_CarModels", new RightTreeViewModel() { ManufacturerSelected = carManufactureName, ModelsList = modelTree.Select(x => x.Name) });
        }
        //====================METHOD TO SHOW PRODUCTS CATEGORIES ======================================
        public IActionResult ShowCategoryAutoParts(string carManufactureName, string carModel)
        {
            var autoParts = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == "620").FirstOrDefault().Children;

            foreach (var item in autoParts)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP_"))
                {
                    item.CategoryId = "GROUP_" + item.CategoryId;
                }
            }

            return View("_AutoPartsTree", new RightTreeViewModel() { ProductCategoryList = autoParts, ManufacturerSelected = carManufactureName, ModelSelected = carModel });
        }
        //====================METHOD TO SHOW PRODUCTS SUBCATEGORIES ======================================
        public IActionResult ShowMainSubcategories(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName)
        {
            var autoPartsSubCategories = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == "620").FirstOrDefault().Children.Where(x => x.CategoryId == mainCategoryId).Select(x => x.Children).FirstOrDefault();

            foreach (var item in autoPartsSubCategories)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP_"))
                {
                    item.CategoryId = "GROUP_" + item.CategoryId;
                }
            }
            RightTreeViewModel treeView = new RightTreeViewModel()
            {
                ManufacturerSelected = carManufactureName,
                ModelSelected = carModel,
                ProductCategoryList = autoPartsSubCategories,
                MainCategoryId = mainCategoryId,
                MainCategoryName = mainCategoryName
            };
            return View("_AutoPartsSubTree", treeView);
        }
        public List<string> FakeListForPager(long quantity)
        {
            List<string> tempList = new List<string>();
            for (int i = 0; i < quantity; i++)
            {
                tempList.Add("");
            }
            return tempList;
        }
        //====================METHOD TO SHOW PRODUCTS(ENTERED IN PRE-LAST GROUP OF AUTOPARTS) ======================================
        public IActionResult ShowMainSubChildsCategories(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string subCategoryId, string subCategoryName)
        {
            var autoPartsSubChildCategories = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == "620").FirstOrDefault().Children.Where(x => x.CategoryId == mainCategoryId).Select(x => x.Children).FirstOrDefault();
            autoPartsSubChildCategories = autoPartsSubChildCategories.Where(x => x.CategoryId == subCategoryId).Select(x => x.Children).FirstOrDefault();
            if (autoPartsSubChildCategories == null)
            {
                var responceAllegro = GetAllegroProducts(carManufactureName, carModel, subCategoryId, null).Result;
                HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
                RightTreeViewModel treeViewModel = new RightTreeViewModel()
                {
                    ManufacturerSelected = carManufactureName,
                    ModelSelected = carModel,
                    MainCategoryId = mainCategoryId,
                    MainCategoryName = mainCategoryName,
                    SubCategoryId = subCategoryId,
                    SubCategoryName = subCategoryName,
                    PageNumber = 1,
                    FunctionReturnFromProducts = String.Format("selectMainCategory('{0}', '{1}', '{2}', '{3}')", carManufactureName, carModel, mainCategoryId, mainCategoryName),
                    AllegroOfferList = responceAllegro.Products,
                    FakeAllegroList = FakeListForPager(responceAllegro.Total),
                    ControllerName = "ShowMainSubChildsCategories"
                };
                HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeViewModel));
                return View("_ProductsList", treeViewModel);
            }
            foreach (var item in autoPartsSubChildCategories)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP_"))
                {
                    item.CategoryId = "GROUP_" + item.CategoryId;
                }
            }
            RightTreeViewModel treeView = new RightTreeViewModel()
            {
                ManufacturerSelected = carManufactureName,
                ModelSelected = carModel,
                MainCategoryId = mainCategoryId,
                MainCategoryName = mainCategoryName,
                SubCategoryId = subCategoryId,
                SubCategoryName = subCategoryName,
                ProductCategoryList = autoPartsSubChildCategories
            };
            return View("_AutoPartsSubChildsTree", treeView);

        }
        //====================METHOD TO SHOW PRODUCTS(ENTERED IN LAST GROUP OF AUTOPARTS) ======================================
        public IActionResult ShowProductList(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string subCategoryId, string subCategoryName, string subChildId, string subChildName)
        {
            var responceAllegro = GetAllegroProducts(carManufactureName, carModel, subChildId, null).Result;
            HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
            RightTreeViewModel treeView = new RightTreeViewModel()
            {
                ManufacturerSelected = carManufactureName,
                ModelSelected = carModel,
                MainCategoryId = mainCategoryId,
                MainCategoryName = mainCategoryName,
                SubCategoryId = subCategoryId,
                SubCategoryName = subCategoryName,
                SubChildCategoryId = subChildId,
                SubChildCategoryName = subChildName,
                PageNumber = 1,
                FunctionReturnFromProducts = String.Format("selectSubMainCategory('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", carManufactureName, carModel, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName),
                AllegroOfferList = responceAllegro.Products,
                ControllerName = "ShowProductList",
                FakeAllegroList = FakeListForPager(responceAllegro.Total),
            };
            HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeView));
            return View("_ProductsList", treeView);
        }
        //==================== METHOD TO GET ALLEGRO PRODUCTS BY INPUT VALUE  ======================================
        public IActionResult PartNumberInput(string partNumber)
        {
            var responceAllegro = GetAllegroProducts(null, null, null, partNumber).Result;
            RightTreeViewModel treeView = new RightTreeViewModel()
            {
                AllegroOfferList = responceAllegro.Products,
                FakeAllegroList = FakeListForPager(responceAllegro.Total),
                FunctionReturnFromProducts = "back()",
                ControllerName = "PartNumberInput",
                PartNumberValue = partNumber,
                PageNumber = 1
            };
            HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeView));
            HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
            return View("_ProductsList", treeView);

        }
        //==================== METHOD FOR GET Exchange Rate ======================================
        private async Task<decimal> GetExchangeRateAsync()
        {
            var ukrainianNow = TimeZones.GetTimeZoneNow(TimeZones.UkrainianTime);
            const string LocalCurrencyCode = "UAH";
            const string ForeignCurrencyCode = "PLN";

            // todo: cache
            var exchangeRate = await _centralBankExchangeRateManager.GetCurrentRateAsync(LocalCurrencyCode, ForeignCurrencyCode, ukrainianNow);
            if (exchangeRate == null)
            {
                throw new InvalidOperationException($"CB exchange rate for {LocalCurrencyCode}-{ForeignCurrencyCode} is not presented.");
            }

            return exchangeRate.Value;
        }
        //==================== METHOD FOR GET ALLEGRO PRODUCTS ======================================
        public async Task<EkKioskProductSearchInEuropeGetResponse> GetAllegroProducts(string carManufactureName, string carModel, string selectedCategoryId, string inputPartNumber, OfferStateEnum state = OfferStateEnum.All, OfferSortingEnum sortingPrice = OfferSortingEnum.Relevance, int pageNumber=1)
        {
            int offset = pageNumber == 1 ? 0 : pageNumber * 10;
            SearchOffersResponse searchOffersResponse;
            if (inputPartNumber == null)
            {
                searchOffersResponse = await _allegroPlClient.SearchOffersAsync(String.Format("{0} {1}", carManufactureName, carModel), null, selectedCategoryId, state, sortingPrice, offset, 10, System.Threading.CancellationToken.None);
            }
            else
            {
                searchOffersResponse = await _allegroPlClient.SearchOffersAsync(inputPartNumber, inputPartNumber, "3", state, sortingPrice, offset, 10, System.Threading.CancellationToken.None);
            }
            try
            {
                await _allegroPlClient.ApplyTranslations(_translateService, searchOffersResponse.Offers, String.Format("{0} {1}", carManufactureName, carModel), null, System.Threading.CancellationToken.None);
            }

            catch { }

            EkProduct[] products;
            if (searchOffersResponse.Offers?.Length > 0)
            {
                var exchangeRate = await GetExchangeRateAsync();

                products = searchOffersResponse.Offers
                    .Select(x => EkConvertHelper.EkAllegroPlOfferToProduct(x, exchangeRate))
                    .ToArray();
            }
            else
            {
                products = new EkProduct[0];
            }

            return new EkKioskProductSearchInEuropeGetResponse()
            {
                Products = products,
                Total = searchOffersResponse.Total,
                TranslatedTerm = searchOffersResponse.TranslatedPhrase,
            };
        }

        //================= METHOD FILTERING LIST PRODUCTS =======================
        public IActionResult FilteredList(string state, string sorting, int page)
        {
            page = page==0 ? 1 : page;
            var rightTreeViewModelString = HttpContext.Session.GetString("rightTreeViewModel");
            RightTreeViewModel rightTree = JsonSerializer.Deserialize<RightTreeViewModel>(rightTreeViewModelString);

            OfferStateEnum stateEnum = new EkSiteFactory().GetStateEnumValue(state);
            OfferSortingEnum sortingEnum = new EkSiteFactory().GetSortingEnumValue(sorting);

            switch (rightTree.ControllerName)
            {
                case "ShowProductList":
                    var responceAllegro = GetAllegroProducts(rightTree.ManufacturerSelected, rightTree.ModelSelected, rightTree.SubChildCategoryId, null, stateEnum, sortingEnum, page).Result;
                    rightTree.AllegroOfferList = responceAllegro.Products;
                    rightTree.FakeAllegroList = FakeListForPager(responceAllegro.Total);
                    rightTree.OfferState = stateEnum;
                    rightTree.OfferSorting = sortingEnum;
                    rightTree.PageNumber = page;
                    HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
                    HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(rightTree));
                    return View("_ProductsList", rightTree);

                case "PartNumberInput":
                    var responceAllegroNumberMode = GetAllegroProducts(null, null, null, rightTree.PartNumberValue, stateEnum, sortingEnum, page).Result;
                    rightTree.AllegroOfferList = responceAllegroNumberMode.Products;
                    rightTree.FakeAllegroList = FakeListForPager(responceAllegroNumberMode.Total);
                    rightTree.OfferState = stateEnum;
                    rightTree.OfferSorting = sortingEnum;
                    rightTree.PageNumber = page;
                    HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegroNumberMode.Products));
                    HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(rightTree));
                    return View("_ProductsList", rightTree);


                case "ShowMainSubChildsCategories":
                    var responceAllegroSubCategories = GetAllegroProducts(rightTree.ManufacturerSelected, rightTree.ModelSelected, rightTree.SubCategoryId, null, stateEnum, sortingEnum, page).Result;
                    rightTree.AllegroOfferList = responceAllegroSubCategories.Products;
                    rightTree.FakeAllegroList = FakeListForPager(responceAllegroSubCategories.Total);
                    rightTree.OfferState = stateEnum;
                    rightTree.OfferSorting = sortingEnum;
                    rightTree.PageNumber = page;
                    HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegroSubCategories.Products));
                    HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(rightTree));
                    return View("_ProductsList", rightTree);
            }
            return View("_ProductsList", rightTree);
        }
        //=============DEFAULT ACTIONS ==============
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult _UseTerms()
        {
            return PartialView();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
