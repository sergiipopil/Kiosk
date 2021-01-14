using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models
{
    public class ProductViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        List<string> _images = new List<string>();

        public ProductViewModel()
        {
            _images = new List<string>();
        }

        public List<string> Images
        {
            get { return _images; }
            set { _images = value; }
        }
    }
}
