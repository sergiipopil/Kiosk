using Newtonsoft.Json;

namespace KioskBrains.Common.Contracts
{
    public class JsonDefaultSettings
    {
        public const string DateFormatString = "yyyy-MM-dd HH:mm:ss.ffffff";

        public static JsonSerializerSettings GetDefaultSettings()
        {
            return new JsonSerializerSettings()
                {
                    DateFormatString = DateFormatString,
                    ContractResolver = new CamelCaseExceptDictionaryKeysResolver(),
                };
        }

        public static void Initialize()
        {
            JsonConvert.DefaultSettings = GetDefaultSettings;
        }
    }
}