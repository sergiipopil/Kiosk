using System.Linq;

namespace KioskApp.Ek.Checkout.Steps
{
    public static class PhoneNumberHelper
    {
        public static string GetCleanedPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return phoneNumber;
            }

            phoneNumber = new string(phoneNumber
                .Where(x => x == '+'
                            || char.IsDigit(x))
                .ToArray());

            if (phoneNumber[0] != '+')
            {
                phoneNumber = "+" + phoneNumber;
            }

            return phoneNumber;
        }
    }
}