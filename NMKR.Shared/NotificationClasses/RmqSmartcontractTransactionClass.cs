namespace NMKR.Shared.NotificationClasses
{
    public class RmqSmartcontractTransactionClass
    {
        public NotificationEventTypes EventType { get; set; }
        public int PreparedTransactionId { get; set; }
        public int PreparedJsonId { get; set; }
    }
}