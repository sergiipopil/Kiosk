using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.EK.Api.CarTree;

namespace WebApplication.Classes
{
    public class GetCarTypeEnumByNumber
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
    }
}
