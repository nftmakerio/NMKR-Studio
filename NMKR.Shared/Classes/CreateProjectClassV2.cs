using System;
using System.ComponentModel.DataAnnotations;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public class CreateProjectClassV2
    {
        [JsonProperty("projectname", NullValueHandling = NullValueHandling.Ignore)]
        public string Projectname { get; set; }
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
        [JsonProperty("projecturl", NullValueHandling = NullValueHandling.Ignore)]
        public string Projecturl { get; set; }
        [JsonProperty("tokennameprefix", NullValueHandling = NullValueHandling.Ignore)]
        public string TokennamePrefix { get; set; }
        [JsonProperty("twitterhandle", NullValueHandling = NullValueHandling.Ignore)]
        public string TwitterHandle { get; set; }
        [JsonProperty("policyexpires", NullValueHandling = NullValueHandling.Ignore)]
        public bool PolicyExpires { get; set; }
        [JsonProperty("policylocksdatetime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? PolicyLocksDateTime { get; set; }
        [JsonProperty("payoutwalletaddress", NullValueHandling = NullValueHandling.Ignore)]
        public string PayoutWalletaddress { get; set; }
        [JsonProperty("payoutwalletaddressusdc", NullValueHandling = NullValueHandling.Ignore)]
        public string? PayoutWalletaddressUsdc { get; set; }

        [JsonProperty("maxnftsupply", NullValueHandling = NullValueHandling.Ignore)]
        [Range(1, long.MaxValue)]
        public long MaxNftSupply { get; set; }
        [JsonProperty("policy", NullValueHandling = NullValueHandling.Ignore)]
        public PolicyClass Policy { get; set; }
        [JsonProperty("metdatatemplate", NullValueHandling = NullValueHandling.Ignore)]
        public string MetadataTemplate { get; set; }
        [JsonProperty("addressexpiretime", NullValueHandling = NullValueHandling.Ignore)]
        [Range(5, 60)]
        public int AddressExpiretime { get; set; }
        [JsonProperty("pricelist", NullValueHandling = NullValueHandling.Ignore)]
        public PricelistClassV2[] Pricelist { get; set; }
        [JsonProperty("additionalpayoutwallets", NullValueHandling = NullValueHandling.Ignore)]
        public PayoutWalletsClassV2[] AdditionalPayoutWallets { get; set; }
        [JsonProperty("saleconditions", NullValueHandling = NullValueHandling.Ignore)]
        public SaleconditionsClassV2[] SaleConditions { get; set; }

        [JsonProperty("discounts", NullValueHandling = NullValueHandling.Ignore)]
        public PriceDiscountClassV2[] Discounts { get; set; }

        [JsonProperty("notifications", NullValueHandling = NullValueHandling.Ignore)]
        public NotificationsClassV2[] Notifications { get; set; }

        [JsonProperty("enablefiat", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EnableFiat { get; set; }
        [JsonProperty("enabledecentralpayments", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EnableDecentralPayments { get; set; }
        [JsonProperty("enablecrosssaleonpaymentgateway", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EnableCrossSaleOnPaymentgateway { get; set; }

        [JsonProperty("activevatepayinaddress", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ActivatePayinAddress { get; set; }

        [JsonProperty("paymentgatewaysalestart", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Paymentgatewaysalestart { get; set; }

        [JsonProperty("storageprovider", NullValueHandling = NullValueHandling.Ignore)]
        public string StorageProvider { get; set; }

        [JsonProperty("metadatastandard", NullValueHandling = NullValueHandling.Ignore)]
        public string MetadataStandard { get; set; } = "CIP25";


        [JsonProperty("cip68referenceaddress", NullValueHandling = NullValueHandling.Ignore)]
        public string Cip68ReferenceAddress { get; set; }
        [JsonProperty("cip68extrafield", NullValueHandling = NullValueHandling.Ignore)]
        public string Cip68ExtraField { get; set; }

        [JsonProperty("enablecardano", NullValueHandling = NullValueHandling.Ignore)]
        public bool EnableCardano { get; set; } = true;
        [JsonProperty("enablesolana", NullValueHandling = NullValueHandling.Ignore)]
        public bool EnableSolana { get; set; }
        [JsonProperty("enableaptos", NullValueHandling = NullValueHandling.Ignore)]
        public bool EnableAptos { get; set; }
        [JsonProperty("enablebitcoin", NullValueHandling = NullValueHandling.Ignore)]
        public bool EnableBitcoin { get; set; }

        [JsonProperty("solanasymbol", NullValueHandling = NullValueHandling.Ignore)]
        public string SolanaSymbol { get; set; }
        [JsonProperty("solancollectionfamiliy", NullValueHandling = NullValueHandling.Ignore)]
        public string SolanaCollectionFamily { get; set; }
        [JsonProperty("payoutwalletaddresssolana", NullValueHandling = NullValueHandling.Ignore)]
        public string PayoutWalletaddressSolana { get; set; }

        [JsonProperty("addcardanopolicyidtosolanametadata", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddCardanoPolicyIdToSolanaMetadata { get; set; }
        [JsonProperty("addsolanacollectionaddresstocardanometadata", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AddSolanaCollectionAddressToCardanoMetadata { get; set; }

        [JsonProperty("solanacreateverifiedcollection", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SolanaCreateVerifiedCollection { get; set; }

        [JsonProperty("solanacollectionimageurl", NullValueHandling = NullValueHandling.Ignore)]
        public string SolanaCollectionImageUrl { get; set; }
        [JsonProperty("solanacollectionimagemimetype", NullValueHandling = NullValueHandling.Ignore)]
        public string SolanaCollectionImageMimeType { get; set; }
        [JsonProperty("solanasellerfeebasispoints", NullValueHandling = NullValueHandling.Ignore)]
        public int? SolanaSellerFeeBasisPoints { get; set; }
       
        [JsonProperty("aptoscollectionimageurl", NullValueHandling = NullValueHandling.Ignore)]
        public string AptosCollectionImageUrl { get; set; }
        [JsonProperty("aptoscollectionimagemimetype", NullValueHandling = NullValueHandling.Ignore)]
        public string AptosCollectionImageMimeType { get; set; }
        [JsonProperty("payoutwalletaddressaptos", NullValueHandling = NullValueHandling.Ignore)]
        public string PayoutWalletaddressAptos { get; set; }
        [JsonProperty("payoutwalletaddressbitcoin", NullValueHandling = NullValueHandling.Ignore)]
        public string PayoutWalletaddressBitcoin { get; set; }
        [JsonProperty("cardanosendbacktocustomer", NullValueHandling = NullValueHandling.Ignore)]
        public MinUtxoTypes CardanoSendbackToCustomer { get; set; } = MinUtxoTypes.twoadaeverynft;
        [JsonProperty("aptoscollectionname", NullValueHandling = NullValueHandling.Ignore)]
        public string AptosCollectionName { get; set; }
    }

    public class PricelistClassV2
    {
        [JsonProperty("countnft", NullValueHandling = NullValueHandling.Ignore)]
        [Range(1, long.MaxValue)]
       
        public long CountNft { get; set; }
        [JsonProperty("priceinlovelace", NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete]
        public long? PriceInLovelace { get; set; }
        [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]

        public float? Price { get; set; }
        [JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]

        public Coin Currency { get; set; }

        [JsonProperty("isactive", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsActive { get; set; }
        [JsonProperty("validfrom", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ValidFrom { get; set; }
        [JsonProperty("validto", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ValidTo { get; set; }
    }

    public class NotificationsClassV2
    {
        [JsonProperty("notificationtype", NullValueHandling = NullValueHandling.Ignore)]
        public PaymentTransactionNotificationTypes NotificationType { get; set; }
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }
        [JsonProperty("isactive", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsActive { get; set; }
    }

    public class GetNotificationsClass : NotificationsClassV2
    {
        [JsonProperty("secret", NullValueHandling = NullValueHandling.Ignore)]
        public string secret { get; set; }
    }


    public class PayoutWalletsClassV2
    {
        [JsonProperty("payoutwallet", NullValueHandling = NullValueHandling.Ignore)]
        public string PayoutWallet { get; set; }
        [JsonProperty("valuepercent", NullValueHandling = NullValueHandling.Ignore)]
        public double? ValuePercent { get; set; }
        [JsonProperty("valuefixinlovelace", NullValueHandling = NullValueHandling.Ignore)]
        public long? ValueFixInLovelace { get; set; }
        [JsonProperty("custompropertycondition", NullValueHandling = NullValueHandling.Ignore)]
        public string CustompropertyCondition { get; set; }

        [JsonProperty("blockchain", NullValueHandling = NullValueHandling.Ignore)]
        public Blockchain Blockchain { get; set; }=Blockchain.Cardano;
    }


    public class PriceDiscountClassV2
    {
        [JsonProperty("condition", NullValueHandling = NullValueHandling.Ignore)]
        public PricelistDiscountTypes Condition { get; set; }
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
        [JsonProperty("minvalue", NullValueHandling = NullValueHandling.Ignore)]
        public long? Minvalue { get; set; }
        [JsonProperty("minvalue2", NullValueHandling = NullValueHandling.Ignore)]
        public long? Minvalue2 { get; set; }
        [JsonProperty("minvalue3", NullValueHandling = NullValueHandling.Ignore)]
        public long? Minvalue3 { get; set; }
        [JsonProperty("minvalue4", NullValueHandling = NullValueHandling.Ignore)]
        public long? Minvalue4 { get; set; }
        [JsonProperty("minvalue5", NullValueHandling = NullValueHandling.Ignore)]
        public long? Minvalue5 { get; set; }
        [JsonProperty("isactive", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsActive { get; set; }
        [JsonProperty("sendbackdiscount", NullValueHandling = NullValueHandling.Ignore)]
        public float SendbackDiscount { get; set; }
        [JsonProperty("policyidorstakeaddress1", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyIdOrStakeAddress1 { get; set; }

        [JsonProperty("policyidorstakeaddress2", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyIdOrStakeAddress2 { get; set; }

        [JsonProperty("policyidorstakeaddress3", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyIdOrStakeAddress3 { get; set; }

        [JsonProperty("policyidorstakeaddress4", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyIdOrStakeAddress4 { get; set; }

        [JsonProperty("policyidorstakeaddress5", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyIdOrStakeAddress5 { get; set; }
        [JsonProperty("whitelistedaddresses", NullValueHandling = NullValueHandling.Ignore)]
        public string[] WhitelistedAddresses { get; set; }
        [JsonProperty("policyprojectname", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyProjectname { get; set; }
        public string Operator { get; set; }
        public string Couponcode { get; set; }
    }


    public class SaleconditionsClassV2
    {
        [JsonProperty("condition", NullValueHandling = NullValueHandling.Ignore)]
        public SaleConditionsTypes Condition { get; set; }

        [JsonProperty("policyid1", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId1 { get; set; }

        [JsonProperty("policyid2", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId2 { get; set; }

        [JsonProperty("policyid3", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId3 { get; set; }

        [JsonProperty("policyid4", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId4 { get; set; }

        [JsonProperty("policyid5", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId5 { get; set; }
        [JsonProperty("policyid6", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId6 { get; set; }
        [JsonProperty("policyid7", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId7 { get; set; }
        [JsonProperty("policyid8", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId8 { get; set; }
        [JsonProperty("policyid9", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId9 { get; set; }
        [JsonProperty("policyid10", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId10 { get; set; }
        [JsonProperty("policyid11", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId11 { get; set; }
        [JsonProperty("policyid12", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId12 { get; set; }
        [JsonProperty("policyid13", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId13 { get; set; }
        [JsonProperty("policyid14", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId14 { get; set; }
        [JsonProperty("policyid15", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId15 { get; set; }

        [JsonProperty("minormaxvalue", NullValueHandling = NullValueHandling.Ignore)]
        public int? MinOrMaxValue { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("isactive", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsActive { get; set; }

        [JsonProperty("policyprojectname", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyProjectname { get; set; }

      
        [JsonProperty("blacklistedaddresses", NullValueHandling = NullValueHandling.Ignore)]
        public string[] BlacklistedAddresses { get; set; }

      

        [JsonProperty("countedwhitelistaddresses", NullValueHandling = NullValueHandling.Ignore)]
        public CountedWhitelistAddressesClass[] CountedWhitelistAddresses { get; set; }
    }
}