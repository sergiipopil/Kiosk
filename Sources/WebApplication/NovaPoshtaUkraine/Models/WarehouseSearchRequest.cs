﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.NovaPoshtaUkraine.Models
{
    public class WarehouseSearchRequest : BaseSearchRequest
    {
        public MethodProperties methodProperties { get; set; }

        public WarehouseSearchRequest(string cityName)
            : base("AddressGeneral", "getWarehouses")
        {
            methodProperties = new MethodProperties
            {
                CityName=cityName
            };
        }

       
    }
}