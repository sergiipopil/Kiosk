using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Clients.AllegroPl.Models;
using KioskBrains.Common.EK.Api.CarTree;

namespace WebApplication.Classes
{
    public class EkSiteFactory
    {
        public EkCarTypeEnum GetCarTypeEnum(string categoryId)
        {
            switch (categoryId)
            {
                case "620":
                    return EkCarTypeEnum.Car;
                case "621":
                    return EkCarTypeEnum.Truck;
                case "622":
                    return EkCarTypeEnum.Bus;
                case "156":
                    return EkCarTypeEnum.Moto;
                case "99022":
                    return EkCarTypeEnum.Special;               
                default:
                    return EkCarTypeEnum.Car;
            }
        }
       
        public OfferStateEnum GetStateEnumValue(string state)
        {
            switch (state)
            {
                case "All":
                    return OfferStateEnum.All;
                case "New":
                    return OfferStateEnum.New;
                case "Used":
                    return OfferStateEnum.Used;
                case "Recovered":
                    return OfferStateEnum.Recovered;
                default:
                    return OfferStateEnum.All;
            }
        }

        public OfferSortingEnum GetSortingEnumValue(string sorting)
        {
            switch (sorting)
            {
                case "Relevance":
                    return OfferSortingEnum.Relevance;
                case "PriceDesc":
                    return OfferSortingEnum.PriceDesc;
                case "PriceAsc":
                    return OfferSortingEnum.PriceAsc;
                default:
                    return OfferSortingEnum.Relevance;
            }
        }
    }
}
