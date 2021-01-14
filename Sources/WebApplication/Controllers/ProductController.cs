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
            List<string> ImagePath = new List<string> { "https://lh3.googleusercontent.com/proxy/Su-lx1na8rBohFs8td-Ef0_5fSk3Y6skfH48zXFzpAc8_TBmhQktMSQmhBlPAATC_ksYwUrq4qKrHHOsGgIZ0DsdlsudvrtuP0YkknBkyucNfI26_fiK8acxqGuQDQ1o4EeD2ONJcu7F", "https://bugaga.ru/uploads/posts/2018-06/1527924361_kartinki-21.jpg" };
            
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
