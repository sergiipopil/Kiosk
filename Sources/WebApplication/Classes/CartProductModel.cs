using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.EK.Api;

namespace WebApplication.Classes
{
    public class CartProductModel
    {
        public IList<CartProduct> Products { get; set; }
        public string ScriptData { get; set; }
        public string ScriptDataDelete { get; set; }
    }
}
