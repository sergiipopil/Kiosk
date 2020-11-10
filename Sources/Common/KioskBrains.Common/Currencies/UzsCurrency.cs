using KioskBrains.Common.Constants;

namespace KioskBrains.Common.Currencies
{
    internal class UzsCurrency : Currency
    {
        public override string Code => "UZS";

        public override int IsoCode => 860;

        public override string Symbol => "soʻm";

        public override string GetAmountString(decimal amount, string languageCode)
        {
            switch (languageCode)
            {
                case Languages.UzbekCode:
                    return $"{GetAmountValueString(amount)} soʻm";
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
            return $"{coinDenomination}";
        }

        public override string GetFullNamePlural(string languageCode)
        {
            switch (languageCode)
            {
                case Languages.RussianCode:
                    return "сум";
                default:
                    return "soʻm";
            }
        }
    }
}