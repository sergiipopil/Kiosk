using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using WebApplication.NovaPoshtaUkraine;
using KioskBrains.Server.Domain.Helpers.Dates;
using WebApplication.NovaPoshtaUkraine.Models;
using System.Net.Http;
using KioskBrains.Common.EK.Transactions;
using KioskBrains.Clients.AllegroPl.Models;
using System.Text.Json;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Clients.Ek4Car.Models;
using KioskBrains.Server.EK.Integration.Jobs;
using KioskBrains.Clients.Ek4Car;

namespace WebApplication.Controllers
{
    public class ProductController : Controller
    {
        private KioskBrains.Server.Domain.Entities.EK.EkTransaction _transaction;
        private EkProduct ekProduct;
        private AllegroPlClient _allegroPlClient;
        private NovaPoshtaUkraineClient _novaPoshtaClient;
        private ITranslateService _translateService;

        private readonly CentralBankExchangeRateManager _centralBankExchangeRateManager;
        private ILogger<AllegroPlClient> _logger;
        private IOptions<AllegroPlClientSettings> _settings;
        private IOptions<YandexTranslateClientSettings> _yandexSettings;
        private readonly KioskBrainsContext _dbContext;
        public ProductController(ILogger<AllegroPlClient> logger,
            IOptions<AllegroPlClientSettings> settings,
            IOptions<YandexTranslateClientSettings> yandexApiClientSettings,
            ITranslateService translateService, CentralBankExchangeRateManager centralBankExchangeRateManager, NovaPoshtaUkraineClient novaPoshtaUkraineClient, KioskBrainsContext dbContext)
        {
            _dbContext = dbContext;
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
        public IList<EkProduct> AddToCartSession(EkProduct product)
        {
            var cartListJson = HttpContext.Session.GetString("cartList");
            IList<EkProduct> cartList = new List<EkProduct>();
            if (cartListJson != null)
            {
                cartList = JsonSerializer.Deserialize<IList<EkProduct>>(cartListJson);
            }

            cartList.Add(product);

            HttpContext.Session.SetString("cartList", JsonSerializer.Serialize(cartList));

            return cartList;
        }
        public IList<EkProduct> GetCartProducts()
        {
            var cartListJson = HttpContext.Session.GetString("cartList");
            IList<EkProduct> cartList = new List<EkProduct>();
            if (cartListJson != null)
            {
                cartList = JsonSerializer.Deserialize<IList<EkProduct>>(cartListJson);
            }
            return cartList;
        }
        public ActionResult EditCartItemQuantity(string cartItemId, string quantity)
        {   
            IList<EkProduct> cartList = GetCartProducts();
            decimal totalPrice = cartList.Where(x => x.SourceId == cartItemId).Select(x => x.Price).FirstOrDefault() * Convert.ToInt32(quantity);
            return Json(totalPrice);
        }
        public ActionResult DeleteProductFromCart(string cartItemId) {
            IList<EkProduct> cartList123 = GetCartProducts();

            cartList123.Remove(cartList123.Where(x => x.SourceId == cartItemId).FirstOrDefault());
            HttpContext.Session.SetString("cartList", JsonSerializer.Serialize(cartList123));
            return View("CartViewInner", cartList123);
        }

        public ActionResult CartView(string selectedProductId=null, string carManufactureName = null, string carModel = null, string mainCategoryId = null, string mainCategoryName = null, string subCategoryId = null, string subCategoryName = null, string subChildId = null, string subChildName = null, string partNumber = null)
        {
            ViewData["CartWidgetPrice"] = HttpContext.Session.GetString("cartWidgetPrice");
            if (String.IsNullOrEmpty(selectedProductId) && String.IsNullOrEmpty(partNumber))
            {
                return View(GetCartProducts());
            }
            ViewBag.RequestParams = "?carManufactureName=" + carManufactureName + "&carModel=" + carModel + "&mainCategoryId=" + mainCategoryId + "&mainCategoryName=" + mainCategoryName +
                "&subCategoryId=" + subCategoryId + "&subCategoryName=" + subCategoryName + "&subChildId=" + subChildId + "&subChildName=" + subChildName;
            if (!String.IsNullOrEmpty(partNumber))
            {
                ViewBag.RequestParams = "?partNumber=" + partNumber;
            }
            var productList = HttpContext.Session.GetString("productList");

            EkProduct[] list10Products = JsonSerializer.Deserialize<EkProduct[]>(productList);
            EkProduct cartProduct = list10Products.Where(x => x.SourceId == selectedProductId).FirstOrDefault();
            IList<EkProduct> cartList = AddToCartSession(cartProduct);
            var totalCartPrice = cartList.Select(x => x.Price).Sum();
            HttpContext.Session.SetString("cartWidgetPrice", totalCartPrice.ToString());
            ViewData["CartWidgetPrice"] = totalCartPrice;
            CartWidgetController cartWidget = new CartWidgetController();
            cartWidget.CartWidget();
            return View(cartList);

        }
        public class CartProduct
        {
            public Product Product { get; set; }
            public int Quantity { get; set; }
        }
        public IList<CartProduct> Products
        {
            get {
                return new List<CartProduct>();
            }
        }
        [HttpPost]
        public ActionResult Submit(string selectedDepartment)
        {
            var transac = TestInsertDB().Result;           
            return View(selectedDepartment);
        }

        
        
        private async Task<KioskBrains.Server.Domain.Entities.EK.EkTransaction> TestInsertDB() {
            KioskBrains.Common.EK.Transactions.EkTransaction tempTransactions = new KioskBrains.Common.EK.Transactions.EkTransaction();
            var productList = HttpContext.Session.GetString("productList");

            tempTransactions.SetCustomerInfo(new EkCustomerInfo()
            {
                FullName = "SITE ORDER",
                Phone = "+380678040010"//PhoneNumberHelper.GetCleanedPhoneNumber(_phoneNumberStepData.PhoneNumber),
            });
            var deliveryValueBuilder = new StringBuilder();
            deliveryValueBuilder.Append("Самовывоз, Новая Почта");
            var ekDeliveryInfo = new EkDeliveryInfo
            {
                Type = EkDeliveryTypeEnum.DeliveryServiceStore,//_deliveryInfoStepData.Type ?? EkDeliveryTypeEnum.Courier,
                DeliveryService = EkDeliveryServiceEnum.NovaPoshtaUkraine,//_deliveryInfoStepData.DeliveryService,
                StoreId = "1",
                Address = new EkTransactionAddress()
                {
                    City = "Дрогобич",
                    AddressLine1 = "Адреса в Дрогобичі",
                }
            };

            tempTransactions.SetDeliveryInfo(ekDeliveryInfo);

            tempTransactions.TotalPrice = 777;
            tempTransactions.TotalPriceCurrencyCode = "UAH";

            var cartListJson = HttpContext.Session.GetString("cartList");
            IList<EkProduct> cartList = new List<EkProduct>();
            if (cartListJson != null)
            {
                cartList = JsonSerializer.Deserialize<IList<EkProduct>>(cartListJson);
            }


            var cartProducts = Products
                .Select(x => new
                {
                    x.Product,
                    x.Quantity,
                })
                .ToArray();

            var transactionProducts = cartList
                .Select(x => EkTransactionProduct.FromProduct(x, x.Description, 1))
                .ToArray();

            tempTransactions.SetProducts(transactionProducts);
            tempTransactions.ReceiptNumber = "132-25";
            tempTransactions.CompletionStatus = KioskBrains.Common.Transactions.TransactionCompletionStatusEnum.Success;
            tempTransactions.Status = KioskBrains.Common.Transactions.TransactionStatusEnum.Completed;
            tempTransactions.UniqueId = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
            tempTransactions.LocalStartedOn = DateTime.Now;
            var serialize = Newtonsoft.Json.JsonConvert.SerializeObject(tempTransactions);
            var tempser = new StringContent(serialize, Encoding.UTF8, "application/json");
            var apiEkTransaction = JsonSerializer.Deserialize<KioskBrains.Common.EK.Transactions.EkTransaction>(serialize);
            var ekTransaction = KioskBrains.Server.Domain.Entities.EK.EkTransaction.FromApiModel(116, DateTime.Now, apiEkTransaction);
            ekTransaction.IsSentToEkSystem = false;
            _dbContext.EkTransactions.Add(ekTransaction);
            await _dbContext.SaveChangesAsync();
            
            return ekTransaction;
        }
        public ActionResult Delivery(string area, string city)
        {
            ViewData["CartWidgetPrice"] = HttpContext.Session.GetString("cartWidgetPrice");
            var allData = _novaPoshtaClient.GetDataFromFile();
            var areas = allData.Select(x => new AreasSearchItem() { Description = x.AreaDescription, Ref = x.Ref }).GroupBy(x => x.Description).Select(g => g.First()).ToArray();
            NovaPoshtaViewModel npView = new NovaPoshtaViewModel
            {
                Areas = areas,
                Cities = string.IsNullOrEmpty(area) ? new List<WarehouseSearchItem>() : allData.Where(x => x.AreaDescription == area).ToList(),
                Departments = string.IsNullOrEmpty(city) ? new WarehouseSearchItem[0] : GetAllNovaPoshtaCityDepts(city).Result
            };
            return area == null && city == null ? (ActionResult)View(npView) : Json(npView);
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
        private async Task<WarehouseSearchItem[]> GetAllNovaPoshtaCityDepts(string city)
        {
            var result = await _novaPoshtaClient.GetAllDepartmentsOfTheCity(CancellationToken.None, city);
            return result;
        }

        
        // GET: ProductController/Details/5        
        public ActionResult Details(string id)
        {
            return View(GetProductInfo(id));
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
        // GET: ProductController/Create
        public ActionResult Create()
        {
            return View();
        }

    }
}
