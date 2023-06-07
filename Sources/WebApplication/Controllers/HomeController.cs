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
using System.Text;
using System.Net.Http;
using Microsoft.Office.Interop.Excel;
using System.Data.OleDb;
using System.Web;
using System.IO;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Table;
using System;

namespace WebApplication.Controllers
{

    public class HomeController : Controller
    {

        public static EkCarTypeEnum _topCategoryCarType;
        public static int pageSize = 40;
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
        public class ManufactureNumbers
        {
            public string Manufacture { get; set; }
            public IList<string> ManufactureNumbersList { get; set; }
        }

        public void CreateExcel()
        {
            ExcelPackage ExcelPkg = new ExcelPackage();
            ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Блок управления гидроусилителя Автобуси");
            using (ExcelRange Rng = wsSheet1.Cells["A1:B100000"])
            {
                ExcelTable table = wsSheet1.Tables.Add(Rng, "tblSalesman");
                table.Name = "tblSales";
                table.Columns[0].Name = "AutoParts/Manufacture/Model";
                table.Columns[1].Name = "Link";
                table.ShowFilter = false;
            }
            Dictionary<string, string> test = new Dictionary<string, string>();

            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Bus).Select(x => x.Manufacturers).FirstOrDefault();

            int i = 2;
            foreach (var itemManufacture in carTree)
            {
                foreach (var itemCarModels in itemManufacture.CarModels)
                {
                    if (itemCarModels.Children == null)
                    {
                        using (ExcelRange Rng = wsSheet1.Cells["A" + i])
                        {
                            Rng.Value = String.Format("Блок управления гидроусилителя {0} {1}", itemManufacture.Name, itemCarModels.Name);
                        }
                        using (ExcelRange Rng = wsSheet1.Cells["B" + i])
                        {
                            Rng.Value = String.Format("https://bi-bi.com.ua/topcategoryid/622/{0}/{1}/group-250842/Рулевое-управление/261084/Блок-управления-гидроусилителя", itemManufacture.Name.ToLower().Replace(" ", "-"), itemCarModels.Name.ToLower().Replace(" ", "-"));
                        }
                        i++;
                    }
                    else
                    {
                        foreach (var itemModifications in itemCarModels.Children)
                        {
                            using (ExcelRange Rng = wsSheet1.Cells["A" + i])
                            {
                                Rng.Value = String.Format("Блок управления гидроусилителя {0} {1}", itemManufacture.Name, itemModifications.Name);
                            }
                            using (ExcelRange Rng = wsSheet1.Cells["B" + i])
                            {
                                Rng.Value = String.Format("https://bi-bi.com.ua/topcategoryid/622/{0}/{1}/group-250842/Рулевое-управление/261084/Блок-управления-гидроусилителя", itemManufacture.Name.ToLower().Replace(" ", "-"), itemModifications.Name.ToLower().Replace(" ", "-"));
                            }
                            i++;
                        }
                    }
                }
            }
            wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFitColumns();
            ExcelPkg.SaveAs(new FileInfo("Блок управления гидроусилителя Автобуси.xlsx"));

        }

        [Route("/")]
        [Route("/{topcategory?}/")]
        [Route("/{topcategory?}/{category?}")]
        [Route("/{topcategory?}/{category?}/{carmanufacture?}")]
        [Route("/{topcategory?}/{category?}/{carmanufacture?}/{carmodel?}")]
        [Route("/{topcategory?}/{category?}/{carmanufacture?}/{carmodel?}/{maincategoryid?}")]
        [Route("/{topcategory?}/{category?}/{carmanufacture?}/{carmodel?}/{maincategoryid?}/{maincategoryname?}")]
        [Route("/{topcategory?}/{category?}/{carmanufacture?}/{carmodel?}/{maincategoryid?}/{maincategoryname?}/{subcategoryid?}")]
        [Route("/{topcategory?}/{category?}/{carmanufacture?}/{carmodel?}/{maincategoryid?}/{maincategoryname?}/{subcategoryid?}/{subcategoryname?}")]
        [Route("/{topcategory?}/{category?}/{carmanufacture?}/{carmodel?}/{maincategoryid?}/{maincategoryname?}/{subcategoryid?}/{subcategoryname?}/{subchildcategoryid?}")]
        [Route("/{topcategory?}/{category?}/{carmanufacture?}/{carmodel?}/{maincategoryid?}/{maincategoryname?}/{subcategoryid?}/{subcategoryname?}/{subchildcategoryid?}/{subchildcategoryname?}")]
        [Route("/{topcategory?}/{category?}/{carmanufacture?}/{carmodel?}/{maincategoryid?}/{maincategoryname?}/{subcategoryid?}/{subcategoryname?}/{subchildcategoryid?}/{subchildcategoryname?}/{kioskid?}")]
        [Route("/{topcategory?}/{category?}/{carmanufacture?}/{carmodel?}/{maincategoryid?}/{maincategoryname?}/{subcategoryid?}/{subcategoryname?}/{subchildcategoryid?}/{subchildcategoryname?}/{lastcategoryid?}/{kioskid?}")]
        [Route("/{topcategory?}/{category?}/{carmanufacture?}/{carmodel?}/{maincategoryid?}/{maincategoryname?}/{subcategoryid?}/{subcategoryname?}/{subchildcategoryid?}/{subchildcategoryname?}/{lastcategoryid?}/{lastcategoryname?}/{kioskid?}")]



        public IActionResult Index(string kioskId)
        {
            //CreateExcel();
            //IList<string> testingList = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
            //var newTest = testingList.AsEnumerable().OrderBy(n => Guid.NewGuid()).Take(5);
            //HtmlWeb web = new HtmlWeb();

            //HtmlDocument doc = web.Load("https://zapchasti.ria.com/uk/map-catalog-number/bmw/");
            //var text = doc.ParsedText;
            //System.IO.File.AppendAllText(@"c:\temp\zapchasti1.txt", "riadon3;");//carTemp + item.Name);
            //var divsDescNew = doc.DocumentNode.QuerySelectorAll("a.elem");
            //foreach (var item in divsDescNew)
            //{
            //   System.IO.File.AppendAllText(@"c:\temp\zapchasti.txt", item.InnerText.Replace("Запчастина ", "")+";");//carTemp + item.Name);
            //}
            //IList<ManufactureNumbers> manufacturePartNumbers = new List<ManufactureNumbers>();
            //IList<string> tempNumbers = new List<string>();

            //foreach (var item in divsDescNew)
            //{
            //    tempNumbers.Add(item.InnerText);
            //}
            //manufacturePartNumbers.Add(new ManufactureNumbers() { Manufacture = "bmw", ManufactureNumbersList = tempNumbers });

            //string pathSitemap12345 = @"c:\temp\sitemap123.json";
            ////System.IO.File.AppendAllText(pathSitemap12345, JsonSerializer.Serialize(manufacturePartNumbers));
            //var readData = System.IO.File.ReadAllText(@"c:\temp\zapchasti.txt").Split(";").ToList();
            //var tempReadData = JsonSerializer.Deserialize<List<ManufactureNumbers>>(readData);

            //System.Web.HttpUtility.UrlEncode(myString)

            TitleController titleController = new TitleController();
            titleController.GetTitleSite();
            string cookieValueFromReq = Request.Cookies["kioskId"];
            string _topCategoryId = HttpContext.Session.GetString("topCategoryId");
            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddMonths(1);


            Response.Cookies.Append("kioskId", String.IsNullOrEmpty(kioskId) ? "116" : kioskId, option);

            HttpContext.Session.Remove("topCategoryId");
            if (kioskId == null)
            {
                kioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            }

            HttpContext.Session.SetString("kioskId", kioskId == null ? "116" : kioskId);
            ViewData["CartWidgetPrice"] = HttpContext.Session.GetString("cartWidgetPrice");

            string carTemp = "https://bi-bi.com.ua/topcategory/620/";

            if (RouteData.Values["category"] != null)
            {
                _topCategoryId = RouteData.Values["category"].ToString();
            }
            _topCategoryCarType = new EkSiteFactory().GetCarTypeEnum(String.IsNullOrEmpty(_topCategoryId) ? "620" : _topCategoryId);
            var carTreeTest = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();

            if (_topCategoryId == "621" || _topCategoryId == "622")
            {//legkovi-gruzovi-avtobusy use similar autoparts category = 620
                _topCategoryId = "620";
            }
            var treeMainCategories = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == (String.IsNullOrEmpty(_topCategoryId) || _topCategoryId.Length > 5 ? "620" : _topCategoryId)).FirstOrDefault().Children;




