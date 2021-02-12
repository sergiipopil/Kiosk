using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Clients.AllegroPl.Rest;
using KioskBrains.Common.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
using KioskBrains.Clients.AllegroPl;
using KioskBrains.Clients.AllegroPl.ServiceInterfaces;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using KioskBrains.Clients.YandexTranslate;
using KioskBrains.Server.Domain.Actions.EkKiosk;
using KioskBrains.Common.EK.Api;
using KioskBrains.Server.Domain.Managers;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Config;
using WebApplication.NovaPoshtaUkraine;
using KioskBrains.Server.Domain.Helpers.Dates;
using WebApplication.NovaPoshtaUkraine.Models;
using KioskBrains.Common.EK.Transactions;

namespace WebApplication.Controllers
{
    public class ProductController : Controller
    {
        private EkTransaction _transaction;
        private EkProduct ekProduct;
        private AllegroPlClient _allegroPlClient;
        private NovaPoshtaUkraineClient _novaPoshtaClient;
        private ITranslateService _translateService;

        private readonly CentralBankExchangeRateManager _centralBankExchangeRateManager;
        private ILogger<AllegroPlClient> _logger;
        private IOptions<AllegroPlClientSettings> _settings;
        private IOptions<YandexTranslateClientSettings> _yandexSettings;
        private CancellationTokenSource _tokenSource;

        public ProductController(ILogger<AllegroPlClient> logger,
            IOptions<AllegroPlClientSettings> settings,
            IOptions<YandexTranslateClientSettings> yandexApiClientSettings,
            ITranslateService translateService, CentralBankExchangeRateManager centralBankExchangeRateManager, NovaPoshtaUkraineClient novaPoshtaUkraineClient)
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
            _novaPoshtaClient = novaPoshtaUkraineClient;
        }
        // GET: ProductController
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CartView(EkProduct data)
        {
            return View(data);
        }        

        [HttpPost]
        public ActionResult Submit(string selectedDepartment)
        {
            //_transaction = TransactionManager.Current.StartNewTransaction<EkTransaction>();
            //_transaction.TotalPriceCurrencyCode = "UAH";
            //_transaction.SetCustomerInfo(new EkCustomerInfo()
            //{
            //    FullName = "Sergii",
            //    Phone = "380678040010",
            //});
            return View(selectedDepartment);
        }

        public ActionResult Delivery(string area, string city)
        {            
            var allData = _novaPoshtaClient.GetDataFromFile();
            //var areas = GetAllNovaPoshtaAreas();
            var areas = allData.Select(x => new AreasSearchItem() { Description = x.AreaDescription, Ref = x.Ref }).GroupBy(x=>x.Description).Select(g=>g.First()).ToArray();
            NovaPoshtaViewModel npView = new NovaPoshtaViewModel
            {
                Areas = areas,
                Cities = string.IsNullOrEmpty(area) ? new List<WarehouseSearchItem>() : allData.Where(x => x.AreaDescription == area).ToList(),
                Departments = string.IsNullOrEmpty(city) ? new WarehouseSearchItem[0] : GetAllNovaPoshtaCityDepts(city).Result
            };
            return area==null && city==null ? (ActionResult)View(npView) : Json(npView);
        }

        public ActionResult Cities(string area)
        {
            var allData = _novaPoshtaClient.GetDataFromFile();

            var areas = allData.Select(x => new AreasSearchItem() { Description = x.AreaDescription, Ref = x.Ref }).GroupBy(x => x.Description).Select(g => g.First()).ToArray();
            NovaPoshtaViewModel npView = new NovaPoshtaViewModel
            {
                Areas = areas,
                Cities = allData.Where(x => x.AreaDescription == area).ToList(),
                Departments = allData.ToArray()
            };
            return Json(npView);
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
        
        private async Task<WarehouseSearchItem[]> GetAllNovaPoshtaCityDepts(string city)
        {
            var result = await _novaPoshtaClient.GetAllDepartmentsOfTheCity(CancellationToken.None, city);
            return result;
        }
        
        public ProductViewModel GetProductInfo(string id)
        {
            var p = _allegroPlClient.GetOfferById(_translateService, id, CancellationToken.None).Result;

            List<string> test = new List<string>();
            foreach (var item in p.Parameters)
            {
                test.Add(item.Name[Languages.RussianCode] + ": " + item.Value[Languages.RussianCode]);
            }

            var rate = GetExchangeRateAsync().Result;
            ekProduct = EkConvertHelper.EkAllegroPlOfferToProduct(p, rate);


            var product = new ProductViewModel()
            {
                Id = id,
                Title = p.Name[Languages.RussianCode],
                Description = p.Description[Languages.RussianCode],
                Images = p.Images,
                Parameters = test,
                Price = ekProduct.Price
            };
            return product;
        }
        // GET: ProductController/Details/5        
        public ActionResult Details(string id)
        {
            return View(GetProductInfo(id));
        }

        // GET: ProductController/Create
        public ActionResult Create()
        {
            return View();
        }
        
    }
}
