using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.NovaPoshtaUkraine.Models;

namespace WebApplication.Models
{
    public class NovaPoshtaViewModel
    {
        public List<WarehouseSearchItem> Cities { get; set; }
        public WarehouseSearchItem[] Departments { get; set; }
        public AreasSearchItem[] Areas { get; set; }
        public AreasSearchItem SelectedArea { get; set; }
        public WarehouseSearchItem SelectedCity { get; set; }
        public WarehouseSearchItem SelectedDepartment { get; set; }
        public IList<WebApplication.Classes.CartProduct> CartProducts { get; set; }
        public string CustomerFullName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public bool isCourier { get; set; }

        public string City { get; set; }
        public string Address { get; set; }
    }
}
