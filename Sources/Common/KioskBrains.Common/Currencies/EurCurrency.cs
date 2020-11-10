using KioskBrains.Common.Constants;

namespace KioskBrains.Common.Currencies
{
    internal class EurCurrency : Currency
    {
        public override string Code => "EUR";

        public override int IsoCode => 978;

        public override string Symbol => "€";

        public override string GetAmountString(decimal amount, string languageCode)
        {
            return $"€{GetAmountValueString(amount)}";
        }

        public override string GetBanknoteString(int banknoteDenomination)
        {
            return $"€{banknoteDenomination}";
        }

        public override string GetCoinString(int coinDenomination)
        {
            return coinDenomination >= 100
                ? $"€{coinDenomination/100}"
                : $"{coinDenomination}¢";
        }

        public override string GetFullNamePlural(string languageCode)
        {
            switch (languageCode)
            {
                case Languages.UkrainianCode:
                    return "євро";
                case Languages.RussianCode:
                    return "евро";
                case Languages.RomanianCode:
                    return "euro";
                case Languages.KazakhCode:
                    return "еуро";
                case Languages.UzbekCode:
                    return "evro";
                default:
                    return "euro";
            }
        }
    }
}