using System.Text;
using KioskBrains.Clients.ElitUa;
using KioskBrains.Clients.OmegaAutoBiz.Models;
using KioskBrains.Common.EK.Api;
using KioskBrains.Common.EK.Helpers;
using Microsoft.Azure.Search.Models;

namespace KioskBrains.Server.EK.Jobs
{
    public static class ConvertHelper
    {
        public static IndexAction OmegaAutoBizApiModelToIndexAction(
            ProductSearchRecord apiProduct,
            long updatedOnUtcTimestamp)
        {
            if (apiProduct == null)
            {
                return null;
            }

            var source = EkProductSourceEnum.OmegaAutoBiz;
            var productKey = new EkProductKey(source, apiProduct.ProductId.ToString())
                .ToKey();
            var price = GetValueOrFallback(apiProduct.CustomerPrice, apiProduct.Price);

            if (price == 0)
            {
                return IndexAction.Delete(new Document()
                    {
                        ["key"] = productKey,
                    });
            }

            var partNumber = apiProduct.Number?.Trim();
            var brandName = apiProduct.BrandDescription?.Trim();
            var nameRu = apiProduct.Description?.Trim();
            var nameUk = GetValueOrFallback(apiProduct.DescriptionUkr?.Trim(), nameRu);
            var description = apiProduct.Info?.Trim();

            var product = new Document()
                {
                    ["key"] = productKey,
                    ["updatedOnUtcTimestamp"] = updatedOnUtcTimestamp,
                    ["source"] = (int)source,
                    ["sourceId"] = apiProduct.ProductId.ToString(),
                    ["partNumber"] = partNumber,
                    ["cleanedPartNumber"] = PartNumberCleaner.GetCleanedPartNumber(partNumber),
                    ["brandName"] = brandName,
                    ["cleanedBrandPartNumber"] = PartNumberCleaner.GetCleanedBrandPartNumber(
                        brandName: brandName,
                        partNumber: partNumber),
                    ["name_ru"] = SearchTextHelpers.TrimNameAndAddBrandIfMissed(
                        productName: nameRu,
                        brandName: brandName),
                    ["name_uk"] = SearchTextHelpers.TrimNameAndAddBrandIfMissed(
                        productName: nameUk,
                        brandName: brandName),
                    ["description_ru"] = description,
                    ["description_uk"] = description,
                    ["price"] = (double)price,
                };

            return IndexAction.MergeOrUpload(product);
        }

        public static IndexAction ElitUaApiModelToIndexAction(
            ElitPriceListRecord apiProduct,
            long updatedOnUtcTimestamp)
        {
            if (apiProduct == null)
            {
                return null;
            }

            var source = EkProductSourceEnum.ElitUa;
            var productKey = new EkProductKey(source, ReplaceInvalidAzureSearchKeySymbolsWithDash(apiProduct.ActiveItemNo))
                .ToKey();
            var nameRu = GetValueOrFallback(apiProduct.EcatDescription, apiProduct.ItemDescription);
            var product = new Document()
                {
                    ["key"] = productKey,
                    ["updatedOnUtcTimestamp"] = updatedOnUtcTimestamp,
                    ["source"] = (int)source,
                    ["sourceId"] = apiProduct.ActiveItemNo,
                    ["partNumber"] = apiProduct.PartNumber,
                    ["cleanedPartNumber"] = PartNumberCleaner.GetCleanedPartNumber(apiProduct.PartNumber),
                    ["brandName"] = apiProduct.Brand,
                    ["cleanedBrandPartNumber"] = PartNumberCleaner.GetCleanedBrandPartNumber(
                        brandName: apiProduct.Brand,
                        partNumber: apiProduct.PartNumber),
                    ["name_ru"] = SearchTextHelpers.TrimNameAndAddBrandIfMissed(
                        productName: nameRu,
                        brandName: apiProduct.Brand),
                    ["price"] = (double)apiProduct.CustomerPrice,
                };

            return IndexAction.MergeOrUpload(product);
        }

        #region Helpers

        private static string GetValueOrFallback(string value, string fallbackValue)
        {
            return !string.IsNullOrEmpty(value)
                ? value
                : fallbackValue;
        }

        private static decimal GetValueOrFallback(decimal value, decimal fallbackValue)
        {
            return value != 0
                ? value
                : fallbackValue;
        }

        private static string ReplaceInvalidAzureSearchKeySymbolsWithDash(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var resultBuilder = new StringBuilder();

            // ReSharper disable PossibleNullReferenceException
            foreach (var textSymbol in text)
                // ReSharper restore PossibleNullReferenceException
            {
                if (char.IsLetterOrDigit(textSymbol)
                    || textSymbol == '-'
                    || textSymbol == '='
                    || textSymbol == '_')
                {
                    resultBuilder.Append(textSymbol);
                }
                else
                {
                    resultBuilder.Append("-");
                }
            }

            return resultBuilder.ToString();
        }

        #endregion
    }
}