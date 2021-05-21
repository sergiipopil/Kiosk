using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Classes
{
    public class TiresFilter
    {
        public IList<string> RSize { get; set; }
        public IList<string> Quantity { get; set; }
        public IList<string> Width { get; set; }
        public IList<string> Height { get; set; }
    }
}