            ViewBag.TitleText = "Авторозбірка Шрот Купити Запчастини Б/у і Нові";
            if (RouteData.Values["category"] != null && (RouteData.Values["category"].ToString() == "99193" || RouteData.Values["category"].ToString() == "18554"))
            {
                var tempC = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == RouteData.Values["category"].ToString()).FirstOrDefault().Children;
                return View(new RightTreeViewModel()
                {
                    ViewName = "_AutoPartsTree",
                    ProductCategoryList = tempC,
                    TopCategoryId = RouteData.Values["category"].ToString(),
                    TopCategoryName = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == RouteData.Values["category"].ToString()).FirstOrDefault().Name["ru"],
                    ReallyTopCategoryId = RouteData.Values["category"].ToString()
                });
            }
            if (RouteData.Values["topcategory"] != null && RouteData.Values["topcategory"].ToString().ToLower() != "search")
            {
                _topCategoryCarType = new EkSiteFactory().GetCarTypeEnum(RouteData.Values["category"] != null ? RouteData.Values["category"].ToString() : "620");

            }
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            var _topCategoryTempId = HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId");

            if (RouteData.Values["topcategory"] != null && RouteData.Values["topcategory"].ToString().ToLower() == "search")
            {
                RightTreeViewModel rightTree = GetNumberInput(RouteData.Values["category"].ToString());
                rightTree.ProductCategoryList = treeMainCategories;
                return View(rightTree);
            }
            if (RouteData.Values["topcategory"] != null && RouteData.Values["category"].ToString().ToLower() == "customsearch")
            {
                RightTreeViewModel rightTree = GetNumberInput(RouteData.Values["carmodel"].ToString(), RouteData.Values["carmanufacture"].ToString());
                rightTree.ProductCategoryList = treeMainCategories;
                return View(rightTree);
            }
            if (RouteData.Values["carmanufacture"] != null && RouteData.Values["carmodel"] == null)
            {
                var carManufactureName = RouteData.Values["carmanufacture"].ToString().ToUpper().Contains("MERCEDES") ? RouteData.Values["carmanufacture"].ToString().ToUpper() : RouteData.Values["carmanufacture"].ToString().ToUpper().Replace("-", " ");
                ViewBag.TitleText = "Авторозбірка Шрот Купити Запчастини Б/У і Нові " + carManufactureName;
                var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault();
                var modelDescription = carTree.Where(x => x.Name == carManufactureName).Select(y => y.description).FirstOrDefault();
                return View(new RightTreeViewModel() { ViewName = "_CarModels", ProductCategoryList = treeMainCategories, ModelDescription = modelDescription, ManufacturerSelected = carManufactureName, ModelsList = modelTree, TopCategoryId = HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId"), kioskId = "116" });
            }

            if (RouteData.Values["carmodel"] != null && RouteData.Values["maincategoryname"] == null)
            {
                var carManufactureName = RouteData.Values["carmanufacture"].ToString().ToUpper().Contains("MERCEDES") ? RouteData.Values["carmanufacture"].ToString().ToUpper() : RouteData.Values["carmanufacture"].ToString().ToUpper().Replace("-", " ");
                var carmodel = RouteData.Values["carmodel"].ToString().ToUpper().Replace("-", " ");
                var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault();
                if (modelTree.Where(x => x.Name == carmodel).Select(y => y.Children).FirstOrDefault() == null)
                {
                    RightTreeViewModel rightTree = GetCategoryAutoParts(carManufactureName, carmodel, null, RouteData.Values["category"].ToString());
                    rightTree.ProductCategoryList = treeMainCategories;
                    return View(rightTree);
                }
                else
                {
                    RightTreeViewModel rightTree = GetModelModifications(carManufactureName, carmodel, RouteData.Values["category"].ToString());
                    rightTree.ProductCategoryList = treeMainCategories;
                    return View(rightTree);
                }
            }
            if (RouteData.Values["carmodel"] != null && RouteData.Values["maincategoryname"] != null && RouteData.Values["subcategoryid"] == null)
            {
                var carManufactureName = RouteData.Values["carmanufacture"].ToString().ToUpper().Contains("MERCEDES") ? RouteData.Values["carmanufacture"].ToString().ToUpper() : RouteData.Values["carmanufacture"].ToString().ToUpper().Replace("-", " ");
                var carmodel = RouteData.Values["carmodel"].ToString().ToUpper().Replace("-", " ");
                var mainCategoryId = RouteData.Values["maincategoryid"].ToString().ToUpper();
                var mainCategoryName = RouteData.Values["maincategoryname"].ToString().Replace("-", " ");
                mainCategoryName = FirstCharToUpper(mainCategoryName);
                RightTreeViewModel rightTree = GetMainSubcategories(carManufactureName, carmodel, mainCategoryId, mainCategoryName, _topCategoryId);
                rightTree.ProductCategoryList = treeMainCategories;
                return View(rightTree);
            }
            if (RouteData.Values["carmodel"] != null && RouteData.Values["maincategoryname"] != null && RouteData.Values["subcategoryid"] != null && RouteData.Values["subchildcategoryid"] == null)
            {
                var carManufactureName = RouteData.Values["carmanufacture"].ToString().ToUpper().Contains("MERCEDES") ? RouteData.Values["carmanufacture"].ToString().ToUpper() : RouteData.Values["carmanufacture"].ToString().ToUpper().Replace("-", " ");
                var carmodel = RouteData.Values["carmodel"].ToString().ToUpper().Replace("-", " ");
                var mainCategoryId = RouteData.Values["maincategoryid"].ToString().ToUpper();
                var mainCategoryName = RouteData.Values["maincategoryname"].ToString().Replace("-", " ");
                var subCategoryId = RouteData.Values["subcategoryid"].ToString().ToUpper();
                var subCategoryName = RouteData.Values["subcategoryname"].ToString().Replace("-", " ");


                var subChildId = RouteData.Values["lastcategoryid"] != null ? RouteData.Values["subchildcategoryid"].ToString().ToUpper() : null;
                var subChildName = RouteData.Values["subchildcategoryname"] != null ? RouteData.Values["subchildcategoryname"].ToString().Replace("-", " ") : null;

                mainCategoryName = FirstCharToUpper(mainCategoryName);
                subCategoryName = FirstCharToUpper(subCategoryName);
                subChildName = FirstCharToUpper(subChildName);
                RightTreeViewModel rightTree = GetMainSubChildsCategories(carManufactureName, carmodel, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName, _topCategoryId);
                rightTree.ProductCategoryList = treeMainCategories;
                return View(rightTree);
            }
            if (RouteData.Values["carmodel"] != null && RouteData.Values["maincategoryname"] != null && RouteData.Values["subcategoryid"] != null && RouteData.Values["subchildcategoryid"] != null && RouteData.Values["lastcategoryid"] == null)
            {
                var carManufactureName = RouteData.Values["carmanufacture"].ToString().ToUpper().Contains("MERCEDES") ? RouteData.Values["carmanufacture"].ToString().ToUpper() : RouteData.Values["carmanufacture"].ToString().ToUpper().Replace("-", " ");
                var carmodel = RouteData.Values["carmodel"].ToString().ToUpper().Replace("-", " ");
                var mainCategoryId = RouteData.Values["maincategoryid"].ToString().ToUpper();
                var mainCategoryName = RouteData.Values["maincategoryname"].ToString().Replace("-", " ");

                var subCategoryId = RouteData.Values["subcategoryid"].ToString().ToUpper();
                var subCategoryName = RouteData.Values["subcategoryname"].ToString().Replace("-", " ");

                var subChildId = RouteData.Values["subchildcategoryid"] != null ? RouteData.Values["subchildcategoryid"].ToString().ToUpper() : null;
                var subChildName = RouteData.Values["subchildcategoryname"] != null ? RouteData.Values["subchildcategoryname"].ToString().Replace("-", " ") : null;

                var lastChildId = RouteData.Values["lastcategoryid"] != null ? RouteData.Values["lastcategoryid"].ToString().ToUpper() : null;
                var lastChildName = RouteData.Values["lastcategoryname"] != null ? RouteData.Values["lastcategoryname"].ToString().Replace("-", " ") : null;

                mainCategoryName = FirstCharToUpper(mainCategoryName);
                subCategoryName = FirstCharToUpper(subCategoryName);
                subChildName = FirstCharToUpper(subChildName);
                lastChildName = FirstCharToUpper(lastChildName);

                RightTreeViewModel rightTree = GetProductList(carManufactureName, carmodel, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName, subChildId, subChildName, lastChildId, lastChildName, _topCategoryId); ;
                rightTree.ProductCategoryList = treeMainCategories;
                return View(rightTree);
            }

            return View(new RightTreeViewModel() { ViewName = "_CarTree", ProductCategoryList = treeMainCategories.ToArray(), ManufacturerList = carTree, kioskId = HttpContext.Session.GetString("kioskId"), TopCategoryId = HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId") });
        }

        //====================METHOD TO SWITCH TYPE CAR TOP CATEGORY ======================================
        public IActionResult ShowMainView(string topCategoryId)
        {
            _topCategoryCarType = new EkSiteFactory().GetCarTypeEnum(topCategoryId);
            HttpContext.Session.SetString("topCategoryId", topCategoryId == null ? "620" : topCategoryId);
            if (topCategoryId == "99193" || topCategoryId == "18554" || topCategoryId == "156")
            {
                var tempC = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == topCategoryId).FirstOrDefault().Children;
                foreach (var item in tempC)
                {
                    if (item.Children != null && !item.CategoryId.Contains("GROUP"))
                    {
                        item.CategoryId = "GROUP-" + item.CategoryId;
                    }
                }
                string topCategoryName = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == topCategoryId).FirstOrDefault().Name["ru"];
                return View("_AutoPartsTree", new RightTreeViewModel() { ProductCategoryList = tempC, TopCategoryName = topCategoryName, TopCategoryId = topCategoryId, ReallyTopCategoryId = (HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId")) });
            }
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");

            return View("_CarTree", new RightTreeViewModel() { ManufacturerList = carTree, TopCategoryId = topCategoryId, kioskId = tempKioskId });
        }
        //====================METHOD TO SHOW CAR MODELS ======================================

        public IActionResult SelectManufactureAndModel(string carmanufacture, string topcategoryid)
        {
            if (String.IsNullOrEmpty(carmanufacture))
            {
                return ShowMainView(topcategoryid);
            }
            carmanufacture = carmanufacture.ToLower().Contains("mercedes") ? carmanufacture.Replace(" ", "-") : carmanufacture;
            TitleController titleController = new TitleController();
            titleController.GetTitleSite();
            string carManufactureName = carmanufacture;
            carManufactureName = carManufactureName != null ? carManufactureName.ToUpper() : carManufactureName;
            if (String.IsNullOrEmpty(carManufactureName))
            {
                HttpContext.Session.Remove("topCategoryId");
                var mainTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
                return View("_CarTree", new RightTreeViewModel() { ManufacturerList = mainTree });
            }
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault();
            var modelDescription = carTree.Where(x => x.Name == carManufactureName).Select(y => y.description).FirstOrDefault();
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");

            return View("_CarModels", new RightTreeViewModel() { ManufacturerSelected = carManufactureName, ModelDescription = modelDescription, ModelsList = modelTree, TopCategoryId = String.IsNullOrEmpty(topcategoryid) ? "620" : topcategoryid, kioskId = tempKioskId });
        }
        public IActionResult SelectModelModifications(string carManufactureName, string carModel, string topcategoryid)
        {
            carManufactureName = carManufactureName != null ? carManufactureName.ToUpper() : carManufactureName;
            carModel = carModel != null ? carModel.ToUpper() : carModel;
            if (String.IsNullOrEmpty(carManufactureName))
            {
                HttpContext.Session.Remove("topCategoryId");
                var mainTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == EkCarTypeEnum.Car).Select(x => x.Manufacturers).FirstOrDefault();
                return View("_CarTree", new RightTreeViewModel() { ManufacturerList = mainTree });
            }
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault().Where(x => x.Name == carModel);

            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            return View("_CarModels", new RightTreeViewModel() { ManufacturerSelected = carManufactureName, ModelSelected = carModel, ModelsList = modelTree.Select(x => x.Children).FirstOrDefault(), IsModificationList = true, TopCategoryId = topcategoryid, kioskId = tempKioskId });
        }
        public RightTreeViewModel GetModelModifications(string carManufactureName, string carModel, string topcategoryid)
        {
            carManufactureName = carManufactureName != null ? carManufactureName.ToUpper() : carManufactureName;
            carModel = carModel != null ? carModel.ToUpper() : carModel;

            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault().Where(x => x.Name == carModel);

            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            ViewBag.TitleText = "Авторозбірка Шрот Купити Запчастини Б/У і Нові " + carManufactureName + " " + carModel;
            return new RightTreeViewModel() { ViewName = "_CarModels", ManufacturerSelected = carManufactureName, ModelSelected = carModel, ModelsList = modelTree.Select(x => x.Children).FirstOrDefault(), IsModificationList = true, TopCategoryId = topcategoryid, kioskId = tempKioskId };
        }
        public RightTreeViewModel GetCategoryAutoParts(string carManufactureName, string carModel, string modification, string topcategoryid)
        {
            carManufactureName = carManufactureName != null ? carManufactureName.ToUpper() : carManufactureName;
            carModel = carModel != null ? carModel.ToUpper() : carModel;
            modification = modification != null ? modification.ToUpper() : modification;

            //string topCategoryId = String.IsNullOrEmpty(HttpContext.Session.GetString("topCategoryId")) || HttpContext.Session.GetString("topCategoryId") == "undefined" || HttpContext.Session.GetString("topCategoryId") == "621" || HttpContext.Session.GetString("topCategoryId") == "622" ? "620" : HttpContext.Session.GetString("topCategoryId");
            string categoryTop = String.IsNullOrEmpty(topcategoryid) || topcategoryid == "621" || topcategoryid == "622" ? "620" : topcategoryid;
            var autoParts = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == categoryTop).FirstOrDefault().Children;
            string reallyTopCategory = "";
            foreach (var item in autoParts)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP-"))
                {
                    item.CategoryId = "GROUP-" + item.CategoryId;
                }
            }
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            if (HttpContext.Session.GetString("topCategoryId") == "621")
            {
                reallyTopCategory = "621";
            }
            ViewBag.TitleText = "Авторозбірка Шрот Купити Запчастини Б/У і Нові " + carManufactureName + " " + carModel;
            return new RightTreeViewModel() { ViewName = "_AutoPartsTree", ProductCategoryList = autoParts, ManufacturerSelected = carManufactureName, ModelSelected = String.IsNullOrEmpty(modification) || modification == "null" || modification == "NULL" ? carModel : modification, kioskId = tempKioskId, ReallyTopCategoryId = reallyTopCategory, TopCategoryId = topcategoryid };
        }
        //====================METHOD TO SHOW PRODUCTS CATEGORIES ======================================
        public IActionResult ShowCategoryAutoParts(string carManufactureName, string carModel, string modification, string topcategoryid)
        {
            carManufactureName = carManufactureName != null ? carManufactureName.ToUpper() : carManufactureName;
            carModel = carModel != null ? carModel.ToUpper() : carModel;
            modification = modification != null ? modification.ToUpper() : modification;

            string categoryTop = String.IsNullOrEmpty(topcategoryid) || topcategoryid == "621" || topcategoryid == "622" ? "620" : topcategoryid;
            var autoParts = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == categoryTop).FirstOrDefault().Children;
            string reallyTopCategory = "";
            foreach (var item in autoParts)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP-"))
                {
                    item.CategoryId = "GROUP-" + item.CategoryId;
                }
            }
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            if (HttpContext.Session.GetString("topCategoryId") == "621")
            {
                reallyTopCategory = "621";
            }
            return View("_AutoPartsTree", new RightTreeViewModel() { ProductCategoryList = autoParts, ManufacturerSelected = carManufactureName, ModelSelected = String.IsNullOrEmpty(modification) || modification == "null" || modification == "NULL" ? carModel : modification, kioskId = tempKioskId, ReallyTopCategoryId = reallyTopCategory, TopCategoryId = topcategoryid });
        }
        //====================METHOD TO SHOW PRODUCTS SUBCATEGORIES ======================================
        public IActionResult ShowMainSubcategories(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string topcategoryid)
        {
            if (!String.IsNullOrEmpty(mainCategoryId))
            {
                mainCategoryId = mainCategoryId.ToUpper();
            }
            carManufactureName = carManufactureName != null ? carManufactureName.ToUpper() : carManufactureName;
            carModel = carModel != null ? carModel.ToUpper() : carModel;
            mainCategoryName = FirstCharToUpper(mainCategoryName);

            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();


            //string topCategoryId = String.IsNullOrEmpty(HttpContext.Session.GetString("topCategoryId")) || HttpContext.Session.GetString("topCategoryId") == "undefined" || HttpContext.Session.GetString("topCategoryId") == "621" || HttpContext.Session.GetString("topCategoryId") == "622" ? "620" : HttpContext.Session.GetString("topCategoryId");
            string categoryTop = String.IsNullOrEmpty(topcategoryid) || topcategoryid == "621" || topcategoryid == "622" ? "620" : topcategoryid;
            var autoPartsSubCategories = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == categoryTop).FirstOrDefault().Children.Where(x => x.CategoryId == mainCategoryId).Select(x => x.Children).FirstOrDefault();

            foreach (var item in autoPartsSubCategories)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP-"))
                {
                    item.CategoryId = "GROUP-" + item.CategoryId;
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
                TopCategoryId = topcategoryid,
                ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" ? "621" : (HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId"))
            };
            return View("_AutoPartsSubTree", treeView);
        }
        public RightTreeViewModel GetMainSubcategories(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string topcategoryid)
        {
            if (!String.IsNullOrEmpty(mainCategoryId))
            {
                mainCategoryId = mainCategoryId.ToUpper();
            }
            carManufactureName = carManufactureName != null ? carManufactureName.ToUpper() : carManufactureName;
            carModel = carModel != null ? carModel.ToUpper() : carModel;
            mainCategoryName = FirstCharToUpper(mainCategoryName);
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();


            //string topCategoryId = String.IsNullOrEmpty(HttpContext.Session.GetString("topCategoryId")) || HttpContext.Session.GetString("topCategoryId") == "undefined" || HttpContext.Session.GetString("topCategoryId") == "621" || HttpContext.Session.GetString("topCategoryId") == "622" ? "620" : HttpContext.Session.GetString("topCategoryId");
            string categoryTop = String.IsNullOrEmpty(topcategoryid) || topcategoryid == "621" || topcategoryid == "622" ? "620" : topcategoryid;
            var autoParts = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == categoryTop).FirstOrDefault().Children;

            foreach (var item in autoParts)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP-"))
                {
                    item.CategoryId = "GROUP-" + item.CategoryId;
                }
            }
            var autoPartsSubCategories = autoParts.Where(x => x.CategoryId == mainCategoryId).Select(x => x.Children).FirstOrDefault();
            if (autoPartsSubCategories == null)
            {
                bool isNew = false;
                var responceAllegro = GetAllegroProducts(carManufactureName, carModel, mainCategoryId, null, OfferStateEnum.Used, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "").Result;
                if (responceAllegro.Total == 0)
                {
                    isNew = true;
                    responceAllegro = GetAllegroProducts(carManufactureName, carModel, mainCategoryId, null, OfferStateEnum.New, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "").Result;
                }
                HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
                RightTreeViewModel treeViewModel = new RightTreeViewModel();

                treeViewModel.ManufacturerSelected = carManufactureName;
                treeViewModel.ModelSelected = carModel;
                treeViewModel.FilterName = GetFilterName(mainCategoryId);
                treeViewModel.MainCategoryId = mainCategoryId;
                treeViewModel.MainCategoryName = mainCategoryName;
                treeViewModel.PageNumber = 1;
                treeViewModel.TopCategoryId = topcategoryid;
                treeViewModel.SelectedTiresSizes = new SelectedTires()
                {
                    Height = null,
                    Width = null,
                    Quantity = null,
                    RSize = null
                };
                treeViewModel.Tires = new TiresFilter()
                {
                    Height = new TiresSizes().GetTiresHeight(),
                    Width = new TiresSizes().GetTiresWidth(),
                    RSize = new TiresSizes().GetTiresRSize(),
                    Quantity = new TiresSizes().GetTiresCnt()
                };
                treeViewModel.EngineValues = new EngineValue().GetEngineValue();
                treeViewModel.SelectedEngineValue = null;
                treeViewModel.ReallyTopCategoryId = topcategoryid == "621" ? "621" : topcategoryid == null ? "620" : topcategoryid;
                treeViewModel.FunctionReturnFromProducts = String.Format("selectMainCategory('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", carManufactureName, carModel, mainCategoryId, mainCategoryName, 116, topcategoryid);
                treeViewModel.AllegroOfferList = responceAllegro.Products;
                treeViewModel.FakeAllegroList = FakeListForPager(responceAllegro.Total);
                treeViewModel.ControllerName = "ShowMainSubChildsCategories";
                treeViewModel.OfferSorting = OfferSortingEnum.Relevance;
                treeViewModel.OfferState = isNew ? OfferStateEnum.New : OfferStateEnum.Used;
                treeViewModel.ViewName = "_ProductsList";
                treeViewModel.SelectedCategoryName = mainCategoryName;
                treeViewModel.ScriptData = GetEcommerceScriptData(treeViewModel);
                HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeViewModel));
                ViewBag.TitleText = "Купити " + mainCategoryId + " " + carManufactureName + " " + carModel + " з розборки";

                return treeViewModel;
            }
            foreach (var item in autoPartsSubCategories)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP-"))
                {
                    item.CategoryId = "GROUP-" + item.CategoryId;
                }
            }
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            RightTreeViewModel treeView = new RightTreeViewModel()
            {
                ManufacturerSelected = carManufactureName,
                ViewName = "_AutoPartsSubTree",
                ModelSelected = carModel,
                ProductCategoryList = autoPartsSubCategories,
                MainCategoryId = mainCategoryId,
                MainCategoryName = mainCategoryName,
                kioskId = tempKioskId,
                TopCategoryId = topcategoryid,
                ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" ? "621" : (HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId"))
            };
            ViewBag.TitleText = "Купити " + mainCategoryName + " " + carManufactureName + " " + carModel + " з розборки";
            return treeView;
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
        public RightTreeViewModel GetMainSubChildsCategories(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string subCategoryId, string subCategoryName, string topcategoryid)
        {
            if (!String.IsNullOrEmpty(mainCategoryId))
            {
                mainCategoryId = mainCategoryId.ToUpper();
            }
            if (!String.IsNullOrEmpty(subCategoryId))
            {
                subCategoryId = subCategoryId.ToUpper();
            }
            carManufactureName = carManufactureName != null ? carManufactureName.ToUpper() : carManufactureName;
            carModel = carModel != null ? carModel.ToUpper() : carModel;
            mainCategoryName = FirstCharToUpper(mainCategoryName);
            subCategoryName = FirstCharToUpper(subCategoryName);
            //if (modelTree == null)
            //{
            //    throw new Exception("Error");
            //}
            string categoryTop = String.IsNullOrEmpty(topcategoryid) || topcategoryid == "621" || topcategoryid == "622" ? "620" : topcategoryid;
            var tempAutoPartsSubChildCategories = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == categoryTop).FirstOrDefault().Children.Where(x => x.CategoryId == mainCategoryId).Select(x => x.Children).FirstOrDefault();
            var autoPartsSubChildCategories = tempAutoPartsSubChildCategories.Where(x => x.CategoryId == subCategoryId).Select(x => x.Children).FirstOrDefault();

            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            if (autoPartsSubChildCategories == null)
            {
                bool isNew = false;
                var responceAllegro = GetAllegroProducts(carManufactureName, carModel, subCategoryId, null, OfferStateEnum.Used, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "").Result;
                if (responceAllegro.Total == 0)
                {
                    isNew = true;
                    responceAllegro = GetAllegroProducts(carManufactureName, carModel, subCategoryId, null, OfferStateEnum.New, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "").Result;
                }
                HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
                RightTreeViewModel treeViewModel = new RightTreeViewModel();

                treeViewModel.ManufacturerSelected = carManufactureName;
                treeViewModel.ModelSelected = carModel;
                treeViewModel.FilterName = GetFilterName(subCategoryId);
                treeViewModel.MainCategoryId = mainCategoryId;
                treeViewModel.MainCategoryName = mainCategoryName;
                treeViewModel.SubCategoryId = subCategoryId;
                treeViewModel.SubCategoryName = subCategoryName;
                treeViewModel.PageNumber = 1;
                treeViewModel.TopCategoryId = topcategoryid;
                treeViewModel.SelectedTiresSizes = new SelectedTires()
                {
                    Height = null,
                    Width = null,
                    Quantity = null,
                    RSize = null
                };
                treeViewModel.Tires = new TiresFilter()
                {
                    Height = new TiresSizes().GetTiresHeight(),
                    Width = new TiresSizes().GetTiresWidth(),
                    RSize = new TiresSizes().GetTiresRSize(),
                    Quantity = new TiresSizes().GetTiresCnt()
                };
                treeViewModel.EngineValues = new EngineValue().GetEngineValue();
                treeViewModel.SelectedEngineValue = null;
                treeViewModel.ReallyTopCategoryId = topcategoryid == "621" ? "621" : topcategoryid == null ? "620" : topcategoryid;
                treeViewModel.FunctionReturnFromProducts = String.Format("selectMainCategory('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", carManufactureName, carModel, mainCategoryId, mainCategoryName, tempKioskId, topcategoryid);
                treeViewModel.AllegroOfferList = responceAllegro.Products;
                treeViewModel.FakeAllegroList = FakeListForPager(responceAllegro.Total);
                treeViewModel.ControllerName = "ShowMainSubChildsCategories";
                treeViewModel.OfferSorting = OfferSortingEnum.Relevance;
                treeViewModel.OfferState = isNew ? OfferStateEnum.New : OfferStateEnum.Used;
                treeViewModel.kioskId = tempKioskId;
                treeViewModel.ViewName = "_ProductsList";
                treeViewModel.SelectedCategoryName = subCategoryName;
                treeViewModel.ScriptData = GetEcommerceScriptData(treeViewModel);
                HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeViewModel));
                ViewBag.TitleText = "Купити " + subCategoryName + " " + carManufactureName + " " + carModel + " з розборки";

                return treeViewModel;
            }
            foreach (var item in autoPartsSubChildCategories)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP-"))
                {
                    item.CategoryId = "GROUP-" + item.CategoryId;
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
                TopCategoryId = topcategoryid,
                ProductCategoryList = autoPartsSubChildCategories,
                ViewName = "_AutoPartsSubChildsTree",
                ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" ? "621" : (HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId"))
            };
            ViewBag.TitleText = "Купити " + subCategoryName + " " + carManufactureName + " " + carModel + " з розборки";
            return treeView;

        }
        //====================METHOD TO SHOW PRODUCTS(ENTERED IN PRE-LAST GROUP OF AUTOPARTS) ======================================
        public IActionResult ShowMainSubChildsCategories(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string subCategoryId, string subCategoryName, string topcategoryid)
        {
            if (!String.IsNullOrEmpty(mainCategoryId))
            {
                mainCategoryId = mainCategoryId.ToUpper();
            }
            if (!String.IsNullOrEmpty(subCategoryId))
            {
                subCategoryId = subCategoryId.ToUpper();
            }
            carManufactureName = carManufactureName != null ? carManufactureName.ToUpper() : carManufactureName;
            carModel = carModel != null ? carModel.ToUpper() : carModel;
            mainCategoryName = FirstCharToUpper(mainCategoryName);
            subCategoryName = FirstCharToUpper(subCategoryName);

            string categoryTop = String.IsNullOrEmpty(topcategoryid) || topcategoryid == "621" || topcategoryid == "622" ? "620" : topcategoryid;
            var tempAutoPartsSubChildCategories = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == categoryTop).FirstOrDefault().Children.Where(x => x.CategoryId == mainCategoryId).Select(x => x.Children).FirstOrDefault();
            var autoPartsSubChildCategories = tempAutoPartsSubChildCategories.Where(x => x.CategoryId == subCategoryId).Select(x => x.Children).FirstOrDefault();

            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            if (autoPartsSubChildCategories == null)
            {
                bool isNew = false;
                var responceAllegro = GetAllegroProducts(carManufactureName, carModel, subCategoryId, null, OfferStateEnum.Used, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "").Result;
                if (responceAllegro.Total == 0)
                {
                    isNew = true;
                    responceAllegro = GetAllegroProducts(carManufactureName, carModel, subCategoryId, null, OfferStateEnum.New, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "").Result;
                }
                HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
                RightTreeViewModel treeViewModel = new RightTreeViewModel();

                treeViewModel.ManufacturerSelected = carManufactureName;
                treeViewModel.ModelSelected = carModel;
                treeViewModel.FilterName = GetFilterName(subCategoryId);
                treeViewModel.MainCategoryId = mainCategoryId;
                treeViewModel.MainCategoryName = mainCategoryName;
                treeViewModel.SubCategoryId = subCategoryId;
                treeViewModel.SubCategoryName = subCategoryName;
                treeViewModel.PageNumber = 1;
                treeViewModel.TopCategoryId = topcategoryid;
                treeViewModel.SelectedTiresSizes = new SelectedTires()
                {
                    Height = null,
                    Width = null,
                    Quantity = null,
                    RSize = null
                };
                treeViewModel.Tires = new TiresFilter()
                {
                    Height = new TiresSizes().GetTiresHeight(),
                    Width = new TiresSizes().GetTiresWidth(),
                    RSize = new TiresSizes().GetTiresRSize(),
                    Quantity = new TiresSizes().GetTiresCnt()
                };
                treeViewModel.EngineValues = new EngineValue().GetEngineValue();
                treeViewModel.SelectedEngineValue = null;
                treeViewModel.ReallyTopCategoryId = topcategoryid == "621" ? "621" : topcategoryid == null ? "620" : topcategoryid;
                treeViewModel.FunctionReturnFromProducts = String.Format("selectMainCategory('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", carManufactureName, carModel, mainCategoryId, mainCategoryName, tempKioskId, topcategoryid);
                treeViewModel.AllegroOfferList = responceAllegro.Products;
                treeViewModel.FakeAllegroList = FakeListForPager(responceAllegro.Total);
                treeViewModel.ControllerName = "ShowMainSubChildsCategories";
                treeViewModel.OfferSorting = OfferSortingEnum.Relevance;
                treeViewModel.OfferState = isNew ? OfferStateEnum.New : OfferStateEnum.Used;
                treeViewModel.kioskId = tempKioskId;
                treeViewModel.SelectedCategoryName = subCategoryName;
                treeViewModel.ScriptData = GetEcommerceScriptData(treeViewModel);
                HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeViewModel));

                return View("_ProductsList", treeViewModel);
            }
            foreach (var item in autoPartsSubChildCategories)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP-"))
                {
                    item.CategoryId = "GROUP-" + item.CategoryId;
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
                TopCategoryId = topcategoryid,
                ProductCategoryList = autoPartsSubChildCategories,
                ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" ? "621" : (HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId"))
            };
            return View("_AutoPartsSubChildsTree", treeView);

        }
        //====================METHOD TO SHOW PRODUCTS(ENTERED IN LAST GROUP OF AUTOPARTS) ======================================
        public IActionResult ShowProductList(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string subCategoryId, string subCategoryName, string subChildId, string subChildName, string lastChildId, string lastChildName, string topcategoryid)
        {
            mainCategoryId = mainCategoryId.ToUpper();
            subCategoryId = subCategoryId.ToUpper();
            if (!String.IsNullOrEmpty(subChildId))
            {
                subChildId = subChildId.ToUpper();
            }
            if (!String.IsNullOrEmpty(lastChildId))
            {
                lastChildId = lastChildId.ToUpper();
            }
            carManufactureName = carManufactureName != null ? carManufactureName.ToUpper() : carManufactureName;
            carModel = carModel != null ? carModel.ToUpper() : carModel;
            string categoryTop = String.IsNullOrEmpty(topcategoryid) || topcategoryid == "621" || topcategoryid == "622" ? "620" : topcategoryid;
            mainCategoryName = FirstCharToUpper(mainCategoryName);
            subCategoryName = FirstCharToUpper(subCategoryName);
            subChildName = FirstCharToUpper(subChildName);
            lastChildName = FirstCharToUpper(lastChildName);
            var autoPartsSubChildCategories = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == categoryTop).FirstOrDefault().Children.Where(x => x.CategoryId == mainCategoryId).Select(x => x.Children).FirstOrDefault();
            if (autoPartsSubChildCategories != null)
            {
                autoPartsSubChildCategories = autoPartsSubChildCategories.Where(x => x.CategoryId == subCategoryId).Select(x => x.Children).FirstOrDefault();
            }
            var tempCat = autoPartsSubChildCategories == null ? null : autoPartsSubChildCategories.Where(x => x.CategoryId == subChildId).Select(x => x.Children).FirstOrDefault();
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");

            if (tempCat != null && lastChildId == null)
            {
                foreach (var item in tempCat)
                {
                    if (item.Children != null && !item.CategoryId.Contains("GROUP-"))
                    {
                        item.CategoryId = "GROUP-" + item.CategoryId;
                    }
                }
                RightTreeViewModel treeViewLast = new RightTreeViewModel()
                {
                    kioskId = tempKioskId,
                    ManufacturerSelected = carManufactureName,
                    ModelSelected = carModel,
                    MainCategoryId = mainCategoryId,
                    MainCategoryName = mainCategoryName,
                    SubCategoryId = subCategoryId,
                    SubCategoryName = subCategoryName,
                    SubChildCategoryId = subChildId,
                    SubChildCategoryName = subChildName,
                    ProductCategoryList = tempCat,
                    TopCategoryId = topcategoryid,
                    ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" ? "621" : (HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId"))
                };
                return View("AutoPartsLastTreeItems", treeViewLast);
            }
            var responceAllegro = GetAllegroProducts(carManufactureName, carModel, String.IsNullOrEmpty(lastChildId) ? subChildId : lastChildId, null, OfferStateEnum.Used, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "").Result;
            bool isNew = false;
            if (responceAllegro.Total == 0)
            {
                isNew = true;
                responceAllegro = GetAllegroProducts(carManufactureName, carModel, String.IsNullOrEmpty(lastChildId) ? subChildId : lastChildId, null, OfferStateEnum.New, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "").Result;
            }
            HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
            RightTreeViewModel treeView = new RightTreeViewModel()
            {
                kioskId = tempKioskId,
                ManufacturerSelected = carManufactureName,
                ModelSelected = carModel,
                FilterName = GetFilterName(String.IsNullOrEmpty(lastChildId) ? subChildId : lastChildId),
                MainCategoryId = mainCategoryId,
                MainCategoryName = mainCategoryName,
                SubCategoryId = subCategoryId,
                SubCategoryName = subCategoryName,
                SubChildCategoryId = subChildId,
                SubChildCategoryName = subChildName,
                LastChildId = lastChildId,
                LastChildName = lastChildName,
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
                EngineValues = new EngineValue().GetEngineValue(),
                SelectedEngineValue = null,
                TopCategoryId = topcategoryid,
                ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" ? "621" : (HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId")),
                OfferState = isNew ? OfferStateEnum.New : OfferStateEnum.Used,
                FunctionReturnFromProducts = String.Format("selectSubMainCategory('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')", carManufactureName, carModel, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName, tempKioskId, topcategoryid),
                AllegroOfferList = responceAllegro.Products,
                ControllerName = "ShowProductList",
                FakeAllegroList = FakeListForPager(responceAllegro.Total),
                OfferSorting = OfferSortingEnum.Relevance
            };
            treeView.SelectedCategoryName = String.IsNullOrEmpty(subChildName) ? lastChildName : subChildName;
            treeView.ScriptData = GetEcommerceScriptData(treeView);
            HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeView));
            return View("_ProductsList", treeView);
        }
        public RightTreeViewModel GetProductList(string carManufactureName, string carModel, string mainCategoryId, string mainCategoryName, string subCategoryId, string subCategoryName, string subChildId, string subChildName, string lastChildId, string lastChildName, string topcategoryid)
        {
            mainCategoryId = mainCategoryId.ToUpper();
            subCategoryId = subCategoryId.ToUpper();
            if (!String.IsNullOrEmpty(subChildId))
            {
                subChildId = subChildId.ToUpper();
            }
            if (!String.IsNullOrEmpty(lastChildId))
            {
                lastChildId = lastChildId.ToUpper();
            }
            carManufactureName = carManufactureName != null ? carManufactureName.ToUpper() : carManufactureName;
            carModel = carModel != null ? carModel.ToUpper() : carModel;
            mainCategoryName = FirstCharToUpper(mainCategoryName);
            subCategoryName = FirstCharToUpper(subCategoryName);
            subChildName = FirstCharToUpper(subChildName);
            lastChildName = FirstCharToUpper(lastChildName);
            string categoryTop = String.IsNullOrEmpty(topcategoryid) || topcategoryid == "621" || topcategoryid == "622" ? "620" : topcategoryid;
            var autoParts = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == categoryTop).FirstOrDefault().Children;
            foreach (var item in autoParts)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP-"))
                {
                    item.CategoryId = "GROUP-" + item.CategoryId;
                }
            }
            var autoPartsSubChildCategories = autoParts.Where(x => x.CategoryId == mainCategoryId).Select(x => x.Children).FirstOrDefault();

            foreach (var item in autoPartsSubChildCategories)
            {
                if (item.Children != null && !item.CategoryId.Contains("GROUP-"))
                {
                    item.CategoryId = "GROUP-" + item.CategoryId;
                }
            }

            autoPartsSubChildCategories = autoPartsSubChildCategories.Where(x => x.CategoryId == subCategoryId).Select(x => x.Children).FirstOrDefault();

            var tempCat = autoPartsSubChildCategories == null ? null : autoPartsSubChildCategories.Where(x => x.CategoryId == subChildId).Select(x => x.Children).FirstOrDefault();
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");

            if (tempCat != null && lastChildId == null)
            {
                foreach (var item in tempCat)
                {
                    if (item.Children != null && !item.CategoryId.Contains("GROUP-"))
                    {
                        item.CategoryId = "GROUP-" + item.CategoryId;
                    }
                }
                RightTreeViewModel treeViewLast = new RightTreeViewModel()
                {
                    kioskId = tempKioskId,
                    ManufacturerSelected = carManufactureName,
                    ModelSelected = carModel,
                    MainCategoryId = mainCategoryId,
                    MainCategoryName = mainCategoryName,
                    SubCategoryId = subCategoryId,
                    SubCategoryName = subCategoryName,
                    SubChildCategoryId = subChildId,
                    SubChildCategoryName = subChildName,
                    ProductCategoryList = tempCat,
                    TopCategoryId = topcategoryid,
                    ViewName = "AutoPartsLastTreeItems",
                    ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" ? "621" : (HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId"))
                };
                ViewBag.TitleText = String.Format("Купити {0} {1} {2} з розборки", subCategoryName, carManufactureName, carModel);
                return treeViewLast;
            }
            var responceAllegro = GetAllegroProducts(carManufactureName, carModel, String.IsNullOrEmpty(lastChildId) ? subChildId : lastChildId, null, OfferStateEnum.Used, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "").Result;
            bool isNew = false;
            if (responceAllegro.Total == 0)
            {
                isNew = true;
                responceAllegro = GetAllegroProducts(carManufactureName, carModel, String.IsNullOrEmpty(lastChildId) ? subChildId : lastChildId, null, OfferStateEnum.New, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "").Result;
            }
            HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
            RightTreeViewModel treeView = new RightTreeViewModel()
            {
                kioskId = tempKioskId,
                ManufacturerSelected = carManufactureName,
                ModelSelected = carModel,
                FilterName = GetFilterName(String.IsNullOrEmpty(lastChildId) ? subChildId : lastChildId),
                MainCategoryId = mainCategoryId,
                MainCategoryName = mainCategoryName,
                SubCategoryId = subCategoryId,
                SubCategoryName = subCategoryName,
                SubChildCategoryId = subChildId,
                SubChildCategoryName = subChildName,
                LastChildId = lastChildId,
                LastChildName = lastChildName,
                PageNumber = 1,
                ViewName = "_ProductsList",
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
                EngineValues = new EngineValue().GetEngineValue(),
                SelectedEngineValue = null,
                ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" ? "621" : (HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId")),
                OfferState = isNew ? OfferStateEnum.New : OfferStateEnum.Used,
                FunctionReturnFromProducts = String.Format("selectSubMainCategory('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')", carManufactureName, carModel, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName, tempKioskId, topcategoryid),
                AllegroOfferList = responceAllegro.Products,
                ControllerName = "ShowProductList",
                TopCategoryId = topcategoryid,
                FakeAllegroList = FakeListForPager(responceAllegro.Total),
                OfferSorting = OfferSortingEnum.Relevance
            };
            ViewBag.TitleText = String.Format("Купити {0} {1} {2} з розборки", String.IsNullOrEmpty(lastChildName) ? subChildName : lastChildName, carManufactureName, carModel);
            treeView.SelectedCategoryName = String.IsNullOrEmpty(lastChildName) ? subChildName : lastChildName;
            treeView.ScriptData = GetEcommerceScriptData(treeView);
            HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeView));
            return treeView;
        }
        //==================== METHOD TO GET ALLEGRO PRODUCTS BY INPUT VALUE  ======================================
        public IActionResult PartNumberInput(string partNumber)
        {
            //Session["SiteTitle"] = partNumber;
            bool isNew = false;
            string? selectedCategoryId = null;
            var rightTreeViewModelString = HttpContext.Session.GetString("rightTreeViewModel");
            if (rightTreeViewModelString != null)
            {
                RightTreeViewModel rightTree = JsonSerializer.Deserialize<RightTreeViewModel>(rightTreeViewModelString);
                if (rightTree.LastChildId != null)
                {
                    selectedCategoryId = rightTree.LastChildId;
                }
                if (rightTree.SubChildCategoryId != null)
                {
                    selectedCategoryId = rightTree.SubChildCategoryId;
                }
                if (rightTree.SubCategoryId != null)
                {
                    selectedCategoryId = rightTree.SubCategoryId;
                }
            }
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            var responceAllegro = GetAllegroProducts(null, null, selectedCategoryId, partNumber).Result;
            if (responceAllegro.Total == 0)
            {
                responceAllegro = GetAllegroProducts(null, null, selectedCategoryId, partNumber, OfferStateEnum.New).Result;
                isNew = true;
            }
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
                EngineValues = new EngineValue().GetEngineValue(),
                SelectedEngineValue = null,
                OfferSorting = OfferSortingEnum.Relevance,
                OfferState = isNew ? OfferStateEnum.New : OfferStateEnum.Used,
                PageNumber = 1
            };
            treeView.SelectedCategoryName = partNumber;
            treeView.ScriptData = GetEcommerceScriptData(treeView);
            HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeView));
            HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
            return View("_ProductsList", treeView);

        }
        public RightTreeViewModel GetNumberInput(string partNumber, string chosedCategoryId = "")
        {
            //Session["SiteTitle"] = partNumber;
            bool isNew = false;
            string? selectedCategoryId = null;
            var rightTreeViewModelString = HttpContext.Session.GetString("rightTreeViewModel");
            if (rightTreeViewModelString != null)
            {
                RightTreeViewModel rightTree = JsonSerializer.Deserialize<RightTreeViewModel>(rightTreeViewModelString);
                if (rightTree.LastChildId != null)
                {
                    selectedCategoryId = rightTree.LastChildId;
                }
                if (rightTree.SubChildCategoryId != null)
                {
                    selectedCategoryId = rightTree.SubChildCategoryId;
                }
                if (rightTree.SubCategoryId != null)
                {
                    selectedCategoryId = rightTree.SubCategoryId;
                }
            }
            if (!String.IsNullOrEmpty(chosedCategoryId))
            {
                selectedCategoryId = chosedCategoryId;
            }
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            var responceAllegro = GetAllegroProducts(null, null, selectedCategoryId, partNumber).Result;
            if (responceAllegro.Total == 0)
            {
                responceAllegro = GetAllegroProducts(null, null, selectedCategoryId, partNumber, OfferStateEnum.New).Result;
                isNew = true;
            }
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
                EngineValues = new EngineValue().GetEngineValue(),
                SelectedEngineValue = null,
                OfferSorting = OfferSortingEnum.Relevance,
                OfferState = isNew ? OfferStateEnum.New : OfferStateEnum.Used,
                PageNumber = 1,
                ViewName = "_ProductsList"
            };
            ViewBag.TitleText = "Купити " + partNumber;
            treeView.ScriptData = GetEcommerceScriptData(treeView);
            HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeView));
            HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
            return treeView;
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
            IList<string> placeFB = new List<string>() { "254699", "254703", "254705", "19063", "250885", "250405" };
            IList<string> placeAll = new List<string>() { "254580", "18708", "254683", "254685", "250902", "18962", "256100", "250878", "250876", "250406", "256006" };
            IList<string> engineType = new List<string>() { "312565","50825", "50828", "256143", "50831", "50829", "50830", "261131", "261131", "255442", "255441", "255440", "261150", "50824", "255451", "255447", "261133",
                "255446", "50838", "255450", "261148", "50839", "50840", "255629", "255448", "255445", "255449", "50837", "256158", "256141", "255520", "50862", "256205", "50841", "256227", "255519", "255521", "256142", "255456",
            "261135", "261093", "255458", "255457", "255459", "256144", "50847", "255480", "50845", "255479", "50844", "50846", "50848", "261137", "255481", "261129", "255478", "261149", "255477", "261136", "50843",
            "50854", "256159", "261138", "259309", "255507", "147922", "50855", "147924", "255506", "255508", "261140", "261139", "50856", "261142", "147923", "261130", "50853", "255517", "255512", "256160", "255514", "261141",
            "255509", "255511", "260944", "255513", "260943", "255515", "50835", "50834", "255443", "255444", "261132", "50833", "50822", "260924", "260923", "260926", "260927", "260928", "260929", "260925", "260911", "260910",
            "256016", "256012", "260912", "256011", "256013", "260898", "260913", "260907", "256014", "256017", "260899", "260900", "260914", "260915", "256015", "256010", "256018", "260909", "50872", "260901", "50868", "50867",
            "256006", "256001", "260903", "260904", "256004", "256003", "256008", "256002", "256007", "260906", "256005", "260908", "260905", "260902", "256009", "261091", "260934", "260937", "260935", "260938", "260939", "260940",
            "260941", "260936", "255925", "261094", "18856", "255917", "261200", "255918", "255935", "255933", "261083", "256226", "261092", "255929", "255922", "255931", "255932", "255924", "256204", "255923", "255927",
            "255920", "255934", "255926", "255930", "261089", "255928", "4134"};
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
        public async Task<EkKioskProductSearchInEuropeGetResponse> GetAllegroProducts(string carManufactureName, string carModel, string selectedCategoryId, string inputPartNumber, OfferStateEnum state = OfferStateEnum.Used, OfferSortingEnum sortingPrice = OfferSortingEnum.Relevance, int pageNumber = 1, string position = "", string isorigin = "", string enginetype = "", string transmissiontype = "", string tiresQuantity = "", string tiresWidth = "", string tiresHeight = "", string tiresRSize = "", string engineValue = "")
        {
            carManufactureName = carManufactureName != null ? carManufactureName.ToUpper() : carManufactureName;
            carModel = carModel != null ? carModel.ToUpper() : carModel;
            int offset = pageNumber == 1 ? 0 : pageNumber * 40;
            SearchOffersResponse searchOffersResponse;
            if (inputPartNumber == null)
            {
                searchOffersResponse = await _allegroPlClient.SearchOffersAsync(String.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}", carManufactureName, carModel, position, isorigin, enginetype, transmissiontype, tiresQuantity, tiresWidth, tiresHeight, tiresRSize, engineValue), null, selectedCategoryId, state, sortingPrice, offset, 40, System.Threading.CancellationToken.None, IsCategoryBody(selectedCategoryId));
            }
            else
            {
                searchOffersResponse = await _allegroPlClient.SearchOffersAsync(inputPartNumber, null, String.IsNullOrEmpty(selectedCategoryId) ? "3" : selectedCategoryId, state, sortingPrice, offset, 40, System.Threading.CancellationToken.None, IsCategoryBody(selectedCategoryId));
            }
            try
            {
                await _allegroPlClient.ApplyTranslations(_translateService, searchOffersResponse.Offers, String.Format("{0} {1}", carManufactureName, carModel), null, System.Threading.CancellationToken.None);
                //if (selectedCategoryId != "250847")
                //{
                //    await _allegroPlClient.ApplyTranslations(_translateService, searchOffersResponse.Offers, String.Format("{0} {1}", carManufactureName, carModel), null, System.Threading.CancellationToken.None);
                //}
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
        public IActionResult FilteredList(string state, string sorting, int page, string position, string isorigin, string enginetype, string transmissionType, string tiresQuantity, string tiresWidth, string tiresHeight, string tiresRSize, string engineValue)
        {
            if (engineValue == "undefined")
            {
                engineValue = null;
            }
            if (tiresQuantity == "undefined")
            {
                tiresQuantity = null;
            }
            if (tiresWidth == "undefined")
            {
                tiresWidth = null;
            }
            if (tiresHeight == "undefined")
            {
                tiresHeight = null;
            }
            if (tiresRSize == "undefined")
            {
                tiresRSize = null;
            }
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
                    var responceAllegro = GetAllegroProducts(rightTree.ManufacturerSelected, rightTree.ModelSelected, rightTree.SubChildCategoryId, null, stateEnum, sortingEnum, page, position, isorigin, enginetype, transmissionType, tiresQuantity, tiresWidth, tiresHeight, tiresRSize, engineValue).Result;
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
                    rightTree.SelectedEngineValue = engineValue;
                    rightTree.ScriptData = GetEcommerceScriptData(rightTree);
                    HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
                    HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(rightTree));
                    return View("_ProductsList", rightTree);

                case "PartNumberInput":
                    var responceAllegroNumberMode = GetAllegroProducts(null, null, null, rightTree.PartNumberValue, stateEnum, sortingEnum, page, position, isorigin, enginetype, transmissionType, tiresQuantity, tiresWidth, tiresHeight, tiresRSize, engineValue).Result;
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
                    rightTree.SelectedEngineValue = engineValue;
                    rightTree.ScriptData = GetEcommerceScriptData(rightTree);
                    HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegroNumberMode.Products));
                    HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(rightTree));
                    return View("_ProductsList", rightTree);


                case "ShowMainSubChildsCategories":
                    var responceAllegroSubCategories = GetAllegroProducts(rightTree.ManufacturerSelected, rightTree.ModelSelected, rightTree.SubCategoryId, null, stateEnum, sortingEnum, page, position, isorigin, enginetype, transmissionType, tiresQuantity, tiresWidth, tiresHeight, tiresRSize, engineValue).Result;
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
                    rightTree.SelectedEngineValue = engineValue;
                    rightTree.ScriptData = GetEcommerceScriptData(rightTree);
                    HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegroSubCategories.Products));
                    HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(rightTree));
                    return View("_ProductsList", rightTree);
            }
            rightTree.ScriptData = GetEcommerceScriptData(rightTree);
            return View("_ProductsList", rightTree);
        }
        //=============DEFAULT ACTIONS ==============
        public IActionResult Privacy()
        {
            return View();
        }
        public bool IsCategoryBody(string categoryId)
        {
            IList<string> bodyCategories = new List<string>() { "250542", "254548", "254699", "254580", "250542", "254559", "261280", "254683", "254659", "261282", "254718" };
            return bodyCategories.Contains(categoryId);
        }
        public static string? FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                return null;
            return input.First().ToString().ToUpper() + input.Substring(1);
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
        //(inputPartNumber, null, String.IsNullOrEmpty(selectedCategoryId)? "3" : selectedCategoryId, state, sortingPrice, offset, 40, System.Threading.CancellationToken.None, IsCategoryBody(selectedCategoryId));
        public IActionResult CustomSearch()
        {
            OfferStateEnum state = OfferStateEnum.Used;
            var responceAllegroSubCategories = GetAllegroProducts(null, null, "50828", "AUDI 100", state, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "", "").Result;
            if (responceAllegroSubCategories.Total == 0)
            {
                state = OfferStateEnum.New;
                responceAllegroSubCategories = GetAllegroProducts(null, null, "50828", "AUDI 100", state, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "", "").Result;
            }
            RightTreeViewModel rightTree = new RightTreeViewModel();
            rightTree.FilterName = GetFilterName(rightTree.SubCategoryId);
            rightTree.AllegroOfferList = responceAllegroSubCategories.Products;
            rightTree.FakeAllegroList = FakeListForPager(responceAllegroSubCategories.Total);
            rightTree.OfferState = state;
            rightTree.OfferSorting = OfferSortingEnum.Relevance;
            rightTree.PageNumber = 1;
            rightTree.OfferSortingPlacement = "";
            rightTree.OfferSortingIsOrigin = "";
            rightTree.OfferSortingEngineType = "";
            rightTree.OfferSortingTransmissionType = "";
            rightTree.SelectedEngineValue = "";
            rightTree.SelectedTiresSizes = new SelectedTires()
            {
                Height = null,
                Width = null,
                Quantity = null,
                RSize = null
            };
            rightTree.Tires = new TiresFilter()
            {
                Height = new TiresSizes().GetTiresHeight(),
                Width = new TiresSizes().GetTiresWidth(),
                RSize = new TiresSizes().GetTiresRSize(),
                Quantity = new TiresSizes().GetTiresCnt()
            };
            rightTree.EngineValues = new EngineValue().GetEngineValue();
            rightTree.ViewName = "_ProductsList";
            rightTree.ScriptData = GetEcommerceScriptData(rightTree);
            HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegroSubCategories.Products));
            HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(rightTree));
            return View(rightTree);
        }
        public static string HtmlEncode(string value)
        {
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }
        public string GetEcommerceScriptData(RightTreeViewModel model)
        {
            string script = "";
            foreach (var item in model.AllegroOfferList)
            {
                script = script + "{" + String.Format("'name': '{0}','id': '{1}','price': '{2}','category': '{3}', 'list': 'Search Results'", item.Name["pl"], item.SourceId, item.FinalPrice, model.SelectedCategoryName) + "},\n";
            }
            if (!String.IsNullOrEmpty(script))
            {
                return script.Remove(script.Length - 2);
            }
            return script;


        }
    }
}
