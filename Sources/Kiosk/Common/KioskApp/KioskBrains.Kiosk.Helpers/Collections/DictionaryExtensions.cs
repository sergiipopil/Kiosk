using System.Collections.Generic;

namespace KioskBrains.Kiosk.Helpers.Collections
{
    public static class DictionaryExtensions
    {
        public static void Update<TKey, TValue>(this Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> newValues)
        {
            foreach (var newValue in newValues)
            {
                source[newValue.Key] = newValue.Value;
            }
        }
    }
}