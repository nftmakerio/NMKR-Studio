using System.ComponentModel;

namespace NMKR.Shared.Enums
{
    public enum SaleConditionsTypes
    {
        [Description("Buyer must have one or more NFT with a specific Policy ID/Collection")]
        walletcontainspolicyid,
        [Description("Buyer must have less than x of a NFT with a specific Policy ID/Collection")]
        walletdoescontainmaxpolicyid,
        [Description("Buyer must NOT have a NFT with a specific Policy ID/Collection")]
        walletdoesnotcontainpolicyid,
        [Description("The buyer must have the same amount of NFT/Token (with a certain policy ID/Collection) to buy the amount requested from this policy ID/Collection.")]
        walletcontainsminpolicyid,
        [Description("Buyer must have a minimum of x NFT with a specific Policy ID/Collection")]
        walletmustcontainminofpolicyid,
        [Description("Whitelisted addresses")]
        whitlistedaddresses,
        [Description("Backlisted addresses can not buy")]
        blacklistedaddresses,
        [Description("Buyer must stake on a specific Pool")]
        stakeonpool,
        [Description("Whitelisted addresses (with different max. count of nft)")]
        countedwhitelistedaddresses,
        [Description("Only one sale per account allowed")]
        onlyonesale
    }
    
}
