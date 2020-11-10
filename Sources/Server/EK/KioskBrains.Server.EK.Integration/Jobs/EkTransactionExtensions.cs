using KioskBrains.Clients.Ek4Car.Models;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Transactions;
using Newtonsoft.Json;
using EkTransaction = KioskBrains.Server.Domain.Entities.EK.EkTransaction;

namespace KioskBrains.Server.EK.Integration.Jobs
{
    public static class EkTransactionExtensions
    {
        public static Order ToEkOrder(this EkTransaction ekTransaction)
        {
            Assure.ArgumentNotNull(ekTransaction, nameof(ekTransaction));

            return new Order()
                {
                    Id = ekTransaction.Id,
                    KioskId = ekTransaction.KioskId,
                    CreatedOnLocalTime = ekTransaction.LocalStartedOn,
                    PreferableLanguageCode = Languages.RussianCode,
                    Products = JsonConvert.DeserializeObject<EkTransactionProduct[]>(ekTransaction.ProductsJson),
                    TotalPrice = ekTransaction.TotalPrice,
                    TotalPriceCurrencyCode = ekTransaction.TotalPriceCurrencyCode,
                    UserCode = ekTransaction.PromoCode,
                    Customer = JsonConvert.DeserializeObject<EkCustomerInfo>(ekTransaction.CustomerInfoJson),
                    Delivery = JsonConvert.DeserializeObject<EkDeliveryInfo>(ekTransaction.DeliveryInfoJson),
                    ReceiptNumber = ekTransaction.ReceiptNumber,
                };
        }
    }
}