using KioskBrains.Common.Constants;

namespace KioskBrains.Common.Currencies
{
    internal class MdlCurrency : Currency
    {
        public override string Code => "MDL";

        public override int IsoCode => 498;

        public override string Symbol => "L";

        public override string GetAmountString(decimal amount, string languageCode)
        {
            switch (languageCode)
            {
                case Languages.RussianCode:
                    return $"{GetAmountValueString(amount)} лей";
                case Languages.RomanianCode:
                    return $"{GetAmountValueString(amount)} lei";
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
                    return "молдавские леи";
                case Languages.RomanianCode:
                    return "lei moldovenești";
                default:
                    return "moldovan lei";
            }
        }
    }
}