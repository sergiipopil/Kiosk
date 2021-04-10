using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Clients.AllegroPl.Models;
using KioskBrains.Common.EK.Api;

namespace WebApplication.Models
{
    public class ProductViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }   
        public string Name { get; set; }

        public string Price { get; set; }
        public List<string> Parameters { get; set; }
        public OfferImage[] Images
        {
            get;set;
        }
    }
}
