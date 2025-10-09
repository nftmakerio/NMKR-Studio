namespace NMKR.Shared.Classes
{
    public class AddIpLimitClass
    {
        public int? Id { get; set; }
        public string  AccessFrom { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public int ApiKeyId { get; set; }
        public int Order { get; set; }
        public int CustomerId { get; set; }

    }
}
