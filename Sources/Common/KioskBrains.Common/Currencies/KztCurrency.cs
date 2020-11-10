using KioskBrains.Common.Constants;

namespace KioskBrains.Common.Currencies
{
    internal class KztCurrency : Currency
    {
        public override string Code => "KZT";

        public override int IsoCode => 398;

        public override string Symbol => "₸";

        public override string GetAmountString(decimal amount, string languageCode)
        {
            switch (languageCode)
            {
                case Languages.RussianCode:
                case Languages.KazakhCode:
                    return $"{GetAmountValueString(amount)} тг.";
                default:
                    return $"{GetAmountValueString(amount)} {Code}";
            }
        }

        public override string GetBanknoteString(int banknoteDenomination)
        {
            return $"{banknoteDenomination}";
        }

        public override string GetCoinString(int coinDenomination)
        {
            // coins start from 1 tenge
            return $"{coinDenomination}";
        }

        public override string GetFullNamePlural(string languageCode)
        {
            switch (languageCode)
            {
                case Languages.RussianCode:
                    return "тенге";
                case Languages.KazakhCode:
                    return "теңге";
                default:
                    return "tenge";
            }
        }
    }
}