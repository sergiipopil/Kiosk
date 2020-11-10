using System.ComponentModel.DataAnnotations;
using KioskBrains.Server.Common.Notifications;
using KioskBrains.Server.Domain.Entities.Common;

namespace KioskBrains.Server.Domain.Entities
{
    public class NotificationReceiver : EntityBase
    {
        public int CustomerId { get; set; }

        public Customer Customer { get; set; }

        public NotificationTypeEnum NotificationType { get; set; }

        public NotificationChannelEnum Channel { get; set; }

        [Required]
        [StringLength(255)]
        public string ReceiverId { get; set; }
    }
}