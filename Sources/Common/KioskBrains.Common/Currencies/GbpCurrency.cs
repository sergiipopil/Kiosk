using KioskBrains.Common.Constants;

namespace KioskBrains.Common.Currencies
{
    internal class GbpCurrency : Currency
    {
        public override string Code => "GBP";

        public override int IsoCode => 826;

        public override string Symbol => "£";

        public override string GetAmountString(decimal amount, string languageCode)
        {
            return $"£{GetAmountValueString(amount)}";
        }

        public override string GetBanknoteString(int banknoteDenomination)
        {
            return $"£{banknoteDenomination}";
        }

        public override string GetCoinString(int coinDenomination)
        {
            return coinDenomination >= 100
                ? $"£{coinDenomination/100}"
                : $"{coinDenomination}p";
        }

        public override string GetFullNamePlural(string languageCode)
        {
            switch (languageCode)
            {
                case Languages.UkrainianCode:
                    return "фунти";
                case Languages.RussianCode:
                    return "фунты";
                case Languages.RomanianCode:
                    return "lire sterline";
                default:
                    return "pounds";
            }
        }
    }
}