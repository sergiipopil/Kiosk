namespace KioskBrains.Common.EK.Api
{
    public class EkProductCategory
    {
        public string CategoryId { get; set; }

        public MultiLanguageString Name { get; set; }

        public EkProductCategory[] Children { get; set; }

        public override string ToString()
        {
            return $"{Name} ({CategoryId}) - {Children?.Length ?? 0} subcategories";
        }
    }
}