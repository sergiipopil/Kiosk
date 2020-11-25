using System;
using System.Collections.Generic;
using System.Text;

namespace KioskBrains.Clients.AllegroPl.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Category Parent { get; set; }
    }
}
