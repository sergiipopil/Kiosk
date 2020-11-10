using System;
using System.ComponentModel.DataAnnotations;
using ApiEkTransaction = KioskBrains.Common.EK.Transactions.EkTransaction;

namespace KioskBrains.Server.Domain.Entities.EK
{
    public class EkTransaction : TransactionBase
    {
        public bool IsSentToEkSystem { get; set; }

        public DateTime? NextSendingToEkTimeUtc { get; set; }

        public string ProductsJson { get; set; }

        public decimal TotalPrice { get; set; }

        [StringLength(3)]
        public string TotalPriceCurrencyCode { get; set; }

        [StringLength(20)]
        public string PromoCode { get; set; }

        public string CustomerInfoJson { get; set; }

        public string DeliveryInfoJson { get; set; }

        [StringLength(50)]
        public string ReceiptNumber { get; set; }

        public static EkTransaction FromApiModel(int kioskId, DateTime utcNow, ApiEkTransaction apiModel)
        {
            var transaction = new EkTransaction();

            transaction.ApplyBaseApiModel(kioskId, utcNow, apiModel);

            transaction.ProductsJson = apiModel.ProductsJson;
            transaction.TotalPrice = apiModel.TotalPrice;
            transaction.TotalPriceCurrencyCode = apiModel.TotalPriceCurrencyCode;
            transaction.PromoCode = apiModel.PromoCode;
            transaction.CustomerInfoJson = apiModel.CustomerInfoJson;
            transaction.DeliveryInfoJson = apiModel.DeliveryInfoJson;
            transaction.ReceiptNumber = apiModel.ReceiptNumber;

            return transaction;
        }
    }
}