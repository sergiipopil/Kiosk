using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.EK.Api.CarTree;
using KioskBrains.Common.EK.Api;
using KioskBrains.Clients.AllegroPl.Models;
using WebApplication.Classes;
using X.PagedList;
using KioskBrains.Server.EK.Common.Helpers;

namespace WebApplication.Models
{
    public class RightTreeViewModel
    {
        public IList<string> AdvertCities {
            get {
                return EkCategoryHelper.GetAdvertCities();
            }
        }
        public IList<string> PartNumbersRandom
        {
            get
            {
                return EkCategoryHelper.GetRandomParts();
            }
        }
        public string ModelDescription { get; set; }
        public string ViewName { get; set; }
        public EkProduct[] AllegroOfferList { get; set; }
        public TiresFilter Tires { get; set; }
        public IList<string> EngineValues { get; set; }
        public string SelectedEngineValue { get; set; }
        public SelectedTires SelectedTiresSizes { get; set; }
        public List<string> FakeAllegroList { get; set; }
        public EkCarManufacturer[] ManufacturerList { get; set; }
        public IEnumerable<EkCarModel> ModelsList { get; set; }
        public bool IsModificationList { get; set; }
        public EkProductCategory[] ProductCategoryList { get; set; }
        public int PageNumber { get; set; }
        public string kioskId { get; set; }
        public string PartNumberValue { get; set; }
        public string ManufacturerSelected { get; set; }
        public string ModificationSelected { get; set; }
        public string ModelSelected { get; set; }
        public string MainCategoryId { get; set; }
        public string MainCategoryUrl { get; set; }
        public string MainCategoryName { get; set; }
        public string SubCategoryId { get; set; }
        public string SubCategoryUrl { get; set; }
        public string LastChildId { get; set; }
        public string LastChildName { get; set; }       
        public string SubCategoryName { get; set; }
        public string SubChildCategoryId { get; set; }
        public string SubChildCategoryUrl { get; set; }
        public string SubChildCategoryName { get; set; }
        public string FunctionReturnFromProducts { get; set; }
        public string ControllerName { get; set; }
        public string TopCategoryId { get; set; }
        public string ReallyTopCategoryId { get; set; }
        public OfferStateEnum OfferState { get; set; }
        public OfferSortingEnum OfferSorting { get; set; }
        public string OfferSortingPlacement { get; set; }
        public string OfferSortingIsOrigin { get; set; }
        public string OfferSortingEngineType { get; set; }
        public string OfferSortingTransmissionType { get; set; }
        public string FilterName { get; set; }

    }
}
