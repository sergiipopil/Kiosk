using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
    public class TitleController:Controller
    {
        public IActionResult GetTitleSite()
        {
            ViewData["TitleText"] = "1234567";
            return PartialView("TitleSite");
        }
    }
}
