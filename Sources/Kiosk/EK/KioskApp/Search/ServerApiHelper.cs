using System;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Common.EK.Api;
using KioskBrains.Common.Helpers;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.ServerApi;

namespace KioskApp.Search
{
    public static class ServerApiHelper
    {
        public static async Task<string[]> GetAutocompleteOptionsAsync(
            EkKioskAutocompleteOptionsGetRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await ServerApiProxy.Current.GetAsync<EkKioskAutocompleteOptionsGetResponse>(
                    "/ek-kiosk-autocomplete-options",
                    request,
                    cancellationToken);

                return response.AutocompleteOptions ?? new string[0];
            }
            catch (OperationCanceledException)
            {
                // rethrow cancellations
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Communication, "Request for autocomplete failed.", new
                    {
                        Request = request.GetLogObject(),
                        Exception = ex.GetLoggableObject(),
                    });
                return new string[0];
            }
        }

        public static async Task<EkKioskProductSearchByNameGetResponse> ProductSearchByNameAsync(
            EkKioskProductSearchByNameGetRequest request,
            CancellationToken cancellationToken)
        {
            var response = await ServerApiProxy.Current.GetAsync<EkKioskProductSearchByNameGetResponse>(
                "/ek-kiosk-product-search-by-name",
                request,
                cancellationToken);

            return response;
        }

        public static async Task<EkKioskProductSearchByPartNumberGetResponse> ProductSearchByPartNumberAsync(
            EkKioskProductSearchByPartNumberGetRequest request,
            CancellationToken cancellationToken)
        {
            var response = await ServerApiProxy.Current.GetAsync<EkKioskProductSearchByPartNumberGetResponse>(
                "/ek-kiosk-product-search-by-part-number",
                request,
                cancellationToken);

            return response;
        }

        public static async Task<EkKioskProductAndReplacementsByPartNumberGetResponse> ProductAndReplacementsByPartNumberAsync(
            EkKioskProductAndReplacementsByPartNumberGetRequest request,
            CancellationToken cancellationToken)
        {
            var response = await ServerApiProxy.Current.GetAsync<EkKioskProductAndReplacementsByPartNumberGetResponse>(
                "/ek-kiosk-product-and-replacements-by-part-number",
                request,
                cancellationToken);

            return response;
        }

        public static async Task<EkKioskProductSearchByVinCodeGetResponse> ProductSearchByVinCodeAsync(
            EkKioskProductSearchByVinCodeGetRequest request,
            CancellationToken cancellationToken)
        {
            var response = await ServerApiProxy.Current.GetAsync<EkKioskProductSearchByVinCodeGetResponse>(
                "/ek-kiosk-product-search-by-vin-code",
                request,
                cancellationToken);

            return response;
        }

        public static async Task<EkKioskProductSearchInEuropeGetResponse> ProductSearchInEuropeAsync(
            EkKioskProductSearchInEuropeGetRequest request,
            CancellationToken cancellationToken)
        {
            var response = await ServerApiProxy.Current.GetAsync<EkKioskProductSearchInEuropeGetResponse>(
                "/ek-kiosk-product-search-in-europe",
                request,
                cancellationToken);

            return response;
        }

        public static async Task<EkKioskCarModelModificationsGetResponse> CarModelModificationsAsync(
            EkKioskCarModelModificationsGetRequest request,
            CancellationToken cancellationToken)
        {
            var response = await ServerApiProxy.Current.GetAsync<EkKioskCarModelModificationsGetResponse>(
                "/ek-kiosk-car-model-modifications",
                request,
                cancellationToken);

            return response;
        }

        public static async Task<EkKioskProductCategoriesByCarModelModificationGetResponse> ProductCategoriesByCarModelModificationAsync(
            EkKioskProductCategoriesByCarModelModificationGetRequest request,
            CancellationToken cancellationToken)
        {
            var response = await ServerApiProxy.Current.GetAsync<EkKioskProductCategoriesByCarModelModificationGetResponse>(
                "/ek-kiosk-product-categories-by-car-model-modification",
                request,
                cancellationToken);

            return response;
        }

        public static async Task<EkKioskProductSearchByCategoryGetResponse> ProductSearchByCategoryAsync(
            EkKioskProductSearchByCategoryGetRequest request,
            CancellationToken cancellationToken)
        {
            var response = await ServerApiProxy.Current.GetAsync<EkKioskProductSearchByCategoryGetResponse>(
                "/ek-kiosk-product-search-by-category",
                request,
                cancellationToken);

            return response;
        }

        public static async Task<EkKioskAllegroProductDescriptionGetResponse> AllegroProductDescriptionAsync(
            EkKioskAllegroProductDescriptionGetRequest request,
            CancellationToken cancellationToken)
        {
            var response = await ServerApiProxy.Current.GetAsync<EkKioskAllegroProductDescriptionGetResponse>(
                "/ek-kiosk-allegro-product-description",
                request,
                cancellationToken);

            return response;
        }

        public static async Task<EkKioskVerifyPhoneNumberPostResponse> VerifyPhoneNumberAsync(
            EkKioskVerifyPhoneNumberPostRequest request,
            CancellationToken cancellationToken)
        {
            var response = await ServerApiProxy.Current.PostAsync<EkKioskVerifyPhoneNumberPostResponse>(
                "/ek-kiosk-verify-phone-number",
                request,
                cancellationToken);

            return response;
        }
    }
}