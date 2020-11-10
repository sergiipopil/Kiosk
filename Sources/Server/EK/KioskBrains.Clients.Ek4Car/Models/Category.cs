using KioskBrains.Common.EK.Api;

namespace KioskBrains.Clients.Ek4Car.Models
{
    public class Category
    {
        public int Id { get; set; }

        public int? ParentCategoryId { get; set; }

        public int Order { get; set; }

        public MultiLanguageString Name { get; set; }
    }
}