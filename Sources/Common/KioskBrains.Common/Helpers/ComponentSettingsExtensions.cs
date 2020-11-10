using System;
using System.Collections.Generic;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.KioskConfiguration;

namespace KioskBrains.Common.Helpers
{
    public static class ComponentSettingsExtensions
    {
        public static TSetting Get<TSetting>(this ComponentSettings componentSettings, string settingName, bool mandatory = true)
        {
            Assure.ArgumentNotNull(settingName, nameof(settingName));

            var value = componentSettings.GetValueOrDefault(settingName);
            if (value == null)
            {
                if (mandatory)
                {
                    throw new KeyNotFoundException($"Setting '{settingName}' was not found.");
                }

                return default(TSetting);
            }

            var valueType = typeof(TSetting);
            if (valueType == typeof(string))
            {
                return (TSetting)(object)value;
            }

            var nullableUnderlyingType = Nullable.GetUnderlyingType(valueType);
            if (nullableUnderlyingType != null)
            {
                valueType = nullableUnderlyingType;
            }

            return (TSetting)Convert.ChangeType(value, valueType);
        }

        private static string GetValueOrDefault(this ComponentSettings componentSettings, string key)
        {
            Assure.ArgumentNotNull(key, nameof(key));

            if (componentSettings == null)
            {
                return null;
            }

            return componentSettings.ContainsKey(key)
                ? componentSettings[key]
                : null;
        }
    }
}