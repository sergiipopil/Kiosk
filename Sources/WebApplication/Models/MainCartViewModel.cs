using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.EK.Api;

namespace WebApplication.Models
{
    public class MainCartViewModel
    {
        public IList<EkProduct> AllSelectedProducts { get; set; }
        public EkProduct SelectedProduct { get; set; }
        public RightTreeViewModel AllTreeData { get; set; }
    }
}
