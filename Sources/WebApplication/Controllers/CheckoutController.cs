using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class CheckoutController : Controller
    {
        [HttpPost]
        public IActionResult Delivery(NovaPoshtaViewModel model)
        {
            string s = model.CustomerFullName;
            return View(model);
        }
    }
}
