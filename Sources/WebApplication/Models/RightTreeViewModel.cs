using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.EK.Api.CarTree;
using KioskBrains.Common.EK.Api;
using KioskBrains.Clients.AllegroPl.Models;
using X.PagedList;

namespace WebApplication.Models
{
    public class RightTreeViewModel
    {
        public EkProduct[] AllegroOfferList { get; set; }
        public List<string> FakeAllegroList { get; set; }
        public EkCarManufacturer[] ManufacturerList { get; set; }
        public IEnumerable<string> ModelsList { get; set; }
        public EkProductCategory[] ProductCategoryList { get; set; }
        public int PageNumber { get; set; }
        public string PartNumberValue { get; set; }
        public string ManufacturerSelected { get; set; }
        public string ModelSelected { get; set; }
        public string MainCategoryId { get; set; }
        public string MainCategoryName { get; set; }
        public string SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string SubChildCategoryId { get; set; }
        public string SubChildCategoryName { get; set; }
        public string FunctionReturnFromProducts { get; set; }
        public string ControllerName { get; set; }
        public OfferStateEnum OfferState { get; set; }
        public OfferSortingEnum OfferSorting { get; set; }

    }
}
