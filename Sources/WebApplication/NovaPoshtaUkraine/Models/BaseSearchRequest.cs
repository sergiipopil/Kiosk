using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.NovaPoshtaUkraine.Models
{    public class BaseSearchRequest
    {
        public string apiKey { get; set; }

        public string modelName { get; set; }

        public string calledMethod { get; set; }

        public BaseSearchRequest(string modelName, string calledMethod)
        {
            this.modelName = modelName;
            this.calledMethod = calledMethod;
        }
    }
}
