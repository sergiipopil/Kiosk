using System;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;

namespace KioskBrains.Common.EK.Helpers
{
    public class EkProductKey
    {
        public const string Separator = "_";

        public EkProductSourceEnum Source { get; }

        public string Id { get; }

        public EkProductKey(EkProductSourceEnum source, string id)
        {
            Source = source;
            Id = id;
        }

        public string ToKey()
        {
            return $"{Source}{Separator}{Id}";
        }

        public static EkProductKey FromKey(string productKey)
        {
            Assure.ArgumentNotNull(productKey, nameof(productKey));

            var parts = productKey.Split(new[] { Separator }, 2, StringSplitOptions.None);

            var source = (EkProductSourceEnum)Enum.Parse(typeof(EkProductSourceEnum), parts[0]);
            var id = parts[1];

            return new EkProductKey(source, id);
        }
    }
}