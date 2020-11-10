namespace KioskBrains.Clients.ElitUa
{
    public class ElitPriceListRecord
    {
        public string ActiveItemNo { get; set; }

        public string PartNumber { get; set; }

        public string Brand { get; set; }

        public string ItemDescription { get; set; }

        public string EcatDescription { get; set; }

        public decimal CustomerPrice { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ActiveItemNo)
                   && !string.IsNullOrEmpty(PartNumber)
                   // TDB: probably brand is not critical (by this reason ~1K from 135K of records are not valid)
                   && !string.IsNullOrEmpty(Brand)
                   && (!string.IsNullOrEmpty(ItemDescription)
                       || !string.IsNullOrEmpty(EcatDescription))
                   && CustomerPrice > 0;
        }

        public override string ToString()
        {
            return $"{PartNumber} {Brand} {EcatDescription} {CustomerPrice}";
        }
    }
}