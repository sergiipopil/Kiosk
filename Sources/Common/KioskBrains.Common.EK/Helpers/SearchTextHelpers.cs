using System;
using System.Text;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Common.EK.Helpers
{
    public static class SearchTextHelpers
    {
        public static string TrimNameAndAddBrandIfMissed(
            string productName,
            string brandName)
        {
            brandName = brandName?.Trim();
            productName = productName?.Trim();

            if (string.IsNullOrEmpty(brandName))
            {
                return productName;
            }

            if (string.IsNullOrEmpty(productName))
            {
                return brandName;
            }

            var preparedProductName = ReplaceSeparatorsWithSpaceAndSurroundWithSpace(productName);
            var preparedBrandName = ReplaceSeparatorsWithSpaceAndSurroundWithSpace(brandName);

            // check if brand name is presented in product name already
            if (preparedProductName.IndexOf(preparedBrandName, StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                return productName;
            }

            return $"{productName} {brandName}";
        }

        private static string ReplaceSeparatorsWithSpaceAndSurroundWithSpace(string text)
        {
            Assure.CheckFlowState(!string.IsNullOrEmpty(text), $"{nameof(text)} is empty.");

            var resultBuilder = new StringBuilder(" ");

            // ReSharper disable PossibleNullReferenceException
            foreach (var textSymbol in text)
                // ReSharper restore PossibleNullReferenceException
            {
                if (char.IsLetterOrDigit(textSymbol))
                {
                    resultBuilder.Append(textSymbol);
                }
                else
                {
                    resultBuilder.Append(" ");
                }
            }

            resultBuilder.Append(" ");

            return resultBuilder.ToString();
        }
    }
}