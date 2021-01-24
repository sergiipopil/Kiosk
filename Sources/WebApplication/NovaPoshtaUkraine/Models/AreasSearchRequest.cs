using WebApplication.NovaPoshtaUkraine.Models;

namespace WebApplication.NovaPoshtaUkraine.Models
{
    public class AreasSearchRequest : BaseSearchRequest
    {
        public AreasSearchItem methodProperties { get; set; }

        public AreasSearchRequest(string refArea, string desc, string areasCenter)
           : base("Address", "getAreas")
        {
            methodProperties = new AreasSearchItem
            {
                Ref = refArea,
                Description = desc,
                AreasCenter = areasCenter
            };
        }
    }
}
