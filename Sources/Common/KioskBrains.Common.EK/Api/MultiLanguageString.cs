using System.Collections.Generic;
using System.Linq;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Common.EK.Api
{
    public class MultiLanguageString : Dictionary<string, string>
    {
        public string GetValue(string languageCode)
        {
            Assure.ArgumentNotNull(languageCode, nameof(languageCode));

            if (Count == 0)
            {
                return null;
            }

            if (ContainsKey(languageCode))
            {
                return this[languageCode];
            }

            return Values.First();
        }

        public override string ToString()
        {
            return string.Join("\n", this.Select(x => $"{x.Key}: {x.Value}"));
        }
    }
}