namespace KioskBrains.Common.EK.Api.CarTree
{
    public class EkCarModel
    {
        public TecDocTypeEnum TecDocType { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string SelectedManufactureURL { get; set; }

        public string SelectedModelURL { get; set; }

        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }

        public EkCarTypeEnum CarType { get; set; }

        public string Path { get; set; }

        public override string ToString()
        {
            return $"{Name} ({TecDocType} {Id})";
        }
    }
}