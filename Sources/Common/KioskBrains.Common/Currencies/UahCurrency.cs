using KioskBrains.Common.Constants;

namespace KioskBrains.Common.Currencies
{
    internal class UahCurrency : Currency
    {
        public override string Code => "UAH";

        public override int IsoCode => 980;

        public override string Symbol => "₴";

        public override string GetAmountString(decimal amount, string languageCode)
        {
            switch (languageCode)
            {
                case Languages.UkrainianCode:
                case Languages.RussianCode:
                    return $"{GetAmountValueString(amount)} грн.";
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
                case Languages.UkrainianCode:
                    return "гривні";
                case Languages.RussianCode:
                    return "гривны";
                default:
                    return "hryvnia";
            }
        }
    }
}