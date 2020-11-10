namespace KioskBrains.Common.Currencies
{
    internal class ChfCurrency : Currency
    {
        public override string Code => "CHF";

        public override int IsoCode => 756;

        public override string Symbol => "CHF";

        public override string GetAmountString(decimal amount, string languageCode)
        {
            return $"{GetAmountValueString(amount)} {Code}";
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
                    return "swiss francs";
            }
        }
    }
}