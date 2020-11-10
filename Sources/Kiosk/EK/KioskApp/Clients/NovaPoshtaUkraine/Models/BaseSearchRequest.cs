namespace KioskApp.Clients.NovaPoshtaUkraine.Models
{
    public class BaseSearchRequest
    {
        public string apiKey { get; set; }

        public string modelName { get; set; }

        public string calledMethod { get; set; }

        public BaseSearchRequest(string modelName, string calledMethod)
        {
            this.modelName = modelName;
            this.calledMethod = calledMethod;
        }
    }
}