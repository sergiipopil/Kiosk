using KioskBrains.Common.EK.Transactions;

namespace KioskApp.Ek.Receipt
{
    public class ReceiptData
    {
        public string ReceiptNumber { get; set; }

        public decimal TotalPrice { get; set; }

        public string TotalPriceCurrencyCode { get; set; }

        public EkCustomerInfo CustomerInfo { get; set; }

        public EkDeliveryInfo DeliveryInfo { get; set; }

        public ReceiptDataProduct[] Products { get; set; }
    }
}