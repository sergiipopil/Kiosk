using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KioskBrains.Common.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KioskBrains.Clients.TecDocWs.Models
{
    /// <summary>
    /// Converter to fix errors with arrays.
    /// </summary>
    internal class BrokenJsonArrayConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.ReadFrom(reader);
            var valueType = objectType.GetElementType();
            var toArrayMethod = GetToArrayMethod(valueType);
            return toArrayMethod.Invoke(null, new object[] { jToken, serializer });
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsArray;
        }

        private static readonly Dictionary<Type, MethodInfo> ToArrayMethods = new Dictionary<Type, MethodInfo>();

        private static readonly object _toArrayMethodsLocker = new object();

        private static MethodInfo GetToArrayMethod(Type valueType)
        {
            Assure.ArgumentNotNull(valueType, nameof(valueType));

            lock (_toArrayMethodsLocker)
            {
                var method = ToArrayMethods.GetValueOrDefault(valueType);
                if (method == null)
                {
                    var genericMethod = typeof(BrokenJsonArrayConverter).GetMethod(nameof(ToArray), BindingFlags.Static | BindingFlags.NonPublic);
                    method = genericMethod.MakeGenericMethod(valueType);
                    ToArrayMethods[valueType] = method;
                }

                return method;
            }
        }

        private static TValue[] ToArray<TValue>(JToken arrayOrObject, JsonSerializer serializer)
        {
            if (arrayOrObject == null)
            {
                return new TValue[0];
            }

            switch (arrayOrObject.Type)
            {
                case JTokenType.Object:
                    // sometimes arrays are returned as an object with key 'array'
                    if (IsArrayWithArrayKey(arrayOrObject))
                    {
                        var arrayProperty = arrayOrObject["array"];
                        return arrayProperty
                            .Value<JToken>()
                            .ToObject<TValue[]>(serializer);
                    }
                    // sometimes arrays are returned as an object with indexes as keys
                    else if (IsArrayWithIndexesAsKeys(arrayOrObject))
                    {
                        return arrayOrObject.Values()
                            .Select(x => x.ToObject<TValue>(serializer))
                            .ToArray();
                    }
                    // single records are returned not in array
                    else
                    {
                        return new[]
                            {
                                arrayOrObject.ToObject<TValue>(serializer),
                            };
                    }

                case JTokenType.Array:
                    // valid array
                    return arrayOrObject
                        .Select(x => x.ToObject<TValue>(serializer))
                        .ToArray();

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(arrayOrObject)}.{nameof(arrayOrObject.Type)}", arrayOrObject.Type, null);
            }
        }

        private static bool IsArrayWithArrayKey(JToken arrayOrObject)
        {
            var firstProperty = arrayOrObject
                .OfType<JProperty>()
                .FirstOrDefault();
            if (firstProperty == null)
            {
                return false;
            }

            return firstProperty.Name == "array";
        }

        private static bool IsArrayWithIndexesAsKeys(JToken arrayOrObject)
        {
            var firstProperty = arrayOrObject
                .OfType<JProperty>()
                .FirstOrDefault();
            if (firstProperty == null)
            {
                return false;
            }

            return int.TryParse(firstProperty.Name, out var index);
        }
    }
}