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

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
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
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
            return View(new RightTreeViewModel() { ManufacturerList = carTree });
        }
        public IActionResult ShowMainView()
        {
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
            return View("_CarTree", new RightTreeViewModel() { ManufacturerList = carTree });
        }
        public IActionResult SelectManufactureAndModel(string carManufactureName)
        {
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
            var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault();

            return View("_CarModels", new RightTreeViewModel() { ManufacturerSelected = carManufactureName, ModelsList= modelTree.Select(x => x.Name) });
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
                ManufacturerSelected=carManufactureName,
                ModelSelected=carModel,
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
                return View("_ProductsList", new RightTreeViewModel()
                {
                    ManufacturerSelected = carManufactureName,
                    ModelSelected = carModel,
                    MainCategoryId = mainCategoryId,
                    MainCategoryName = mainCategoryName,
                    SubCategoryId = subCategoryId,
                    SubCategoryName = subCategoryName,
                    FunctionReturnFromProducts = String.Format("selectMainCategory('{0}', '{1}', '{2}', '{3}')", carManufactureName, carModel, mainCategoryId, mainCategoryName),
                    AllegroOfferList=responceAllegro.Products
                });
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
                AllegroOfferList = responceAllegro.Products
            };
            
            return View("_ProductsList", treeView);
        }
        // ============Block for left part of site
        public IActionResult PartNumberInput(string partNumber)
        {
            var responceAllegro = GetDetailsFromTree(null, null, null, partNumber).Result;
            RightTreeViewModel treeView = new RightTreeViewModel()
            {
                AllegroOfferList = responceAllegro.Products,
                FunctionReturnFromProducts = "back()"
            };
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
        public async Task<EkKioskProductSearchInEuropeGetResponse> GetDetailsFromTree(string carManufactureName, string carModel, string selectedCategoryId, string inputPartNumber)
        {
            SearchOffersResponse searchOffersResponse;
            if (inputPartNumber == null)
            {
                searchOffersResponse = await _allegroPlClient.SearchOffersAsync(String.Format("{0} {1}", carManufactureName, carModel), null, selectedCategoryId, KioskBrains.Clients.AllegroPl.Models.OfferStateEnum.All, KioskBrains.Clients.AllegroPl.Models.OfferSortingEnum.Relevance, 0, 10, System.Threading.CancellationToken.None);
            }
            else {
                searchOffersResponse = await _allegroPlClient.SearchOffersAsync(inputPartNumber, inputPartNumber, "3", KioskBrains.Clients.AllegroPl.Models.OfferStateEnum.All, KioskBrains.Clients.AllegroPl.Models.OfferSortingEnum.Relevance, 0, 10, System.Threading.CancellationToken.None);
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
