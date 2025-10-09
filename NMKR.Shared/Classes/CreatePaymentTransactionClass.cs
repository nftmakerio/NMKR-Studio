using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NMKR.Shared.Classes
{
    public enum PaymentTransactionTypes 
    {
        paymentgateway_nft_specific,
        paymentgateway_nft_random,
        smartcontract_directsale,
        smartcontract_auction,
        legacy_auction,
        legacy_directsale,
        decentral_mintandsend_specific,
        decentral_mintandsend_random,
        decentral_mintandsale_specific,
        decentral_mintandsale_random,
        paymentgateway_mintandsend_specific,
        paymentgateway_mintandsend_random,
        nmkr_pay_random,
        nmkr_pay_specific,
        smartcontract_directsale_offer,
        paymentgateway_buyout_smartcontract,
    }

    public enum NmkrPayTransactionTypes
    {
        nmkr_pay_random,
        nmkr_pay_specific
    }
    public enum PaymentTransactionNotificationTypes
    {
        webhook,
        email
    }


    public class AuctionParametersClass
    {
        [Range(1000, int.MaxValue)] public int DurationInSeconds { get; set; }
        [Range(1000000, long.MaxValue)] public long MinBet { get; set; }
    }

    public class AuctionParametersResultClass
    {
        public int DurationInSeconds { get; set; }
        public long MinBet { get; set; }
    }

    public class MintNftsClass
    {
        public long? CountNfts { get; set; }
        public ReserveNftsClassV2[] ReserveNfts { get; set; }
    }

    public class DecentralParametersClass
    {
        public MintNftsClass MintNfts { get; set; }

        //   public long? PriceInLovelace { get; set; }
        public CreateRoyaltyTokenIfNotExistsClass CreateRoyaltyTokenIfNotExists { get; set; }
        public string OptionalRecevierAddress { get; set; }
    }

    public class CreateRoyaltyTokenIfNotExistsClass
    {
        [Range(1, 100)] public float Percentage { get; set; }
        public string Address { get; set; }
    }

    public class NmkrPayPaymentgatewayParamatersClass : PaymentgatewayParametersClass
    {
        // TODO: Add NMKR Pay specific parameters
    /*    public long? PriceInLovelace { get; set; }
        public TokensBaseClass AdditionalPriceInTokens { get; set; }*/
    }

    public class PaymentgatewayParametersClass
    {
        public MintNftsClass MintNfts { get; set; }
        public string OptionalRecevierAddress { get; set; }
    }

    public class MintNftsResultClass
    {
        public long? CountNfts { get; set; }
        public ReservedNftsClassV2[] ReserveNfts { get; set; }
    }

    public class DecentralParametersResultClass
    {
        public MintNftsResultClass MintNfts { get; set; }
        public long? PriceInLovelace { get; set; }
        public Tokens[] AdditionalPriceInTokens { get; set; }

        public long? StakeRewards { get; set; }

        public long? Discount { get; set; }

        public string RejectParameter { get; set; }

        public string RejectReason { get; set; }

        public long? TokenRewards { get; set; }
        public long? Fees { get; set; }
        public string OptionalReceiverAddress { get; set; }
    }

    public class PaymentgatewayParametersResultClass
    {
        public MintNftsResultClass MintNfts { get; set; }
        public long? PriceInLovelace { get; set; }
    }



    public class TransactionParametersClass
    {
        public long Tokencount { get; set; }
        public string PolicyId { get; set; }
        public string Tokenname { get; set; }
        public string TokennameHex { get; set; }
    }

    public class DirectSaleParameterClass
    {
        public long PriceInLovelace { get; set; }
        public string TxHashForAlreadyLockedinAssets { get; set; }
        public string SmartContractName { get; set; }
        public string OverrideMarkteplaceFeeAddress { get; set; }
        public double? OverrideMarketplaceFee { get; set; }
    }

    public class DirectSaleOfferParameterClass
    {
        public long OfferInLovelace { get; set; }
        public string TxHashForAlreadyLockedinAssets { get; set; }
        public string OverrideMarkteplaceFeeAddress { get; set; }
        public double? OverrideMarketplaceFee { get; set; }
    }

    public class DirectSaleParameterResultClass
    {
        public long PriceInLovelace { get; set; }
    }

    public class PaymentTransactionNotificationsClass
    {
        public PaymentTransactionNotificationTypes NotificationType { get; set; }
        public string NotificationEndpoint { get; set; }
        public string HMACSecret { get; set; }
    }

    public class CreatePaymentTransactionClass : CreatePaymentTransactionBaseClass
    {
        public CreatePaymentTransactionClass()
        {

        }

        public CreatePaymentTransactionClass(GetNmkrPayLinkClass nmkrpaylink, PaymentTransactionTypes paymentTransactionType )
        {
            ProjectUid = nmkrpaylink.ProjectUid;
            Referer = nmkrpaylink.Referer;
            CustomerIpAddress = nmkrpaylink.CustomerIpAddress;
            CustomProperties = nmkrpaylink.CustomProperties;
            PaymentTransactionNotifications = nmkrpaylink.PaymentTransactionNotifications;
            PaymentgatewayParameters = nmkrpaylink.PaymentgatewayParameters;
            PaymentTransactionType = paymentTransactionType;
        }
        public PaymentgatewayParametersClass PaymentgatewayParameters { get; set; }
        public PaymentTransactionTypes PaymentTransactionType { get; set; }
        public TransactionParametersClass[] TransactionParameters { get; set; }
        public DecentralParametersClass DecentralParameters { get; set; }
        public AuctionParametersClass AuctionParameters { get; set; }
        public DirectSaleParameterClass DirectSaleParameters { get; set; }
        public DirectSaleOfferParameterClass DirectSaleOfferParameters { get; set; }
        public string ReferencedPaymenttransactionUid { get; set; }
    }


    public class GetNmkrPayLinkClass : CreatePaymentTransactionBaseClass
    {
        public NmkrPayTransactionTypes PaymentTransactionType { get; set; }
        public NmkrPayPaymentgatewayParamatersClass PaymentgatewayParameters { get; set; }
    }

    public class CreatePaymentTransactionBaseClass
    {
        public string ProjectUid { get; set; }
        public string Referer { get; set; }
        public string CustomerIpAddress { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }
        public PaymentTransactionNotificationsClass[] PaymentTransactionNotifications { get; set; }
    }
}