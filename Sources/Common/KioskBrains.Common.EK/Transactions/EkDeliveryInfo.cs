namespace KioskBrains.Common.EK.Transactions
{
    public class EkDeliveryInfo
    {
        public EkDeliveryTypeEnum Type { get; set; }

        /// <summary>
        /// Non-empty for <see cref="EkDeliveryTypeEnum.EkStore" /> and <see cref="EkDeliveryTypeEnum.DeliveryServiceStore" />.
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Non-empty for <see cref="EkDeliveryTypeEnum.DeliveryServiceStore" />.
        /// </summary>
        public EkDeliveryServiceEnum? DeliveryService { get; set; }

        public EkTransactionAddress Address { get; set; }
    }
}