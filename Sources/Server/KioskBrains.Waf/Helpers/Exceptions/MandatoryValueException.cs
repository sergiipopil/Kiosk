using System.ComponentModel.DataAnnotations;

namespace KioskBrains.Waf.Helpers.Exceptions
{
    public class MandatoryValueException : ValidationException
    {
        public MandatoryValueException()
            : base("Mandatory value is missed.")
        {
        }
    }
}