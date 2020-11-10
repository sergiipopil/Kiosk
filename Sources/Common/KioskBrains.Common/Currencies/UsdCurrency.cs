using KioskBrains.Common.Constants;

namespace KioskBrains.Common.Currencies
{
    internal class UsdCurrency : Currency
    {
        public override string Code => "USD";

        public override int IsoCode => 840;

        public override string Symbol => "$";

        public override string GetAmountString(decimal amount, string languageCode)
        {
            return $"${GetAmountValueString(amount)}";
        }

        public override string GetBanknoteString(int banknoteDenomination)
        {
            return $"${banknoteDenomination}";
        }

        public override string GetCoinString(int coinDenomination)
        {
            return coinDenomination >= 100
                ? $"${coinDenomination/100}"
                : $"{coinDenomination}¢";
        }

        public override string GetFullNamePlural(string languageCode)
        {
            switch (languageCode)
            {
                case Languages.UkrainianCode:
                    return "долари США";
                case Languages.RussianCode:
                    return "доллары США";
                case Languages.RomanianCode:
                    return "dolari americani";
                case Languages.KazakhCode:
                    return "АҚШ доллары";
                case Languages.UzbekCode:
                    return "AQSh dollari";
                default:
                    return "US dollars";
            }
        }
    }
}