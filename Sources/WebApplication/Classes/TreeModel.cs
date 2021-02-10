using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.EK.Api;

namespace WebApplication.Classes
{
    public class TreeModel
    {
        public string Manufacturer { get; set; }
        public IEnumerable<string> Models { get; set; }
        public string MainCategoryId { get; set; }
        public EkProductCategory[] productCategory { get; set; }
    }
}
