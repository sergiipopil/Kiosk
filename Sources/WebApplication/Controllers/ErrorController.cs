using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Error()
        {
            return View("Error");
        }
    }
}
