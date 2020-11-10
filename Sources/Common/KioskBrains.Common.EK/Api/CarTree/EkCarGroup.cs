namespace KioskBrains.Common.EK.Api.CarTree
{
    public class EkCarGroup
    {
        public EkCarTypeEnum CarType { get; set; }

        public EkCarManufacturer[] Manufacturers { get; set; }

        public override string ToString()
        {
            return $"{CarType} ({Manufacturers.Length})";
        }
    }
}