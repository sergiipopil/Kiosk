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
                    id = ekTransaction.Id,
                    kioskId = ekTransaction.KioskId,
                    createdOnLocalTime = ekTransaction.LocalStartedOn,
                    preferableLanguageCode = Languages.RussianCode,
                    products = JsonConvert.DeserializeObject<EkTransactionProduct[]>(ekTransaction.ProductsJson),
                    totalPrice = ekTransaction.TotalPrice,
                    totalPriceCurrencyCode = ekTransaction.TotalPriceCurrencyCode,
                    userCode = ekTransaction.PromoCode,
                    customer = JsonConvert.DeserializeObject<EkCustomerInfo>(ekTransaction.CustomerInfoJson),
                    delivery = JsonConvert.DeserializeObject<EkDeliveryInfo>(ekTransaction.DeliveryInfoJson),
                    receiptNumber = ekTransaction.ReceiptNumber,
                };
        }
    }
}