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
        public static int pageSize = 10;
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


        public IActionResult Index(string kioskId)
        {
            HttpContext.Session.Remove("topCategoryId");
            if (kioskId == null)
            {
                kioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            }
            HttpContext.Session.SetString("kioskId", kioskId == null ? "116" : kioskId);
            ViewData["CartWidgetPrice"] = HttpContext.Session.GetString("cartWidgetPrice");
            _topCategoryCarType = EkCarTypeEnum.Car;
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
            return View(new RightTreeViewModel() { ManufacturerList = carTree, kioskId = HttpContext.Session.GetString("kioskId") });
        }
        //====================METHOD TO SWITCH TYPE CAR TOP CATEGORY ======================================
        public IActionResult ShowMainView(string topCategoryId)
        {
            _topCategoryCarType = new EkSiteFactory().GetCarTypeEnum(topCategoryId == "undefined" || topCategoryId == null ? HttpContext.Session.GetString("topCategoryId") : topCategoryId);
            HttpContext.Session.SetString("topCategoryId", topCategoryId == null ? "620" : topCategoryId);
            if (topCategoryId == "99193" || topCategoryId == "18554")
            {
                var tempC = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == topCategoryId).FirstOrDefault().Children;
                return View("_AutoPartsTree", new RightTreeViewModel() { ProductCategoryList = tempC, ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") });
            }
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            return View("_CarTree", new RightTreeViewModel() { ManufacturerList = carTree, TopCategoryId = HttpContext.Session.GetString("topCategoryId"), kioskId = tempKioskId });
        }
        //====================METHOD TO SHOW CAR MODELS ======================================

        public IActionResult SelectManufactureAndModel(string carManufactureName)
        {
            if (String.IsNullOrEmpty(carManufactureName))
            {
                HttpContext.Session.Remove("topCategoryId");
                var mainTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
                return View("_CarTree", new RightTreeViewModel() { ManufacturerList = mainTree });
            }
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault();
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            return View("_CarModels", new RightTreeViewModel() { ManufacturerSelected = carManufactureName, ModelsList = modelTree, TopCategoryId = HttpContext.Session.GetString("topCategoryId"), kioskId = tempKioskId });
        }
        public IActionResult SelectModelModifications(string carManufactureName, string model)
        {
            if (String.IsNullOrEmpty(carManufactureName))
            {
                HttpContext.Session.Remove("topCategoryId");
                var mainTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
                return View("_CarTree", new RightTreeViewModel() { ManufacturerList = mainTree });
            }
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault().Where(x => x.Name == model);
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            return View("_CarModels", new RightTreeViewModel() { ManufacturerSelected = carManufactureName, ModelSelected = model, ModelsList = modelTree.Select(x => x.Children).FirstOrDefault(), IsModificationList = true, TopCategoryId = HttpContext.Session.GetString("topCategoryId"), kioskId = tempKioskId });
        }
        //====================METHOD TO SHOW PRODUCTS CATEGORIES ======================================
        public IActionResult ShowCategoryAutoParts(string carManufactureName, string carModel, string mainCategoryUrl)
        {
            string topCategoryId = String.IsNullOrEmpty(HttpContext.Session.GetString("topCategoryId")) || HttpContext.Session.GetString("topCategoryId") == "undefined" || HttpContext.Session.GetString("topCategoryId") == "621" || HttpContext.Session.GetString("topCategoryId") == "622" ? "620" : HttpContext.Session.GetString("topCategoryId");
            var autoParts = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == topCategoryId).FirstOrDefault().Children;
            string reallyTopCategory = "";
            foreach (var item in autoParts)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP_"))
                {
                    item.CategoryId = "GROUP_" + item.CategoryId;
                }
            }
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            if (HttpContext.Session.GetString("topCategoryId") == "621" || mainCategoryUrl.Contains("/Truck/")) {
                reallyTopCategory = "621";
            }
            return View("_AutoPartsTree", new RightTreeViewModel() { ProductCategoryList = autoParts, ManufacturerSelected = carManufactureName, ModelSelected = carModel, kioskId = tempKioskId, ReallyTopCategoryId = reallyTopCategory, MainCategoryUrl=mainCategoryUrl });
        }
        //====================METHOD TO SHOW PRODUCTS SUBCATEGORIES ======================================
        public IActionResult ShowMainSubcategories(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string mainCategoryUrl)
        {
            string topCategoryId = String.IsNullOrEmpty(HttpContext.Session.GetString("topCategoryId")) || HttpContext.Session.GetString("topCategoryId") == "undefined" || HttpContext.Session.GetString("topCategoryId") == "621" || HttpContext.Session.GetString("topCategoryId") == "622" ? "620" : HttpContext.Session.GetString("topCategoryId");
            var autoPartsSubCategories = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == topCategoryId).FirstOrDefault().Children.Where(x => x.CategoryId == mainCategoryId).Select(x => x.Children).FirstOrDefault();

            foreach (var item in autoPartsSubCategories)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP_"))
                {
                    item.CategoryId = "GROUP_" + item.CategoryId;
                }
            }
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            RightTreeViewModel treeView = new RightTreeViewModel()
            {
                ManufacturerSelected = carManufactureName,
                ModelSelected = carModel,
                ProductCategoryList = autoPartsSubCategories,
                MainCategoryId = mainCategoryId,
                MainCategoryName = mainCategoryName,
                kioskId = tempKioskId,
                MainCategoryUrl= mainCategoryUrl,
                ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" || mainCategoryUrl.Contains("/Truck/") ? "621" : HttpContext.Session.GetString("topCategoryId")
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
        public IActionResult ShowMainSubChildsCategories(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string subCategoryId, string subCategoryName, string mainCategoryUrl)
        {
            string topCategoryId = String.IsNullOrEmpty(HttpContext.Session.GetString("topCategoryId")) || HttpContext.Session.GetString("topCategoryId") == "undefined" || HttpContext.Session.GetString("topCategoryId") == "621" || HttpContext.Session.GetString("topCategoryId") == "622" ? "620" : HttpContext.Session.GetString("topCategoryId");
            var autoPartsSubChildCategories = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == topCategoryId).FirstOrDefault().Children.Where(x => x.CategoryId == mainCategoryId).Select(x => x.Children).FirstOrDefault();
            autoPartsSubChildCategories = autoPartsSubChildCategories.Where(x => x.CategoryId == subCategoryId).Select(x => x.Children).FirstOrDefault();
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            if (autoPartsSubChildCategories == null)
            {
                var responceAllegro = GetAllegroProducts(carManufactureName, carModel, subCategoryId, null, OfferStateEnum.Used, OfferSortingEnum.PriceAsc, 1, "", "", "", "", "", "", "", "").Result;
                HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
                RightTreeViewModel treeViewModel = new RightTreeViewModel()
                {
                    ManufacturerSelected = carManufactureName,
                    ModelSelected = carModel,
                    FilterName = GetFilterName(subCategoryId),
                    MainCategoryId = mainCategoryId,
                    MainCategoryName = mainCategoryName,
                    SubCategoryId = subCategoryId,
                    SubCategoryName = subCategoryName,
                    PageNumber = 1,
                    SelectedTiresSizes = new SelectedTires()
                    {
                        Height = null,
                        Width = null,
                        Quantity = null,
                        RSize = null
                    },
                    Tires = new TiresFilter()
                    {
                        Height = new TiresSizes().GetTiresHeight(),
                        Width = new TiresSizes().GetTiresWidth(),
                        RSize = new TiresSizes().GetTiresRSize(),
                        Quantity = new TiresSizes().GetTiresCnt()
                    },
                    MainCategoryUrl = mainCategoryUrl,
                    ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" || mainCategoryUrl.Contains("/Truck/") ? "621" : HttpContext.Session.GetString("topCategoryId"),
                    FunctionReturnFromProducts = String.Format("selectMainCategory('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", carManufactureName, carModel, mainCategoryId, mainCategoryName, tempKioskId, mainCategoryUrl),
                    AllegroOfferList = responceAllegro.Products,
                    FakeAllegroList = FakeListForPager(responceAllegro.Total),
                    ControllerName = "ShowMainSubChildsCategories",
                    OfferSorting = OfferSortingEnum.PriceAsc,
                    OfferState = OfferStateEnum.Used,
                    kioskId = tempKioskId
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
                kioskId = tempKioskId,
                ManufacturerSelected = carManufactureName,
                ModelSelected = carModel,
                MainCategoryId = mainCategoryId,
                MainCategoryName = mainCategoryName,
                SubCategoryId = subCategoryId,
                SubCategoryName = subCategoryName,
                ProductCategoryList = autoPartsSubChildCategories,
                MainCategoryUrl = mainCategoryUrl,
                ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" || mainCategoryUrl.Contains("/Truck/") ? "621" : HttpContext.Session.GetString("topCategoryId")
            };
            return View("_AutoPartsSubChildsTree", treeView);

        }
        //====================METHOD TO SHOW PRODUCTS(ENTERED IN LAST GROUP OF AUTOPARTS) ======================================
        public IActionResult ShowProductList(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string subCategoryId, string subCategoryName, string subChildId, string subChildName, string mainCategoryUrl)
        {
            var responceAllegro = GetAllegroProducts(carManufactureName, carModel, subChildId, null, OfferStateEnum.Used, OfferSortingEnum.PriceAsc, 1, "", "", "", "", "", "", "", "").Result;
            HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            RightTreeViewModel treeView = new RightTreeViewModel()
            {
                kioskId = tempKioskId,
                ManufacturerSelected = carManufactureName,
                ModelSelected = carModel,
                FilterName = GetFilterName(subChildId),
                MainCategoryId = mainCategoryId,
                MainCategoryName = mainCategoryName,
                SubCategoryId = subCategoryId,
                SubCategoryName = subCategoryName,
                SubChildCategoryId = subChildId,
                SubChildCategoryName = subChildName,
                PageNumber = 1,
                SelectedTiresSizes = new SelectedTires()
                {
                    Height = null,
                    Width = null,
                    Quantity = null,
                    RSize = null
                },
                Tires = new TiresFilter()
                {
                    Height = new TiresSizes().GetTiresHeight(),
                    Width = new TiresSizes().GetTiresWidth(),
                    RSize = new TiresSizes().GetTiresRSize(),
                    Quantity = new TiresSizes().GetTiresCnt()
                },
                MainCategoryUrl = mainCategoryUrl,
                ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" || mainCategoryUrl.Contains("/Truck/") ? "621" : HttpContext.Session.GetString("topCategoryId"),
                OfferState = OfferStateEnum.Used,
                FunctionReturnFromProducts = String.Format("selectSubMainCategory('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')", carManufactureName, carModel, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName, tempKioskId, mainCategoryUrl),
                AllegroOfferList = responceAllegro.Products,
                ControllerName = "ShowProductList",
                FakeAllegroList = FakeListForPager(responceAllegro.Total),
                OfferSorting = OfferSortingEnum.PriceAsc
            };
            HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeView));
            return View("_ProductsList", treeView);
        }
        //==================== METHOD TO GET ALLEGRO PRODUCTS BY INPUT VALUE  ======================================
        public IActionResult PartNumberInput(string partNumber)
        {
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            var responceAllegro = GetAllegroProducts(null, null, null, partNumber).Result;
            RightTreeViewModel treeView = new RightTreeViewModel()
            {
                kioskId = tempKioskId,
                AllegroOfferList = responceAllegro.Products,
                FakeAllegroList = FakeListForPager(responceAllegro.Total),
                FunctionReturnFromProducts = "back()",
                ControllerName = "PartNumberInput",
                PartNumberValue = partNumber,
                SelectedTiresSizes = new SelectedTires()
                {
                    Height = null,
                    Width = null,
                    Quantity = null,
                    RSize = null
                },
                Tires = new TiresFilter()
                {
                    Height = new TiresSizes().GetTiresHeight(),
                    Width = new TiresSizes().GetTiresWidth(),
                    RSize = new TiresSizes().GetTiresRSize(),
                    Quantity = new TiresSizes().GetTiresCnt()
                },
                OfferSorting = OfferSortingEnum.PriceAsc,
                OfferState = OfferStateEnum.Used,
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
        public string GetFilterName(string categoryId)
        {
            IList<string> placeLR = new List<string>() { "254604", "18713", "18712", "18711", "255119", "255099", "4136" };
            IList<string> placeFB = new List<string>() { "254699", "254703", "254705", "19063", "250885" };
            IList<string> placeAll = new List<string>() { "254580", "18708", "254683", "254685" };
            IList<string> engineType = new List<string>() { "312565","50825", "50828", "256143", "50831", "50829", "50830", "261131", "261131", "255442", "255441", "255440", "261150", "50824", "255451", "255447", "261133",
                "255446", "50838", "255450", "261148", "50839", "50840", "255629", "255448", "255445", "255449", "50837", "256158", "256141", "255520", "50862", "256205", "50841", "256227", "255519", "255521", "256142", "255456",
            "261135", "261093", "255458", "255457", "255459", "256144", "50847", "255480", "50845", "255479", "50844", "50846", "50848", "261137", "255481", "261129", "255478", "261149", "255477", "261136", "50843",
            "50854", "256159", "261138", "259309", "255507", "147922", "50855", "147924", "255506", "255508", "261140", "261139", "50856", "261142", "147923", "261130", "50853", "255517", "255512", "256160", "255514", "261141",
            "255509", "255511", "260944", "255513", "260943", "255515", "50835", "50834", "255443", "255444", "261132", "50833", "50822", "260924", "260923", "260926", "260927", "260928", "260929", "260925", "260911", "260910",
            "256016", "256012", "260912", "256011", "256013", "260898", "260913", "260907", "256014", "256017", "260899", "260900", "260914", "260915", "256015", "256010", "256018", "260909", "50872", "260901", "50868", "50867",
            "256006", "256001", "260903", "260904", "256004", "256003", "256008", "256002", "256007", "260906", "256005", "260908", "260905", "260902", "256009", "261091", "260934", "260937", "260935", "260938", "260939", "260940",
            "260941", "260936", "255925", "261094", "18856", "255917", "261200", "255918", "255935", "255933", "261083", "256226", "261092", "255929", "255922", "255931", "255932", "255924", "256204", "255923", "255927",
            "255920", "255934", "255926", "255930", "261089", "255928"};
            if (placeLR.Contains(categoryId))
            {
                return "placeLR";
            }
            if (placeFB.Contains(categoryId))
            {
                return "placeFB";
            }
            if (placeAll.Contains(categoryId))
            {
                return "placeAll";
            }
            if (engineType.Contains(categoryId))
            {
                return "engineType";
            }
            return "none";
        }
        //==================== METHOD FOR GET ALLEGRO PRODUCTS ======================================
        public async Task<EkKioskProductSearchInEuropeGetResponse> GetAllegroProducts(string carManufactureName, string carModel, string selectedCategoryId, string inputPartNumber, OfferStateEnum state = OfferStateEnum.Used, OfferSortingEnum sortingPrice = OfferSortingEnum.PriceAsc, int pageNumber = 1, string position = "", string isorigin = "", string enginetype = "", string transmissiontype = "", string tiresQuantity = "", string tiresWidth = "", string tiresHeight = "", string tiresRSize = "")
        {
            int offset = pageNumber == 1 ? 0 : pageNumber * 10;
            SearchOffersResponse searchOffersResponse;
            if (inputPartNumber == null)
            {
                searchOffersResponse = await _allegroPlClient.SearchOffersAsync(String.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", carManufactureName, carModel, position, isorigin, enginetype, transmissiontype, tiresQuantity, tiresWidth, tiresHeight, tiresRSize), null, selectedCategoryId, state, sortingPrice, offset, 10, System.Threading.CancellationToken.None, IsCategoryBody(selectedCategoryId));
            }
            else
            {
                searchOffersResponse = await _allegroPlClient.SearchOffersAsync(inputPartNumber, null, "3", state, sortingPrice, offset, 10, System.Threading.CancellationToken.None, IsCategoryBody(selectedCategoryId));
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
        public IActionResult FilteredList(string state, string sorting, int page, string position, string isorigin, string enginetype, string transmissionType, string tiresQuantity, string tiresWidth, string tiresHeight, string tiresRSize)
        {
            page = page == 0 ? 1 : page;
            var rightTreeViewModelString = HttpContext.Session.GetString("rightTreeViewModel");
            RightTreeViewModel rightTree = JsonSerializer.Deserialize<RightTreeViewModel>(rightTreeViewModelString);
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            OfferStateEnum stateEnum = new EkSiteFactory().GetStateEnumValue(state);
            OfferSortingEnum sortingEnum = new EkSiteFactory().GetSortingEnumValue(sorting);
            rightTree.kioskId = tempKioskId;
            switch (rightTree.ControllerName)
            {
                case "ShowProductList":
                    var responceAllegro = GetAllegroProducts(rightTree.ManufacturerSelected, rightTree.ModelSelected, rightTree.SubChildCategoryId, null, stateEnum, sortingEnum, page, position, isorigin, enginetype, transmissionType, tiresQuantity, tiresWidth, tiresHeight, tiresRSize).Result;
                    rightTree.FilterName = GetFilterName(rightTree.SubChildCategoryId);
                    rightTree.AllegroOfferList = responceAllegro.Products;
                    rightTree.FakeAllegroList = FakeListForPager(responceAllegro.Total);
                    rightTree.OfferState = stateEnum;
                    rightTree.OfferSorting = sortingEnum;
                    rightTree.PageNumber = page;
                    rightTree.OfferSortingPlacement = position;
                    rightTree.OfferSortingIsOrigin = isorigin;
                    rightTree.OfferSortingEngineType = enginetype;
                    rightTree.OfferSortingTransmissionType = transmissionType;
                    rightTree.SelectedTiresSizes.Quantity = tiresQuantity;
                    rightTree.SelectedTiresSizes.Width = tiresWidth;
                    rightTree.SelectedTiresSizes.Height = tiresHeight;
                    rightTree.SelectedTiresSizes.RSize = tiresRSize;
                    HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
                    HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(rightTree));
                    return View("_ProductsList", rightTree);

                case "PartNumberInput":
                    var responceAllegroNumberMode = GetAllegroProducts(null, null, null, rightTree.PartNumberValue, stateEnum, sortingEnum, page, position, isorigin, enginetype, transmissionType, tiresQuantity, tiresWidth, tiresHeight, tiresRSize).Result;
                    rightTree.AllegroOfferList = responceAllegroNumberMode.Products;
                    rightTree.FakeAllegroList = FakeListForPager(responceAllegroNumberMode.Total);
                    rightTree.OfferState = stateEnum;
                    rightTree.OfferSorting = sortingEnum;
                    rightTree.PageNumber = page;
                    rightTree.OfferSortingPlacement = position;
                    rightTree.OfferSortingIsOrigin = isorigin;
                    rightTree.OfferSortingEngineType = enginetype;
                    rightTree.OfferSortingTransmissionType = transmissionType;
                    rightTree.SelectedTiresSizes.Quantity = tiresQuantity;
                    rightTree.SelectedTiresSizes.Width = tiresWidth;
                    rightTree.SelectedTiresSizes.Height = tiresHeight;
                    rightTree.SelectedTiresSizes.RSize = tiresRSize;
                    HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegroNumberMode.Products));
                    HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(rightTree));
                    return View("_ProductsList", rightTree);


                case "ShowMainSubChildsCategories":
                    var responceAllegroSubCategories = GetAllegroProducts(rightTree.ManufacturerSelected, rightTree.ModelSelected, rightTree.SubCategoryId, null, stateEnum, sortingEnum, page, position, isorigin, enginetype, transmissionType, tiresQuantity, tiresWidth, tiresHeight, tiresRSize).Result;
                    rightTree.FilterName = GetFilterName(rightTree.SubCategoryId);
                    rightTree.AllegroOfferList = responceAllegroSubCategories.Products;
                    rightTree.FakeAllegroList = FakeListForPager(responceAllegroSubCategories.Total);
                    rightTree.OfferState = stateEnum;
                    rightTree.OfferSorting = sortingEnum;
                    rightTree.PageNumber = page;
                    rightTree.OfferSortingPlacement = position;
                    rightTree.OfferSortingIsOrigin = isorigin;
                    rightTree.OfferSortingEngineType = enginetype;
                    rightTree.OfferSortingTransmissionType = transmissionType;
                    rightTree.SelectedTiresSizes.Quantity = tiresQuantity;
                    rightTree.SelectedTiresSizes.Width = tiresWidth;
                    rightTree.SelectedTiresSizes.Height = tiresHeight;
                    rightTree.SelectedTiresSizes.RSize = tiresRSize;
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
        public bool IsCategoryBody(string categoryId) {
            IList<string> bodyCategories = new List<string>() { "250542", "254548", "254699", "254580", "250542", "254559", "261280", "254683", "254659", "261282", "254718" };
            return bodyCategories.Contains(categoryId);
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
