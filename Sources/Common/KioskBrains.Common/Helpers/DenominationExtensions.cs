using System;

namespace KioskBrains.Common.Helpers
{
    public static class DenominationExtensions
    {
        public static int AmountToDenomination(this decimal amount)
        {
            return (int)(amount*100);
        }

        public static int AmountToDenomination(this int amount)
        {
            return amount*100;
        }

        public static decimal DenominationToAmount(this int denomination)
        {
            var amount = ((decimal)denomination)/100;
            return Math.Round(amount, denomination%100 == 0 ? 0 : 2);
        }
    }
}