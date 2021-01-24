using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.NovaPoshtaUkraine.Models
{
    public class AreasSearchResponse : BaseSearchResponse
    {
        public AreasSearchItem[] data { get; set; }
    }
}
