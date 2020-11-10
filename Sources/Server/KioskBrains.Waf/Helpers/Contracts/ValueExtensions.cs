using KioskBrains.Waf.Helpers.Exceptions;

namespace KioskBrains.Waf.Helpers.Contracts
{
    public static class ValueExtensions
    {
        public static TValue GetMandatoryValue<TValue>(this TValue? nullable)
            where TValue : struct
        {
            if (nullable == null)
            {
                throw new MandatoryValueException();
            }
            return nullable.Value;
        }
    }
}