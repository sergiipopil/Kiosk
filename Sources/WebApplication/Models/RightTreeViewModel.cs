using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.EK.Api.CarTree;

namespace WebApplication.Models
{
    public class RightTreeViewModel
    {
        public EkCarManufacturer[] manufacturer { get; set; }

        public string PartNumberValue { get; set; }
    }
}
