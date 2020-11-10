namespace KioskApp.Clients.NovaPoshtaUkraine.Models
{
    public class WarehouseSearchRequest : BaseSearchRequest
    {
        public MethodProperties methodProperties { get; set; }

        public WarehouseSearchRequest(string cityRef, int? limit, int? page)
            : base("AddressGeneral", "getWarehouses")
        {
            methodProperties = new MethodProperties
                {
                    CityRef = cityRef,
                    Limit = limit,
                    Page = page,
                    Language = "ru",
                };
        }

        public class MethodProperties
        {
            public string CityRef { get; set; }

            public int? Limit { get; set; }

            public int? Page { get; set; }

            public string Language { get; set; }
        }
    }
}