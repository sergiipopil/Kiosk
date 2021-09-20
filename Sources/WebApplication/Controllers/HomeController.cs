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
using System.Net.Http;
using System.Web;
using System.IO;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

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
            //HtmlWeb web = new HtmlWeb();

            //HtmlDocument doc = web.Load("https://zapchasti.ria.com/uk/map-catalog-number/audi/");
            //var text = doc.ParsedText;

            //var divsDescNew = doc.DocumentNode.QuerySelectorAll("a.elem");



            TitleController titleController = new TitleController();
            titleController.GetTitleSite();
            string cookieValueFromReq = Request.Cookies["kioskId"];
            string _topCategoryId = HttpContext.Session.GetString("topCategoryId");
            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddMonths(1);


            Response.Cookies.Append("kioskId", "777", option);

            HttpContext.Session.Remove("topCategoryId");
            if (kioskId == null)
            {
                kioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            }
            HttpContext.Session.SetString("kioskId", kioskId == null ? "116" : kioskId);
            ViewData["CartWidgetPrice"] = HttpContext.Session.GetString("cartWidgetPrice");

            string carTemp = "https://bi-bi.com.ua/topcategory/620/";
            
            _topCategoryCarType = new EkSiteFactory().GetCarTypeEnum(String.IsNullOrEmpty(_topCategoryId) ? "620" : _topCategoryId);
            var carTreeTest = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            string pathSitemap = @"c:\temp\sitemap.xml";
            /*foreach (var item in carTreeTest)
            {
                string carMap = carTemp + item.Name;//bibi/topcategory/620/audi
                string text123 = String.Format("\t<url>\n\t\t<loc>{0}</loc>\n\t\t<lastmod>2021-09-09</lastmod>\n\t\t<changefreq>daily</changefreq>\n\t\t<priority>0.9</priority>\n\t</url>\n", carMap);
                
                System.IO.File.AppendAllText(pathSitemap, text123);//carTemp + item.Name);
                var modelTree123 = carTreeTest.Where(x => x.Name == item.Name).Select(y => y.CarModels).FirstOrDefault();
                foreach (var item1 in modelTree123)
                {
                    string modelMap = carMap + "/" + item1.Name;
                    string textModels = String.Format("\t<url>\n\t\t<loc>{0}</loc>\n\t\t<lastmod>2021-09-09</lastmod>\n\t\t<changefreq>daily</changefreq>\n\t\t<priority>0.9</priority>\n\t</url>\n", modelMap);
                    System.IO.File.AppendAllText(pathSitemap, textModels);//carTemp + item.Name);
                    var autoParts123 = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == "620").FirstOrDefault().Children;
                    foreach (var itemPart in autoParts123)
                    {
                        if (itemPart.Children != null && !itemPart.CategoryId.Contains("GROUP-"))
                        {
                            itemPart.CategoryId = "GROUP-" + itemPart.CategoryId;
                        }
                        string mainCategoryMap = modelMap + "/" + itemPart.CategoryId + "/" + itemPart.Name.GetValue("ru").Replace(" ", "-");
                        string textMainCategory = String.Format("\t<url>\n\t\t<loc>{0}</loc>\n\t\t<lastmod>2021-09-09</lastmod>\n\t\t<changefreq>daily</changefreq>\n\t\t<priority>0.9</priority>\n\t</url>\n", mainCategoryMap);
                        System.IO.File.AppendAllText(pathSitemap, textMainCategory);//carTemp + item.Name);

                        var autoPartsSubCategories = autoParts123.Where(x => x.CategoryId == itemPart.CategoryId).Select(x => x.Children).FirstOrDefault();
                        if (autoPartsSubCategories != null)
                        {
                            foreach (var itemSubCat in autoPartsSubCategories)
                            {
                                if (itemSubCat.Children != null && !itemSubCat.CategoryId.Contains("GROUP-"))
                                {
                                    itemSubCat.CategoryId = "GROUP-" + itemSubCat.CategoryId;
                                }
                                string subCategoryMap = mainCategoryMap + "/" + itemSubCat.CategoryId + "/" + itemSubCat.Name.GetValue("ru").Replace(" ", "-");
                                string textSubCategory = String.Format("\t<url>\n\t\t<loc>{0}</loc>\n\t\t<lastmod>2021-09-09</lastmod>\n\t\t<changefreq>daily</changefreq>\n\t\t<priority>0.9</priority>\n\t</url>\n", subCategoryMap);
                                System.IO.File.AppendAllText(pathSitemap, textSubCategory);//carTemp + item.Name);


                                var autoPartsSubChildCategories = autoPartsSubCategories.Where(x => x.CategoryId == itemSubCat.CategoryId).Select(x => x.Children).FirstOrDefault();
                                if (autoPartsSubChildCategories != null)
                                {
                                    foreach (var itemsubChild in autoPartsSubChildCategories)
                                    {
                                        string subChildCategoryMap = subCategoryMap + "/" + itemsubChild.CategoryId + "/" + itemsubChild.Name.GetValue("ru").Replace(" ", "-");
                                        string textSubChildCategory = String.Format("\t<url>\n\t\t<loc>{0}</loc>\n\t\t<lastmod>2021-09-09</lastmod>\n\t\t<changefreq>daily</changefreq>\n\t\t<priority>0.9</priority>\n\t</url>\n", subChildCategoryMap);
                                        System.IO.File.AppendAllText(pathSitemap, textSubChildCategory);//carTemp + item.Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }*/
            /*int counter = 0;
            string line;

            // Read the file and display it line by line.  
            System.IO.StreamReader file =
                new System.IO.StreamReader(pathSitemap);
            while ((line = file.ReadLine()) != null)
            {                
                counter++;
                if (counter > 0 && counter <= 252000) {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap1.xml", line + "\n");
                }
                if (counter > 252000 && counter <= 504000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap2.xml", line + "\n");
                }
                if (counter > 504000 && counter <= 756000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap3.xml", line + "\n");
                }
                if (counter > 756000 && counter <= 1008000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap4.xml", line + "\n");
                }
                if (counter > 1008000 && counter <= 1260000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap5.xml", line + "\n");
                }
                if (counter > 1260000 && counter <= 1512000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap6.xml", line + "\n");
                }
                if (counter > 1512000 && counter <= 1764000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap7.xml", line + "\n");
                }
                if (counter > 1764000 && counter <= 2016000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap8.xml", line + "\n");
                }
                if (counter > 2016000 && counter <= 2268000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap9.xml", line + "\n");
                }
                if (counter > 2268000 && counter <= 2520000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap10.xml", line + "\n");
                }
                if (counter > 2520000 && counter <= 2772000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap11.xml", line + "\n");
                }
                if (counter > 2772000 && counter <= 3024000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap12.xml", line + "\n");
                }
                if (counter > 3024000 && counter <= 3276000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap13.xml", line + "\n");
                }
                if (counter > 3276000 && counter <= 3528000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap14.xml", line + "\n");
                }
                if (counter > 3528000)
                {
                    System.IO.File.AppendAllText(@"c:\temp\sitemap15.xml", line + "\n");
                }
            }

            file.Close();
            */
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            var _topCategoryTempId = HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId");
            if (RouteData.Values["topcategory"] != null && RouteData.Values["topcategory"].ToString().ToLower() == "search")
            {
                RightTreeViewModel rightTree = GetNumberInput(RouteData.Values["category"].ToString());
                return View(rightTree);
            }
            if (RouteData.Values["carmanufacture"] != null && RouteData.Values["carmodel"] == null)
            {
                var carManufactureName = RouteData.Values["carmanufacture"].ToString().ToUpper();
                ViewBag.TitleText = "Купить автозапчасти на " + carManufactureName;
                var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault();
                var modelDescription = carTree.Where(x => x.Name == carManufactureName).Select(y => y.description).FirstOrDefault();
                return View(new RightTreeViewModel() { ViewName = "_CarModels", ModelDescription = modelDescription, ManufacturerSelected = carManufactureName, ModelsList = modelTree, TopCategoryId = HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId"), kioskId = "116" });
            }

            if (RouteData.Values["carmodel"] != null && RouteData.Values["maincategoryname"] == null)
            {
                var carManufactureName = RouteData.Values["carmanufacture"].ToString().ToUpper();
                var carmodel = RouteData.Values["carmodel"].ToString().ToUpper().Replace("-", " ");
                var modelTree = carTree.Where(x => x.Name == carManufactureName).Select(y => y.CarModels).FirstOrDefault();
                if (modelTree.Where(x => x.Name == carmodel).Select(y => y.Children).FirstOrDefault() == null)
                {
                    RightTreeViewModel rightTree = GetCategoryAutoParts(carManufactureName, carmodel, null, _topCategoryTempId);
                    return View(rightTree);
                }
                else
                {
                    RightTreeViewModel rightTree = GetModelModifications(carManufactureName, carmodel, _topCategoryTempId);
                    return View(rightTree);
                }
            }
            if (RouteData.Values["carmodel"] != null && RouteData.Values["maincategoryname"] != null && RouteData.Values["subcategoryid"] == null)
            {
                var carManufactureName = RouteData.Values["carmanufacture"].ToString().ToUpper();
                var carmodel = RouteData.Values["carmodel"].ToString().ToUpper().Replace("-", " ");
                var mainCategoryId = RouteData.Values["maincategoryid"].ToString().ToUpper();
                var mainCategoryName = RouteData.Values["maincategoryname"].ToString().Replace("-", " ");
                mainCategoryName = FirstCharToUpper(mainCategoryName);
                RightTreeViewModel rightTree = GetMainSubcategories(carManufactureName, carmodel, mainCategoryId, mainCategoryName, _topCategoryTempId);
                return View(rightTree);
            }
            if (RouteData.Values["carmodel"] != null && RouteData.Values["maincategoryname"] != null && RouteData.Values["subcategoryid"] != null && RouteData.Values["subchildcategoryid"] == null)
            {
                var carManufactureName = RouteData.Values["carmanufacture"].ToString().ToUpper();
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
                RightTreeViewModel rightTree = GetMainSubChildsCategories(carManufactureName, carmodel, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName, _topCategoryTempId);
                return View(rightTree);
            }
            if (RouteData.Values["carmodel"] != null && RouteData.Values["maincategoryname"] != null && RouteData.Values["subcategoryid"] != null && RouteData.Values["subchildcategoryid"] != null && RouteData.Values["lastcategoryid"] == null)
            {
                var carManufactureName = RouteData.Values["carmanufacture"].ToString().ToUpper();
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

                RightTreeViewModel rightTree = GetProductList(carManufactureName, carmodel, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName, subChildId, subChildName, lastChildId, lastChildName, _topCategoryTempId); ;
                return View(rightTree);
            }

            return View(new RightTreeViewModel() { ViewName = "_CarTree", ManufacturerList = carTree, kioskId = HttpContext.Session.GetString("kioskId"), TopCategoryId = HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId") });
        }

        //====================METHOD TO SWITCH TYPE CAR TOP CATEGORY ======================================
        public IActionResult ShowMainView(string topCategoryId)
        {
            _topCategoryCarType = new EkSiteFactory().GetCarTypeEnum(topCategoryId);
            HttpContext.Session.SetString("topCategoryId", topCategoryId == null ? "620" : topCategoryId);
            if (topCategoryId == "99193" || topCategoryId == "18554")
            {
                var tempC = EkCategoryHelper.GetEuropeCategories().Where(x => x.CategoryId == topCategoryId).FirstOrDefault().Children;
                return View("_AutoPartsTree", new RightTreeViewModel() { ProductCategoryList = tempC, TopCategoryId = topCategoryId, ReallyTopCategoryId = (HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId")) });
            }
            var carTree = EkCategoryHelper.GetCarModelTree().Where(x => x.CarType == _topCategoryCarType).Select(x => x.Manufacturers).FirstOrDefault();
            string tempKioskId = String.IsNullOrEmpty(HttpContext.Session.GetString("kioskId")) ? "116" : HttpContext.Session.GetString("kioskId");
            return View("_CarTree", new RightTreeViewModel() { ManufacturerList = carTree, TopCategoryId = topCategoryId, kioskId = tempKioskId });
        }
        //====================METHOD TO SHOW CAR MODELS ======================================

        public IActionResult SelectManufactureAndModel(string carmanufacture, string topcategoryid)
        {

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
            ViewBag.TitleText = "Купить автозапчасти на " + carManufactureName + " " + carModel;
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
            ViewBag.TitleText = "Авто Разборка Шрот Купить Запчасти Б/У и Новые " + carManufactureName + " " + carModel;
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
            ViewBag.TitleText = mainCategoryName + " " + carManufactureName + " " + carModel;
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
                if (responceAllegro.Total < 1)
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

                HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeViewModel));
                ViewBag.TitleText = subCategoryName + " " + carManufactureName + " " + carModel;
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
            ViewBag.TitleText = subCategoryName + " " + carManufactureName + " " + carModel;
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
                if (responceAllegro.Total < 1)
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
            autoPartsSubChildCategories = autoPartsSubChildCategories.Where(x => x.CategoryId == subCategoryId).Select(x => x.Children).FirstOrDefault();
            var tempCat = autoPartsSubChildCategories.Where(x => x.CategoryId == subChildId).Select(x => x.Children).FirstOrDefault();
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
            if (responceAllegro.Total < 1)
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
                ReallyTopCategoryId = HttpContext.Session.GetString("topCategoryId") == "621" ? "621" : (HttpContext.Session.GetString("topCategoryId") == null ? "620" : HttpContext.Session.GetString("topCategoryId")),
                OfferState = isNew ? OfferStateEnum.New : OfferStateEnum.Used,
                FunctionReturnFromProducts = String.Format("selectSubMainCategory('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')", carManufactureName, carModel, mainCategoryId, mainCategoryName, subCategoryId, subCategoryName, tempKioskId, topcategoryid),
                AllegroOfferList = responceAllegro.Products,
                ControllerName = "ShowProductList",
                FakeAllegroList = FakeListForPager(responceAllegro.Total),
                OfferSorting = OfferSortingEnum.Relevance
            };
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

            var tempCat = autoPartsSubChildCategories.Where(x => x.CategoryId == subChildId).Select(x => x.Children).FirstOrDefault();
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
                ViewBag.TitleText = String.Format("{0} {1} {2}", subCategoryName, carManufactureName, carModel);
                return treeViewLast;
            }
            var responceAllegro = GetAllegroProducts(carManufactureName, carModel, String.IsNullOrEmpty(lastChildId) ? subChildId : lastChildId, null, OfferStateEnum.Used, OfferSortingEnum.Relevance, 1, "", "", "", "", "", "", "", "").Result;
            bool isNew = false;
            if (responceAllegro.Total < 1)
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
                FakeAllegroList = FakeListForPager(responceAllegro.Total),
                OfferSorting = OfferSortingEnum.Relevance
            };
            ViewBag.TitleText = String.Format("{0} {1} {2}", String.IsNullOrEmpty(lastChildName) ? subChildName : lastChildName, carManufactureName, carModel);
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
            HttpContext.Session.SetString("rightTreeViewModel", JsonSerializer.Serialize(treeView));
            HttpContext.Session.SetString("productList", JsonSerializer.Serialize(responceAllegro.Products));
            return View("_ProductsList", treeView);

        }
        public RightTreeViewModel GetNumberInput(string partNumber)
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
                PageNumber = 1,
                ViewName = "_ProductsList"
            };
            ViewBag.TitleText = "Купить " + partNumber;
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
    }
}
