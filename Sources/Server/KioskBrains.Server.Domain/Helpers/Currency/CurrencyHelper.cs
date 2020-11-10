using System.Linq;
using KioskBrains.Server.Domain.Actions.Common.Models;

namespace KioskBrains.Server.Domain.Helpers.Currency
{
    public static class CurrencyHelper
    {
        public static ListOptionString[] GetCurrencyListOptions()
        {
            return KioskBrains.Common.Currencies.Currency.GetAllSupportedCurrencyCodes()
                .OrderBy(x => x)
                .Select(x => new ListOptionString()
                    {
                        Value = x,
                        DisplayName = x
                    })
                .ToArray();
        }
    }
}