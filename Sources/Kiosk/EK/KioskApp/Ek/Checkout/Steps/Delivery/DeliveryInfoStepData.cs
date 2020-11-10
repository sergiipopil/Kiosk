using KioskBrains.Common.EK.Transactions;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Checkout.Steps.Delivery
{
    public class DeliveryInfoStepData : UiBindableObject
    {
        public EkDeliveryTypeEnum? Type { get; set; }

        /// <summary>
        /// Non-empty for <see cref="EkDeliveryTypeEnum.EkStore" /> and <see cref="EkDeliveryTypeEnum.DeliveryServiceStore" />.
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Non-empty for <see cref="EkDeliveryTypeEnum.DeliveryServiceStore" />.
        /// </summary>
        public EkDeliveryServiceEnum? DeliveryService { get; set; }

        public DeliveryInfoAddress Address { get; } = new DeliveryInfoAddress();
    }
}