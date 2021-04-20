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
using WebApplication.Classes;
using System.Security.Cryptography;
using KioskBrains.Clients.Ek4Car;
using ScraperApi;
namespace WebApplication.Controllers
{
    public class ProductController : Controller
    {
        private KioskBrains.Server.Domain.Entities.EK.EkTransaction _transaction;
        private EkProduct ekProduct;
        private AllegroPlClient _allegroPlClient;
        private NovaPoshtaUkraineClient _novaPoshtaClient;
        private ITranslateService _translateService;
        private Ek4CarClient _ek4CarClient;
        private readonly CentralBankExchangeRateManager _centralBankExchangeRateManager;
        private ILogger<AllegroPlClient> _logger;
        private IOptions<AllegroPlClientSettings> _settings;
        private IOptions<YandexTranslateClientSettings> _yandexSettings;
        private readonly KioskBrainsContext _dbContext;
        public ProductController(ILogger<AllegroPlClient> logger,
            IOptions<AllegroPlClientSettings> settings,
            IOptions<YandexTranslateClientSettings> yandexApiClientSettings,
            ITranslateService translateService, CentralBankExchangeRateManager centralBankExchangeRateManager, NovaPoshtaUkraineClient novaPoshtaUkraineClient, KioskBrainsContext dbContext, Ek4CarClient ek4CarClient)
        {
            _ek4CarClient = ek4CarClient;
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
        private string _paymentPublicKey = "i48356791714";
        private string _paymentPrivate_key = "nNgO8jvGMxbqO8EGpithJb5WeiCNI1IFT3VW5U7s";


        //private string _paymentPublicKey="i93808059847";
        //private string _paymentPrivate_key = "YS1ku34S1ixt8FqyxSXCdegYKfhdtvsO7TVp0Qnm";
        public ActionResult Index()
        {
            
            return View();
        }
       
        public PaymentLinkData GetPaymentLinkData(string paymentSettings) {
            string data = Convert.ToBase64String(Encoding.UTF8.GetBytes(paymentSettings));
            string sign_string = _paymentPrivate_key + data + _paymentPrivate_key;
            string signature = Hash(Encoding.UTF8.GetBytes(sign_string));
            return new PaymentLinkData() { Data = data, Signature = signature };
        }
        [HttpPost]
        public ActionResult Order(string CustomerName, string CustomerSurName, string CustomerFatherName, string CustomerPhoneNumber, string SelectedCity, string SelectedDepartment, string City, string Address)
        {
            string customerFullName = String.Format("{0} {1} {2}", CustomerSurName, CustomerName, CustomerFatherName);
            var ordered = MakeOrder(customerFullName, CustomerPhoneNumber, SelectedCity, SelectedDepartment, City, Address).Result;
            Ek4CarClient.Ek4CarResponse response = SendRequstToEk4Car(ordered).Result;
            EkTransactionProduct[] tempCartProducts = JsonSerializer.Deserialize<EkTransactionProduct[]>(ordered.ProductsJson);
            string payProducts = String.Format("№ замовлення {0}\n Оплата за товар:\n", response.data);
            foreach (var item in tempCartProducts) {
                payProducts += String.Format("{0} ({1} шт) - {2}{3}", item.name["ru"], item.quantity, item.price, item.priceCurrencyCode) + "\n";
            }

            OrderPaymentSettings privat24Settings = new OrderPaymentSettings(_paymentPublicKey, "3", "pay", ordered.TotalPrice.ToString(), "UAH", payProducts, response.data, "uk", "privat24", "https://api.ek4car.com/payment/callback");
            string privat24Json = JsonSerializer.Serialize(privat24Settings);
            PaymentLinkData privat24DataLink = GetPaymentLinkData(privat24Json);

            OrderPaymentSettings QRSettings = new OrderPaymentSettings(_paymentPublicKey, "3", "pay", ordered.TotalPrice.ToString(), "UAH", payProducts, response.data, "uk", "qr", "https://api.ek4car.com/payment/callback");
            string QRJson = JsonSerializer.Serialize(QRSettings);
            PaymentLinkData QRDataLink = GetPaymentLinkData(QRJson);

            OrderPaymentSettings APaySettings = new OrderPaymentSettings(_paymentPublicKey, "3", "pay", ordered.TotalPrice.ToString(), "UAH", payProducts, response.data, "uk", "apay", "https://api.ek4car.com/payment/callback");
            string APayJson = JsonSerializer.Serialize(APaySettings);
            PaymentLinkData APayDataLink = GetPaymentLinkData(APayJson);

            OrderPaymentSettings GPaySettings = new OrderPaymentSettings(_paymentPublicKey, "3", "pay", ordered.TotalPrice.ToString(), "UAH", payProducts, response.data, "uk", "gpay", "https://api.ek4car.com/payment/callback");
            string GPayJson = JsonSerializer.Serialize(GPaySettings);
            PaymentLinkData GPayDataLink = GetPaymentLinkData(GPayJson);

            OrderPaymentSettings cardSettings = new OrderPaymentSettings(_paymentPublicKey, "3", "pay", ordered.TotalPrice.ToString(), "UAH", payProducts, response.data, "uk", "card", "https://api.ek4car.com/payment/callback");
            string cardJson = JsonSerializer.Serialize(cardSettings);
            PaymentLinkData cardDataLink = GetPaymentLinkData(cardJson);

            OrderPaymentSettings liqpaySettings = new OrderPaymentSettings(_paymentPublicKey, "3", "pay", ordered.TotalPrice.ToString(), "UAH", payProducts, response.data, "uk", "liqpay", "https://api.ek4car.com/payment/callback");
            string liqpayJson = JsonSerializer.Serialize(liqpaySettings);
            PaymentLinkData liqpayDataLink = GetPaymentLinkData(liqpayJson);

            OrderPaymentSettings masterpassSettings = new OrderPaymentSettings(_paymentPublicKey, "3", "pay", ordered.TotalPrice.ToString(), "UAH", payProducts, response.data, "uk", "masterpass", "https://api.ek4car.com/payment/callback");
            string masterpassJson = JsonSerializer.Serialize(masterpassSettings);
            PaymentLinkData masterpassDataLink = GetPaymentLinkData(masterpassJson);

            OrderPaymentSettings moment_partSettings = new OrderPaymentSettings(_paymentPublicKey, "3", "pay", ordered.TotalPrice.ToString(), "UAH", payProducts, response.data, "uk", "moment_part", "https://api.ek4car.com/payment/callback");
            string moment_partSettingsJson = JsonSerializer.Serialize(moment_partSettings);
            PaymentLinkData moment_partDataLink = GetPaymentLinkData(moment_partSettingsJson);

            OrderPaymentSettings cashSettings = new OrderPaymentSettings(_paymentPublicKey, "3", "pay", ordered.TotalPrice.ToString(), "UAH", payProducts, response.data, "uk", "cash", "https://api.ek4car.com/payment/callback");
            string cashJson = JsonSerializer.Serialize(cashSettings);
            PaymentLinkData cashDataLink = GetPaymentLinkData(cashJson);

            OrderPaymentSettings invoiceSettings = new OrderPaymentSettings(_paymentPublicKey, "3", "pay", ordered.TotalPrice.ToString(), "UAH", payProducts, response.data, "uk", " invoice", "https://api.ek4car.com/payment/callback");
            string invoiceJson = JsonSerializer.Serialize(invoiceSettings);
            PaymentLinkData invoiceDataLink = GetPaymentLinkData(invoiceJson);

            PaymentViewModel paymentModel = new PaymentViewModel()
            {
                Privat24Data = privat24DataLink,
                QRData = QRDataLink,
                APayData = APayDataLink,
                GPayData = GPayDataLink,
                cardData = cardDataLink,
                liqpayData =liqpayDataLink,
                masterpassData=masterpassDataLink,
                moment_partData= moment_partDataLink,
                cashData= cashDataLink,
                invoiceData=invoiceDataLink
            };

            //ClearAllSessions();
            return View(paymentModel);
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
        
       
        private async Task<KioskBrains.Server.Domain.Entities.EK.EkTransaction> MakeOrder(string customerFullUserName, string customerPhoneNumber, string selectedCity=null, string selectedDepartment = null, string inputCity = null, string inputStreet = null) {
            KioskBrains.Common.EK.Transactions.EkTransaction eKTransactions = new KioskBrains.Common.EK.Transactions.EkTransaction();

            eKTransactions.SetCustomerInfo(new EkCustomerInfo()
            {
                fullName = customerFullUserName,
                phone = customerPhoneNumber,
            });
            var ekDeliveryInfo = new EkDeliveryInfo
            {
                type = String.IsNullOrEmpty(inputCity) ? EkDeliveryTypeEnum.DeliveryServiceStore : EkDeliveryTypeEnum.Courier,
                deliveryService = null,// String.IsNullOrEmpty(inputCity) ? EkDeliveryServiceEnum.NovaPoshtaUkraine : null,
                storeId = "1",
                address = new EkTransactionAddress()
                {
                    city = String.IsNullOrEmpty(inputCity) ? selectedCity : inputCity,
                    addressLine1 = String.IsNullOrEmpty(inputCity) ? selectedDepartment : inputStreet,
                }                
            };
            if (String.IsNullOrEmpty(inputCity))
            {
                ekDeliveryInfo.deliveryService = EkDeliveryServiceEnum.NovaPoshtaUkraine;
            }

            eKTransactions.SetDeliveryInfo(ekDeliveryInfo);

            var cartListJson = HttpContext.Session.GetString("cartList");
            IList<WebApplication.Classes.CartProduct> cartList = new List<WebApplication.Classes.CartProduct>();
            if (cartListJson != null)
            {
                cartList = JsonSerializer.Deserialize<IList<WebApplication.Classes.CartProduct>>(cartListJson);
            }

            var transactionProducts = cartList
                .Select(x => EkTransactionProduct.FromProduct(x.Product, x.Product.Description, x.Quantity))
                .ToArray();

            foreach (var item in cartList)
            {
                eKTransactions.TotalPrice += item.Product.Price * item.Quantity;
            }

            eKTransactions.TotalPriceCurrencyCode = "UAH";
            eKTransactions.SetProducts(transactionProducts);
            eKTransactions.ReceiptNumber = $"{ HttpContext.Session.GetString("kioskId")}-{DateTime.Now:yyyyMMddHHmm}" ;
            eKTransactions.CompletionStatus = KioskBrains.Common.Transactions.TransactionCompletionStatusEnum.Success;
            eKTransactions.Status = KioskBrains.Common.Transactions.TransactionStatusEnum.Completed;
            eKTransactions.UniqueId = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
            eKTransactions.LocalStartedOn = DateTime.Now;
            var serialize = Newtonsoft.Json.JsonConvert.SerializeObject(eKTransactions);           
            var apiEkTransaction = JsonSerializer.Deserialize<KioskBrains.Common.EK.Transactions.EkTransaction>(serialize);
            var ekTransaction = KioskBrains.Server.Domain.Entities.EK.EkTransaction.FromApiModel(Convert.ToInt32(HttpContext.Session.GetString("kioskId")), DateTime.Now, apiEkTransaction);
            ekTransaction.IsSentToEkSystem = true;
            HttpContext.Session.SetString("transactionTotalPrice", ekTransaction.TotalPrice.ToString());
            HttpContext.Session.SetString("transactionUserName", customerFullUserName);
            _dbContext.EkTransactions.Add(ekTransaction);
            await _dbContext.SaveChangesAsync();
           
            return ekTransaction;
        }
        private async Task<Ek4CarClient.Ek4CarResponse> SendRequstToEk4Car(KioskBrains.Server.Domain.Entities.EK.EkTransaction transac) {
            Order order = KioskBrains.Server.EK.Integration.Jobs.EkTransactionExtensions.ToEkOrder(transac);
            order.website = true;
             Ek4CarClient.Ek4CarResponse ek4CarResponse = await _ek4CarClient.SendOrderAsyncTest(order, CancellationToken.None);
            return ek4CarResponse;
        }
        public ActionResult Delivery(string area, string city)
        {
            IList<CustomCartProduct> cartProducts = GetCartProducts();

            ViewData["CartWidgetPrice"] = HttpContext.Session.GetString("cartWidgetPrice");
            var allData = _novaPoshtaClient.GetDataFromFile();
            var areas = allData.Select(x => new AreasSearchItem() { Description = x.AreaDescription, Ref = x.Ref }).GroupBy(x => x.Description).Select(g => g.First()).ToArray();
            NovaPoshtaViewModel npView = new NovaPoshtaViewModel
            {
                Areas = areas,
                Cities = string.IsNullOrEmpty(area) ? new List<WarehouseSearchItem>() : allData.Where(x => x.AreaDescription == area).ToList(),
                Departments = string.IsNullOrEmpty(city) ? new WarehouseSearchItem[0] : GetAllNovaPoshtaCityDepts(city).Result,
                CartProducts = cartProducts
            };
            return area == null && city == null ? (ActionResult)View(npView) : Json(npView);
        }
        public void ClearAllSessions() {
            HttpContext.Session.Remove("cartWidgetPrice");
            HttpContext.Session.Remove("cartList");
            HttpContext.Session.Remove("rightTreeViewModel");
            HttpContext.Session.Remove("productList");
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
        public ActionResult Details(string id, string price)
        {
            ProductViewModel model = GetProductInfo(id);
            model.Price = price;
            var rightTreeViewModelString = HttpContext.Session.GetString("rightTreeViewModel");
            RightTreeViewModel rightTree = JsonSerializer.Deserialize<RightTreeViewModel>(rightTreeViewModelString);
            model.ReturnFunction = rightTree.FunctionReturnFromProducts;
            return View("Details", model);
        }
        public ProductViewModel GetProductInfo(string id)
        {
            var p = _allegroPlClient.GetOfferById(_translateService, id, CancellationToken.None).Result;

            List<string> test = new List<string>();
            foreach (var item in p.Parameters)
            {
                test.Add(item.Name[Languages.RussianCode] + ": " + item.Value[Languages.RussianCode]);
            }
            

            var product = new ProductViewModel()
            {
                Id = id,
                Title = p.Name[Languages.RussianCode],
                Description = p.Description[Languages.RussianCode],
                Images = p.Images,
                Parameters = test
            };
            return product;
        }
        // GET: ProductController/Create
        public ActionResult Create()
        {
            return View();
        }

        public string Hash(byte[] temp)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(temp);
                return Convert.ToBase64String(hash);
            }
        }        
    }
}
