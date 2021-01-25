using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.NovaPoshtaUkraine.Models
{
    public class WarehouseSearchRequest : BaseSearchRequest
    {
        public MethodProperties methodProperties { get; set; }

        public WarehouseSearchRequest(string cityRef, int? limit, int? page)
            : base("AddressGeneral", "getSettlements")
        {
            methodProperties = new MethodProperties
            {
                //FindByString = "Одеська область",
                //Page=1
                //RegionRef= "db5c88d0-391c-11dd-90d9-001a92567626"
            };
        }

        public class MethodProperties
        {
            public string CityRef { get; set; }
            public string RegionRef { get; set; }
            public string FindByString { get; set; }
            public string CityName { get; set; }

            public int? Limit { get; set; }

            public int? Page { get; set; }

            public string Language { get; set; }
        }
    }
}
