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
using System.Text.Json;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Clients.Ek4Car.Models;
using CustomCartProduct = WebApplication.Classes.CartProduct;

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
        public IList<CustomCartProduct> AddToCartSession(CustomCartProduct cartItem)
        {
            var cartListJson = HttpContext.Session.GetString("cartList");
            IList<CustomCartProduct> cartList = new List<CustomCartProduct>();
            if (cartListJson != null)
            {
                cartList = JsonSerializer.Deserialize<IList<WebApplication.Classes.CartProduct>>(cartListJson);
            }

            cartList.Add(cartItem);

            HttpContext.Session.SetString("cartList", JsonSerializer.Serialize(cartList));

            return cartList;
        }
        public IList<CustomCartProduct> GetCartProducts()
        {
            var cartListJson = HttpContext.Session.GetString("cartList");
            IList<WebApplication.Classes.CartProduct> cartList = new List<WebApplication.Classes.CartProduct>();
            if (cartListJson != null)
            {
                cartList = JsonSerializer.Deserialize<IList<WebApplication.Classes.CartProduct>>(cartListJson);
            }
            return cartList;
        }
        public ActionResult EditCartItemQuantity(string cartItemId, string quantity)
        {   
            IList<CustomCartProduct> cartList = GetCartProducts();
            cartList.Where(w => w.Product.SourceId == cartItemId).ToList().ForEach(s => s.Quantity = Convert.ToInt32(quantity));

            decimal tempAllPrice = 0;
            foreach (var item in cartList)
            {
                tempAllPrice += item.Product.Price * item.Quantity;
            }
            HttpContext.Session.SetString("cartList", JsonSerializer.Serialize(cartList));
            HttpContext.Session.SetString("cartWidgetPrice", tempAllPrice.ToString());
            ViewData["CartWidgetPrice"] = tempAllPrice;
            return View("CartViewInner", cartList);
        }
        public ActionResult DeleteProductFromCart(string cartItemId) {
            IList<CustomCartProduct> cartList123 = GetCartProducts();

            cartList123.Remove(cartList123.Where(x => x.Product.SourceId == cartItemId).FirstOrDefault());
            HttpContext.Session.SetString("cartList", JsonSerializer.Serialize(cartList123));
            if (cartList123.Count > 0)
            {
                decimal tempAllPrice = 0;
                foreach (var item in cartList123)
                {
                    tempAllPrice += item.Product.Price * item.Quantity;
                }
                HttpContext.Session.SetString("cartWidgetPrice", tempAllPrice.ToString());
                ViewData["CartWidgetPrice"] = tempAllPrice;
                new CartWidgetController().CartWidget();
            }
            else {
                HttpContext.Session.Remove("cartWidgetPrice");
                ViewData["CartWidgetPrice"] = null;
                new CartWidgetController().CartWidget();
            }
            HttpContext.Session.SetString("cartList", JsonSerializer.Serialize(cartList123));
            return View("CartViewInner", cartList123);

            
        }

        public ActionResult CartView(string selectedProductId=null)
        {
            var rightTreeViewModelString = HttpContext.Session.GetString("rightTreeViewModel");
            RightTreeViewModel rightTree = JsonSerializer.Deserialize<RightTreeViewModel>(rightTreeViewModelString);

            //set price to CartWidget in left panel
            ViewData["CartWidgetPrice"] = HttpContext.Session.GetString("cartWidgetPrice");

            if (String.IsNullOrEmpty(selectedProductId) && String.IsNullOrEmpty(rightTree.PartNumberValue))
            {
                return View(GetCartProducts());
            }
            //======================= START -- RETURN TO LIST PARAMS ----------- =================
            ViewBag.RequestParams = "?carManufactureName=" + rightTree.ManufacturerSelected + "&carModel=" + rightTree.ModelSelected + "&mainCategoryId=" + rightTree.MainCategoryId +"&mainCategoryName=" + rightTree.MainCategoryName +
                "&subCategoryId=" + rightTree.SubCategoryId + "&subCategoryName=" + rightTree.SubCategoryName + "&subChildId=" + rightTree.SubChildCategoryId + "&subChildName=" + rightTree.SubChildCategoryName;

            if (!String.IsNullOrEmpty(rightTree.PartNumberValue))
            {
                ViewBag.RequestParams = "?partNumber=" + rightTree.PartNumberValue;
            }
            //======================= END -- RETURN TO LIST PARAMS ----------- =================

            var productList = HttpContext.Session.GetString("productList");
            EkProduct[] list10Products = JsonSerializer.Deserialize<EkProduct[]>(productList);
            EkProduct productTemp = list10Products.Where(x => x.SourceId == selectedProductId).FirstOrDefault();

            CustomCartProduct cartProduct = new CustomCartProduct() { 
                 Product=productTemp,
                 Quantity=1
            };

            IList<CustomCartProduct> cartList = AddToCartSession(cartProduct);
            decimal tempAllPrice=0;
            foreach (var item in cartList)
            {
                tempAllPrice += item.Product.Price * item.Quantity;
            }


            HttpContext.Session.SetString("cartWidgetPrice", tempAllPrice.ToString());
            ViewData["CartWidgetPrice"] = tempAllPrice;
            CartWidgetController cartWidget = new CartWidgetController();
            cartWidget.CartWidget();
            return View(cartList);

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
            IList<WebApplication.Classes.CartProduct> cartList = new List<WebApplication.Classes.CartProduct>();
            if (cartListJson != null)
            {
                cartList = JsonSerializer.Deserialize<IList<WebApplication.Classes.CartProduct>>(cartListJson);
            }


            var transactionProducts = cartList
                .Select(x => EkTransactionProduct.FromProduct(x.Product, x.Product.Description, x.Quantity))
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
            //_dbContext.EkTransactions.Add(ekTransaction);
            //await _dbContext.SaveChangesAsync();
            
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
