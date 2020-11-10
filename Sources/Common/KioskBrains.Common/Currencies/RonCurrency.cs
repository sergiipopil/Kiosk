namespace KioskBrains.Common.Currencies
{
    internal class RonCurrency : Currency
    {
        public override string Code => "RON";

        public override int IsoCode => 946;

        public override string Symbol => "lei";

        public override string GetAmountString(decimal amount, string languageCode)
        {
            switch (languageCode)
            {
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
                default:
                    return "romanian lei";
            }
        }
    }
}