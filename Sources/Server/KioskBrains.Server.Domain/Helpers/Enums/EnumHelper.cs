using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using KioskBrains.Server.Domain.Actions.Common.Models;

namespace KioskBrains.Server.Domain.Helpers.Enums
{
    public static class EnumHelper
    {
        private static readonly ConcurrentDictionary<Type, ListOptionInt[]> Cache = new ConcurrentDictionary<Type, ListOptionInt[]>();

        public static ListOptionInt[] ToListOptions<TEnum>()
        {
            var enumType = typeof(TEnum);
            if (Cache.TryGetValue(enumType, out var result))
            {
                return result;
            }

            var options = new List<ListOptionInt>();
            foreach (var enumValue in Enum.GetValues(enumType).Cast<int>())
            {
                var enumName = Enum.GetName(enumType, enumValue);
                var enumField = enumType.GetField(enumName);
                var shouldIgnore = enumField.GetCustomAttribute<IgnoreAttribute>();
                if (shouldIgnore == null)
                {
                    var enumFieldDisplayAttribute = enumField.GetCustomAttribute<DisplayAttribute>();

                    var enumDisplayName = enumFieldDisplayAttribute == null
                        ? enumName
                        : enumFieldDisplayAttribute.Name;

                    options.Add(new ListOptionInt
                        {
                            Value = enumValue,
                            DisplayName = enumDisplayName
                        });
                }
            }

            result = options.ToArray();
            Cache[enumType] = result;

            return result;
        }

        // todo: add caching!!!
        public static string ToEnumDisplayName<TEnum>(this TEnum value)
        {
            var enumType = value.GetType();
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("The method is intended for Enum values only.", nameof(value));
            }

            var enumName = Enum.GetName(enumType, value) ?? string.Empty;
            var enumField = enumType.GetField(enumName);
            var enumFieldDisplayAttribute = enumField?.GetCustomAttribute<DisplayAttribute>();
            return enumFieldDisplayAttribute == null
                ? enumName
                : enumFieldDisplayAttribute.Name;
        }
    }
}