using Newtonsoft.Json;

namespace KioskBrains.Common.EK.Api.CarTree
{
    public class EkCarManufacturer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CarImageURL { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        public EkCarModel[] CarModels { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Id}, {CarModels.Length} models)";
        }
    }
}