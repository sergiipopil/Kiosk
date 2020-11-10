using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace KioskBrains.Server.Domain.Helpers.DbContext
{
    public class JsonValueConverter<TData> : ValueConverter<TData, string>
    {
        public JsonValueConverter()
            : base(
                // no need to handle 'null' since it's not passed to value converters
                modelValue => JsonConvert.SerializeObject(modelValue),
                providerValue => JsonConvert.DeserializeObject<TData>(providerValue))
        {
        }
    }
}