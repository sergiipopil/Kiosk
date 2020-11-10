using System;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;

namespace KioskBrains.Common.Currencies
{
    /// <summary>
    /// Not for serialization. Should be used as helper.
    /// </summary>
    public struct CurrencyPair : ILoggableObject
    {
        public string LocalCurrencyCode { get; }

        public string ForeignCurrencyCode { get; }

        public CurrencyPair(string localCurrencyCode, string foreignCurrencyCode)
        {
            Assure.ArgumentNotNull(localCurrencyCode, nameof(localCurrencyCode));
            Assure.ArgumentNotNull(foreignCurrencyCode, nameof(foreignCurrencyCode));

            LocalCurrencyCode = localCurrencyCode;
            ForeignCurrencyCode = foreignCurrencyCode;
        }

        private const char CurrencyPairKeySeparator = '-';

        public string ToCurrencyPairKey()
        {
            return $"{LocalCurrencyCode}{CurrencyPairKeySeparator}{ForeignCurrencyCode}";
        }

        public static CurrencyPair FromCurrencyPairKey(string currencyPairKey)
        {
            Assure.ArgumentNotNull(currencyPairKey, nameof(currencyPairKey));

            var currencyCodes = currencyPairKey.Split(CurrencyPairKeySeparator);
            if (currencyCodes.Length != 2)
            {
                throw new FormatException($"'{currencyPairKey}' has wrong currency pair key format.");
            }

            return new CurrencyPair(currencyCodes[0], currencyCodes[1]);
        }

        public object GetLogObject()
        {
            return ToCurrencyPairKey();
        }

        #region Equals

        public override bool Equals(object obj)
        {
            return obj is CurrencyPair && this == (CurrencyPair)obj;
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash*23 + LocalCurrencyCode.GetHashCode();
            hash = hash*23 + ForeignCurrencyCode.GetHashCode();
            return hash;
        }

        public static bool operator ==(CurrencyPair obj1, CurrencyPair obj2)
        {
            return obj1.LocalCurrencyCode == obj2.LocalCurrencyCode
                   && obj1.ForeignCurrencyCode == obj2.ForeignCurrencyCode;
        }

        public static bool operator !=(CurrencyPair obj1, CurrencyPair obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}