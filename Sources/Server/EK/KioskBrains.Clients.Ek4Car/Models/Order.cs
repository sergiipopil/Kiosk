using System;
using KioskBrains.Common.EK.Transactions;

namespace KioskBrains.Clients.Ek4Car.Models
{
    public class Order
    {
        /// <summary>
        /// Order ID in KioskBrains portal.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Kiosk ID in KioskBrains portal.
        /// </summary>
        public int KioskId { get; set; }

        public DateTime CreatedOnLocalTime { get; set; }

        /// <summary>
        /// 2-symbol code.
        /// </summary>
        public string PreferableLanguageCode { get; set; }

        public EkTransactionProduct[] Products { get; set; }

        public decimal TotalPrice { get; set; }

        public string TotalPriceCurrencyCode { get; set; }

        public string UserCode { get; set; }

        public EkCustomerInfo Customer { get; set; }

        public EkDeliveryInfo Delivery { get; set; }

        public string ReceiptNumber { get; set; }
    }
}