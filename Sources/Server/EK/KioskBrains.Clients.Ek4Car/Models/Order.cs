using System;
using KioskBrains.Common.EK.Transactions;

namespace KioskBrains.Clients.Ek4Car.Models
{
    public class Order
    {
        /// <summary>
        /// Order ID in KioskBrains portal.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Kiosk ID in KioskBrains portal.
        /// </summary>
        public int kioskId { get; set; }

        public DateTime createdOnLocalTime { get; set; }

        /// <summary>
        /// 2-symbol code.
        /// </summary>
        public string preferableLanguageCode { get; set; }

        public EkTransactionProduct[] products { get; set; }

        public decimal totalPrice { get; set; }

        public string totalPriceCurrencyCode { get; set; }

        public string userCode { get; set; }

        public EkCustomerInfo customer { get; set; }

        public EkDeliveryInfo delivery { get; set; }

        public string receiptNumber { get; set; }
    }
}