namespace KioskBrains.Server.Common.Notifications
{
    public class Notification
    {
        public int CustomerId { get; set; }

        public NotificationTypeEnum Type { get; set; }

        public string Message { get; set; }
    }
}