using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.NovaPoshtaUkraine.Models
{    public class BaseSearchResponse
    {
        public bool success { get; set; }

        public string[] errors { get; set; }
    }
}
