namespace NMKR.Shared.Classes.CustodialWallets
{
    public class ImportManagedWalletClass
    {
        public string WalletName { get; set; }
        public string WalletPassword { get; set; }
        public string[] SeedWords { get; set; }
        public bool EnterpriseAddress { get; set; }
    }
}
