using KioskBrains.Common.Constants;

namespace KioskBrains.Common.Currencies
{
    internal class PlnCurrency : Currency
    {
        public override string Code => "PLN";

        public override int IsoCode => 985;

        public override string Symbol => "zł";

        public override string GetAmountString(decimal amount, string languageCode)
        {
            switch (languageCode)
            {
                case Languages.PolishCode:
                    return $"{GetAmountValueString(amount)} zł";
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
                case Languages.PolishCode:
                    return "złotych";
                default:
                    return "zlotys";
            }
        }
    }
}