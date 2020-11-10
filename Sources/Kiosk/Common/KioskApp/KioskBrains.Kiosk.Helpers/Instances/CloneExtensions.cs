using Newtonsoft.Json;

namespace KioskBrains.Kiosk.Helpers.Instances
{
    public static class CloneExtensions
    {
        public static TInstance CloneViaJsonSerialization<TInstance>(this TInstance baseInstance)
            where TInstance : class, new()
        {
            if (baseInstance == null)
            {
                return null;
            }

            var serializedInstance = JsonConvert.SerializeObject(baseInstance);
            var newInstance = JsonConvert.DeserializeObject<TInstance>(serializedInstance);
            return newInstance;
        }
    }
}