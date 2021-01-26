using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.NovaPoshtaUkraine.Models;

namespace WebApplication.Models
{
    public class NovaPoshtaViewModel
    {
        public List<WarehouseSearchItem> WareHouses { get; set; }
        public AreasSearchItem[] Areas { get; set; }
    }
}
