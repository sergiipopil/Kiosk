using KioskBrains.Common.Constants;

namespace KioskBrains.Common.Currencies
{
    internal class RubCurrency : Currency
    {
        public override string Code => "RUB";

        public override int IsoCode => 643;

        public override string Symbol => "₽";

        public override string GetAmountString(decimal amount, string languageCode)
        {
            switch (languageCode)
            {
                case Languages.UkrainianCode:
                case Languages.RussianCode:
                    return $"{GetAmountValueString(amount)} руб.";
                default:
                    return $"{GetAmountValueString(amount)} {Code}";
            }
        }

        public override string GetBanknoteString(int banknoteDenomination)
        {
            return $"₽{banknoteDenomination}";
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
                    return "рос. рублі";
                case Languages.RussianCode:
                    return "рос. рубли";
                case Languages.RomanianCode:
                    return "ruble rusești";
                case Languages.KazakhCode:
                    return "rесей рублі";
                case Languages.UzbekCode:
                    return "rossiya rubli";
                default:
                    return "rus. rubles";
            }
        }
    }
}