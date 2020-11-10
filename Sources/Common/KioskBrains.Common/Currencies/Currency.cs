using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Common.Currencies
{
    public abstract class Currency
    {
        private static readonly Currency[] _currencies =
            {
                new ChfCurrency(),
                new EurCurrency(),
                new GbpCurrency(),
                new KztCurrency(),
                new MdlCurrency(),
                new PlnCurrency(),
                new RonCurrency(),
                new RubCurrency(),
                new TryCurrency(),
                new UahCurrency(),
                new UsdCurrency(),
                new UzsCurrency(),
            };

        private static readonly Dictionary<string, Currency> _currenciesByCode = _currencies.ToDictionary(x => x.Code);

        // Adds thousand space separator while converting decimal/int to string
        private static readonly NumberFormatInfo _numberFormatInfo;

        static Currency()
        {
            _numberFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            _numberFormatInfo.NumberGroupSeparator = " ";
        }

        public static Currency GetCurrencyByCode(string code, bool failIfNotFound = true)
        {
            Assure.ArgumentNotNull(code, nameof(code));
            code = code.ToUpper();

            if (!_currenciesByCode.TryGetValue(code, out var currency))
            {
                if (failIfNotFound)
                {
                    throw new NotSupportedException($"Currency '{code}' is not supported.");
                }

                return null;
            }

            return currency;
        }

        public abstract string Code { get; }

        public abstract int IsoCode { get; }

        public abstract string Symbol { get; }

        public abstract string GetAmountString(decimal amount, string languageCode);

        public abstract string GetBanknoteString(int banknoteDenomination);

        public abstract string GetCoinString(int coinDenomination);

        public abstract string GetFullNamePlural(string languageCode);

        public static string[] GetAllSupportedCurrencyCodes()
        {
            return _currencies
                .Select(x => x.Code)
                .ToArray();
        }

        public override string ToString()
        {
            return Code;
        }

        #region Helpers

        public static string GetAmountValueString(decimal amount)
        {
            var amountString = Math.Round(amount, 2).ToString("n", _numberFormatInfo);
            if (amountString.EndsWith(".00"))
            {
                amountString = amountString.Substring(0, amountString.Length - 3);
            }

            return amountString;
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            return this == (obj as Currency);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public static bool operator ==(Currency obj1, Currency obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            if ((object)obj1 == null || (object)obj2 == null)
            {
                return false;
            }

            return obj1.Code == obj2.Code;
        }

        public static bool operator !=(Currency obj1, Currency obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}