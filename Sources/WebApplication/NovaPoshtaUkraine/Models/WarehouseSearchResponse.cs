using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.NovaPoshtaUkraine.Models
{    public class WarehouseSearchResponse : BaseSearchResponse
    {
        public WarehouseSearchItem[] data { get; set; }
    }
}
