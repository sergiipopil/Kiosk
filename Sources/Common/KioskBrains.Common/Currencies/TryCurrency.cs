using KioskBrains.Common.Constants;

namespace KioskBrains.Common.Currencies
{
    internal class TryCurrency : Currency
    {
        public override string Code => "TRY";

        public override int IsoCode => 949;

        public override string Symbol => "₺";

        public override string GetAmountString(decimal amount, string languageCode)
        {
            return $"₺{GetAmountValueString(amount)}";
        }

        public override string GetBanknoteString(int banknoteDenomination)
        {
            return $"₺{banknoteDenomination}";
        }

        public override string GetCoinString(int coinDenomination)
        {
            return coinDenomination >= 100
                ? $"₺{coinDenomination/100}"
                : $"{coinDenomination}kr";
        }

        public override string GetFullNamePlural(string languageCode)
        {
            switch (languageCode)
            {
                case Languages.UkrainianCode:
                    return "турецькі ліри";
                case Languages.RussianCode:
                    return "турецкие лиры";
                case Languages.RomanianCode:
                    return "turcă lire";
                default:
                    return "turkish lira";
            }
        }
    }
}