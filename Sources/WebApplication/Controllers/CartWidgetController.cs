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
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication.Controllers
{
    public class CartWidgetController : Controller
    {
        public IActionResult CartWidget()
        {            
            return PartialView("CartWidget");
        }
    }
}
