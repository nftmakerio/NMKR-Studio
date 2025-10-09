namespace NMKR.Shared.NotificationClasses
{
    public class RmqTransactionClass
    {
        public NotificationEventTypes EventType { get; set; }
        public int? TransactionId { get; set; }
        public int? AddressId { get; set; }
        public int? ProjectId { get; init; }
    }
}
