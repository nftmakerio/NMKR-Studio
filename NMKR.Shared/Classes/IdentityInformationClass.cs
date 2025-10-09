namespace NMKR.Shared.Classes
{
    public partial class IdentityInformationClass
    {
        public long? Date { get; set; }
        public string PolicyId { get; set; }
        public object[] Accounts { get; set; }
        public bool? Signatures { get; set; }
    }

    public partial struct AccountElement
    {
        public object AccountClass;
        public string String;

        public static implicit operator AccountElement(string String) => new() { String = String };
    }
}
