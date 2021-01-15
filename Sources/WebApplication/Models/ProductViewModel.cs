using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Clients.AllegroPl.Models;

namespace WebApplication.Models
{
    public class ProductViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }   
        public string Name { get; set; }

        public OfferImage[] Images
        {
            get;set;
        }
    }
}
