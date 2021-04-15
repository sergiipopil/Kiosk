namespace KioskBrains.Common.EK.Transactions
{
    public class EkDeliveryInfo
    {
        public EkDeliveryTypeEnum type { get; set; }

        /// <summary>
        /// Non-empty for <see cref="EkDeliveryTypeEnum.EkStore" /> and <see cref="EkDeliveryTypeEnum.DeliveryServiceStore" />.
        /// </summary>
        public string storeId { get; set; }

        /// <summary>
        /// Non-empty for <see cref="EkDeliveryTypeEnum.DeliveryServiceStore" />.
        /// </summary>
        public EkDeliveryServiceEnum? deliveryService { get; set; }

        public EkTransactionAddress address { get; set; }
    }
}