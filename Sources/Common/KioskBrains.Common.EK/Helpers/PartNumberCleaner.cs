using System.Linq;

namespace KioskBrains.Common.EK.Helpers
{
    public static class PartNumberCleaner
    {
        public static string GetCleanedPartNumber(string partNumber)
        {
            if (string.IsNullOrEmpty(partNumber))
            {
                return partNumber;
            }

            var cleanedPartNumber = new string(partNumber
                .Where(x => char.IsLetterOrDigit(x))
                .ToArray());

            return cleanedPartNumber;
        }

        public static string GetCleanedBrandName(string brandName)
        {
            if (string.IsNullOrEmpty(brandName))
            {
                return brandName;
            }

            var cleanedBrandName = new string(brandName
                .Where(x => char.IsLetterOrDigit(x))
                .ToArray());

            return cleanedBrandName;
        }

        public static string GetCleanedBrandPartNumber(string brandName, string partNumber)
        {
            return $"{GetCleanedBrandName(brandName)}_{GetCleanedPartNumber(partNumber)}";
        }
    }
}