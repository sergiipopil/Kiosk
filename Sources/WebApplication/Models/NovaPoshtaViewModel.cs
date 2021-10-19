using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.NovaPoshtaUkraine.Models;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models
{
    public class NovaPoshtaViewModel
    {
        public string ScriptData { get; set; }
        public List<WarehouseSearchItem> Cities { get; set; }
        
        public WarehouseSearchItem[] Departments { get; set; }
        
        public AreasSearchItem[] Areas { get; set; }

        [Required(ErrorMessage = "* Обязательное поле")]
        public AreasSearchItem SelectedArea { get; set; }
        [Required(ErrorMessage = "* Обязательное поле")]
        public WarehouseSearchItem SelectedCity { get; set; }
        [Required(ErrorMessage = "* Обязательное поле")]
        public WarehouseSearchItem SelectedDepartment { get; set; }
        public IList<WebApplication.Classes.CartProduct> CartProducts { get; set; }

        [Required(ErrorMessage = "* Обязательное поле")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "* Обязательное поле")]
        public string CustomerSurName { get; set; }

        [Required(ErrorMessage = "* Обязательное поле")]
        public string CustomerFatherName { get; set; }

        [Required(ErrorMessage = "* Обязательное поле")]
        public string CustomerPhoneNumber { get; set; }
        public bool isCourier { get; set; }

        [Required(ErrorMessage = "* Обязательное поле")]
        public string City { get; set; }
        [Required(ErrorMessage = "* Обязательное поле")]
        public string Address { get; set; }
    }
}
