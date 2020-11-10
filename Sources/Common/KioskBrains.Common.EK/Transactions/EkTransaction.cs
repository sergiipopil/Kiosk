using KioskBrains.Common.Transactions;
using Newtonsoft.Json;

namespace KioskBrains.Common.EK.Transactions
{
    public class EkTransaction : TransactionBase
    {
        public override TransactionWorkflowEnum Workflow => TransactionWorkflowEnum.Ek;

        public string ProductsJson { get; set; }

        public void SetProducts(EkTransactionProduct[] transactionProducts)
        {
            ProductsJson = JsonConvert.SerializeObject(transactionProducts);
        }

        public decimal TotalPrice { get; set; }

        public string TotalPriceCurrencyCode { get; set; }

        public string PromoCode { get; set; }

        public string CustomerInfoJson { get; set; }

        public void SetCustomerInfo(EkCustomerInfo customerInfo)
        {
            CustomerInfoJson = JsonConvert.SerializeObject(customerInfo);
        }

        public string DeliveryInfoJson { get; set; }

        public void SetDeliveryInfo(EkDeliveryInfo deliveryInfo)
        {
            DeliveryInfoJson = JsonConvert.SerializeObject(deliveryInfo);
        }

        public string ReceiptNumber { get; set; }
    }
}