using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.EK.Api;

namespace WebApplication.Classes
{
    public class CartProduct
    {
        public EkProduct Product { get; set; }
        public int Quantity { get; set; }        
    }
}
