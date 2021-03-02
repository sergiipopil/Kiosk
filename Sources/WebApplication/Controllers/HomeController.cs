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
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication.Controllers
{

    public class HomeController : Controller
    {
        public static EkCarTypeEnum _topCategoryCarType;
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
            _topCategoryCarType = EkCarTypeEnum.Car;
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
            return View(new RightTreeViewModel() { ManufacturerList = carTree });
        }
        public IActionResult ShowMainView(string topCategoryId)
        {
            _topCategoryCarType = new GetCarTypeEnumByNumber().GetCarTypeEnum(topCategoryId);
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            return View("_CarTree", new RightTreeViewModel() { ManufacturerList = carTree });
        }
        public IActionResult SelectManufactureAndModel(string carManufactureName)
        {
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault();

            return View("_CarModels", new RightTreeViewModel() { ManufacturerSelected = carManufactureName, ModelsList = modelTree.Select(x => x.Name) });
        }
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
        public IActionResult ShowMainSubChildsCategories(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string subCategoryId, string subCategoryName)
        {
            var autoPartsSubChildCategories = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == "620").FirstOrDefault().Children.Where(x => x.CategoryId == mainCategoryId).Select(x => x.Children).FirstOrDefault();
            autoPartsSubChildCategories = autoPartsSubChildCategories.Where(x => x.CategoryId == subCategoryId).Select(x => x.Children).FirstOrDefault();
            if (autoPartsSubChildCategories == null)
            {
                var responceAllegro = GetDetailsFromTree(carManufactureName, carModel, subCategoryId, null).Result;
                long pages = responceAllegro.Total / 10;
                HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
                RightTreeViewModel treeViewModel = new RightTreeViewModel()
                {
                    ManufacturerSelected = carManufactureName,
                    ModelSelected = carModel,
                    MainCategoryId = mainCategoryId,
                    MainCategoryName = mainCategoryName,
                    SubCategoryId = subCategoryId,
                    SubCategoryName = subCategoryName,
                    FunctionReturnFromProducts = String.Format("selectMainCategory('{0}', '{1}', '{2}', '{3}')", carManufactureName, carModel, mainCategoryId, mainCategoryName),
                    AllegroOfferList = responceAllegro.Products,
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
        public IActionResult ShowProductList(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string subCategoryId, string subCategoryName, string subChildId, string subChildName)
        {
            var responceAllegro = GetDetailsFromTree(carManufactureName, carModel, subChildId, null).Result;
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
                FunctionReturnFromProducts = String.Format("selectSubMainCategory('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", carManufactureName, carModel, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName),
                AllegroOfferList = responceAllegro.Products,
                ControllerName = "ShowProductList"
            };
            HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeView));
            return View("_ProductsList", treeView);
        }
        // ============Block for left part of site
        public IActionResult PartNumberInput(string partNumber)
        {
            var responceAllegro = GetDetailsFromTree(null, null, null, partNumber).Result;
            RightTreeViewModel treeView = new RightTreeViewModel()
            {
                AllegroOfferList = responceAllegro.Products,
                FunctionReturnFromProducts = "back()",
                ControllerName = "PartNumberInput",
                PartNumberValue = partNumber
            };
            HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeView));
            HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
            return View("_ProductsList", treeView);

        }

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
        public async Task<EkKioskProductSearchInEuropeGetResponse> GetDetailsFromTree(string carManufactureName, string carModel, string selectedCategoryId, string inputPartNumber, OfferStateEnum state = OfferStateEnum.All, OfferSortingEnum sortingPrice = OfferSortingEnum.Relevance)
        {
            SearchOffersResponse searchOffersResponse;
            if (inputPartNumber == null)
            {
                searchOffersResponse = await _allegroPlClient.SearchOffersAsync(String.Format("{0} {1}", carManufactureName, carModel), null, selectedCategoryId, state, sortingPrice, 0, 10, System.Threading.CancellationToken.None);
            }
            else
            {
                searchOffersResponse = await _allegroPlClient.SearchOffersAsync(inputPartNumber, inputPartNumber, "3", state, sortingPrice, 0, 10, System.Threading.CancellationToken.None);
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
        // ============Block for left part of site
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult _UseTerms()
        {
            return PartialView();
        }
        public IActionResult _Details(string id)
        {
            return PartialView("_Details", id);
        }
        public IActionResult _DetailsTest(string id)
        {
            return PartialView("_DetailsTest", id);
        }

        public virtual ActionResult EditFeed(string id)
        {
            return View("_Details", id);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult FilteredList(string state, string sorting)
        {
            var rightTreeViewModelString = HttpContext.Session.GetString("rightTreeViewModel");
            RightTreeViewModel rightTree = JsonSerializer.Deserialize<RightTreeViewModel>(rightTreeViewModelString);

            OfferStateEnum stateEnum = GetStateEnumValue(state);
            OfferSortingEnum sortingEnum = GetSortingEnumValue(sorting);

            switch (rightTree.ControllerName)
            {
                case "ShowProductList":
                    var responceAllegro = GetDetailsFromTree(rightTree.ManufacturerSelected, rightTree.ModelSelected, rightTree.SubChildCategoryId, null, stateEnum, sortingEnum).Result;
                    rightTree.AllegroOfferList = responceAllegro.Products;
                    rightTree.OfferState = stateEnum;
                    rightTree.OfferSorting = sortingEnum;
                    HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
                    HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(rightTree));
                    return View("_ProductsList", rightTree);

                case "PartNumberInput":
                    var responceAllegroNumberMode = GetDetailsFromTree(null, null, null, rightTree.PartNumberValue, stateEnum, sortingEnum).Result;
                    rightTree.AllegroOfferList = responceAllegroNumberMode.Products;
                    rightTree.OfferState = stateEnum;
                    rightTree.OfferSorting = sortingEnum;
                    HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegroNumberMode.Products));
                    HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(rightTree));
                    return View("_ProductsList", rightTree);


                case "ShowMainSubChildsCategories":
                    var responceAllegroSubCategories = GetDetailsFromTree(rightTree.ManufacturerSelected, rightTree.ModelSelected, rightTree.SubCategoryId, null, stateEnum, sortingEnum).Result;
                    rightTree.AllegroOfferList = responceAllegroSubCategories.Products;
                    rightTree.OfferState = stateEnum;
                    rightTree.OfferSorting = sortingEnum;
                    HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegroSubCategories.Products));
                    HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(rightTree));
                    return View("_ProductsList", rightTree);
            }


            return View("_ProductsList", rightTree);
        }
        private OfferStateEnum GetStateEnumValue(string state)
        {
            switch (state)
            {
                case "All":
                    return OfferStateEnum.All;
                case "New":
                    return OfferStateEnum.New;
                case "Used":
                    return OfferStateEnum.Used;
                case "Recovered":
                    return OfferStateEnum.Recovered;
                default:
                    return OfferStateEnum.All;
            }
        }

        private OfferSortingEnum GetSortingEnumValue(string sorting)
        {
            switch (sorting)
            {
                case "Relevance":
                    return OfferSortingEnum.Relevance;
                case "PriceDesc":
                    return OfferSortingEnum.PriceDesc;
                case "PriceAsc":
                    return OfferSortingEnum.PriceAsc;
                default:
                    return OfferSortingEnum.Relevance;
            }
        }
    }
}
