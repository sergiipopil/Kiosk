using Newtonsoft.Json;

namespace KioskBrains.Common.KioskConfiguration
{
    public class KioskConfiguration
    {
        public int KioskId { get; set; }

        public ComponentConfiguration[] AppComponents { get; set; }

        public string SupportPhone { get; set; }

        public string[] LanguageCodes { get; set; }

        public KioskAddress KioskAddress { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SpecificSettingsJson { get; set; }
    }
}