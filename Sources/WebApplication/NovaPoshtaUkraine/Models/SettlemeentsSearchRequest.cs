using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.NovaPoshtaUkraine.Models
{
    public class SettlemeentsSearchRequest : BaseSearchRequest
    {
        public MethodProperties methodProperties { get; set; }

        public SettlemeentsSearchRequest(int? page)
            : base("AddressGeneral", "getSettlements")
        {
            methodProperties = new MethodProperties
            {
                Page=page,
                Warehouse="1"
            };
        }       
    }
}
