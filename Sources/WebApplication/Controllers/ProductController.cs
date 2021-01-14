using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Clients.AllegroPl.Rest;
using KioskBrains.Common.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
namespace WebApplication.Controllers
{
    public class ProductController : Controller
    {
        // GET: ProductController
        public ActionResult Index()
        {            
            return View();
        }

        // GET: ProductController/Details/5
        public ActionResult Details(string id)
        {            
            var restClient = new RestClient("", "");
            var p = restClient.GetExtraDataInit(id);
            List<string> ImagePath = new List<string> { "https://9.allegroimg.com/original/030a63/6d2953e8451eb54c45e7d93f8fa9/BLOTNIK-VW-GOLF-4-IV-KOLOR-LB9A-nowy", "https://a.allegroimg.com/original/03f97b/90520c994c718dab243caef5f9b3/Blotnik-VW-Golf-3-III-Dowolny-Kolor-Lewy-Nowy" };
            
            var product = new ProductViewModel() {Id = id,  Description = p.Description[Languages.PolishCode], Images = ImagePath };
            return View(product);
        }

        // GET: ProductController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
