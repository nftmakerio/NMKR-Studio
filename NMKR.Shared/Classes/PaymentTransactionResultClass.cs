using System;
using System.Collections.Generic;
using System.Linq;
using NMKR.Shared.SmartContracts;

namespace NMKR.Shared.Classes
{
    [Serializable]
    public enum PaymentTransactionSubstates
    {
        waitingforlocknft,
        waitingforbid,
        sold,
        canceled,
        readytosignbyseller,
        readytosignbysellercancel,
        readytosignbybuyer,
        readytosignbybuyercancel,
        auctionexpired,
        waitingforsale,
        submitted,
        confirmed,
        waitingforlockada
    }

    public enum PaymentGatewayStates
    {
        prepared,
        sold,
        canceled,
        readytosignbybuyer,
        signedbybuyer,
        submitted
    }

    [Serializable]
    public enum PaymentTransactionsStates
    {
        active,
        expired,
        finished,
        prepared,
        error,
        canceled,
        rejected
    }

    [Serializable]
    public enum AuctionHistoryStates
    {
        seller,
        buyer,
        outbid,
        invalid,
        expired,
    }

    [Serializable]
    public enum MintAndSendSubstates
    {
        execute,
        success,
        error,
        canceled,
        invalid
    }

    [Serializable]
    public class PaymentgatewayResultsClass
    {
        public long? PriceInLovelace { get; set; }
        public long? Fee { get; set; }
        public long? Discount { get; set; }
        public long? TokenRewards { get; set; }
        public long? StakeRewards { get; set; }

        public long? MinUtxo { get; set; }
        public MintNftsResultClass MintNfts { get; set; }
        public Tokens[] AdditionalPriceInTokens { get; set; }
        public string OptionalReceiverAddress { get; set; }
        public string ReceiverAddress { get; set; }
        public string TxHash { get; set; }
        public string ReceiverStakeAddress { get; set; }
        public string SenderAddress { get; set; }

        public PaymentgatewayParametersClass ToPaymentGatewayParameters()
        {
            PaymentgatewayParametersClass res = new()
            {
                MintNfts = new MintNftsClass()
                {
                    CountNfts = MintNfts.CountNfts,
                    ReserveNfts = (from a in MintNfts.ReserveNfts
                        select new ReserveNftsClassV2()
                            {Lovelace = a.Lovelace, Tokencount = a.Tokencount, NftUid = a.NftUid}).ToArray()
                },
                OptionalRecevierAddress = OptionalReceiverAddress
            };
            return res;
        }
    }

    [Serializable]
    public class PaymentTransactionSubStateResultClass
    {
        public PaymentTransactionSubstates PaymentTransactionSubstate { get; set; }
        public string LastTxHash { get; set; }
    }

    [Serializable]
    public class AuctionHistoryResultClass
    {
        public string TxHash { get; set; }
        public long BidAmount { get; set; }
        public DateTime Created { get; set; }
        public AuctionHistoryStates State { get; set; }
        public string Address { get; set; }
        public string ReturnTxHash { get; set; }
        public bool SignedAndSubmitted { get; set; }
    }

    [Serializable]
    public class AuctionsResultClass
    {
        public string JsonHash { get; set; }
        public long MinBet { get; set; }
        public DateTime RunsUntil { get; set; }
        public long? ActualBid { get; set; }

        public AuctionHistoryResultClass[] History { get; set; }
        public float? MarketplaceFeePercent { get; set; }
        public float? RoyaltyFeePercent { get; set; }
    }

    [Serializable]
    public class DirectSaleResultsClass
    {
        public long SellingPrice { get; set; }
        public long LockedInAmount { get; set; }
        public string SellerAddress { get; set; }
        public string BuyerAddress { get; set; }
        public string SellerTxDatumHash { get; set; }
        public string SellerTxHash { get; set; }
        public DateTime? SellerTxCreate { get; set; }
        public SmartcontractDirectsaleReceiverClass[] Receivers { get; set; }
        public GetPaymentAddressResultClass BuyoutSmartcontractAddress { get; set; }
    }

    [Serializable]
    public class DirectSaleOfferResultsClass
    {
        public long OfferPrice { get; set; }
        public long LockedInAmount { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerTxDatumHash { get; set; }
        public string BuyerTxHash { get; set; }
        public DateTime? BuyerTxCreate { get; set; }
        public string SellerAddress { get; set; }
        public SmartcontractDirectsaleReceiverClass[] Receivers { get; set; }
    }

    [Serializable]
    public class PaymentTransactionsMintAndSendResultClass
    {
        public MintAndSendSubstates State { get; set; }
        public string TransactionId { get; set; }
        public DateTime? Executed { get; set; }
        public string ReceiverAddress { get; set; }
    }

    [Serializable]
    public class SmartContractInformationResultClass
    {
        public string SmartcontractName { get; set; }
        public string SmartcontractType { get; set; }
        public string SmartcontractAddress { get; set; }
    }

    [Serializable]
    public class PaymentTransactionResultClass : PaymentTransactionResultBaseClass
    {
        public PaymentTransactionTypes PaymentTransactionType { get; set; }
        public TransactionParametersClass[] TransactionParameters { get; set; }
        public PaymentgatewayResultsClass PaymentgatewayResults { get; set; }
        public AuctionsResultClass AuctionResults { get; set; }
        public DirectSaleResultsClass DirectSaleResults { get; set; }
        public DirectSaleOfferResultsClass DirectSaleOfferResults { get; set; }
        public DecentralParametersResultClass DecentralParameters { get; set; }
        public PaymentTransactionsMintAndSendResultClass MintAndSendResults { get; set; }
        public SmartContractInformationResultClass SmartContractInformation { get; set; }

        public PaymentTransactionSubStateResultClass PaymentTransactionSubStateResult { get; set; }
        public string Cbor { get; set; }
        public string SignedCbor { get; set; }
        public string SignGuid { get; set; }  // For security reasons - that only a valid person can sign the contract - only if you have the UID  - the PaymenttransactionUid can have more than 1 personen (an other bidder) and could try to submit always to flood the submit process
        public long? Fee { get; set; }
       
        public PaymentTransactionResultClass? ReferencedTransaction { get; set; }
        public PaymentTransactionTypes PaymentGatewayType { get; set; }
    }

    public class GetNmkrPayLinkResultClass : PaymentTransactionResultBaseClass
    {
        public GetNmkrPayLinkResultClass()
        {
        }
        public GetNmkrPayLinkResultClass(PaymentTransactionResultClass res)
        {
            if (res.PaymentgatewayResults != null)
                PaymentgatewayParameters = res.PaymentgatewayResults.ToPaymentGatewayParameters();

            NMKRPayUrl = res.NMKRPayUrl;
            PaymentTransactionUid= res.PaymentTransactionUid;
            ProjectUid= res.ProjectUid;
            CustomProperties= res.CustomProperties;
            State= res.State;
            PaymentTransactionCreated= res.PaymentTransactionCreated;
            Customeripaddress= res.Customeripaddress;
            Referer= res.Referer;
            Expires= res.Expires;
            PaymentGatewayType= res.PaymentGatewayType;
            TransactionType= res.PaymentTransactionType switch
            {
                PaymentTransactionTypes.paymentgateway_nft_random => NmkrPayTransactionTypes.nmkr_pay_random,
                PaymentTransactionTypes.paymentgateway_nft_specific => NmkrPayTransactionTypes.nmkr_pay_specific,
                PaymentTransactionTypes.decentral_mintandsale_random => NmkrPayTransactionTypes.nmkr_pay_random,
                PaymentTransactionTypes.decentral_mintandsale_specific=>NmkrPayTransactionTypes.nmkr_pay_specific,
                PaymentTransactionTypes.nmkr_pay_random=>NmkrPayTransactionTypes.nmkr_pay_random,
                PaymentTransactionTypes.nmkr_pay_specific=>NmkrPayTransactionTypes.nmkr_pay_specific,
                _ => NmkrPayTransactionTypes.nmkr_pay_random
            };

            PaymentTransactionSubstate = PaymentTransactionSubstates.waitingforsale;

            if (res.ReferencedTransaction != null)
            {
                State = res.ReferencedTransaction.State;
                TxHash = res.ReferencedTransaction.TxHash;
                Expires = res.ReferencedTransaction.Expires;
                PaymentTransactionSubstate =
                    res.ReferencedTransaction.PaymentTransactionSubStateResult?.PaymentTransactionSubstate ??
                    PaymentTransactionSubstates.waitingforsale;
          
                PaymentgatewayResults = res.ReferencedTransaction.DecentralParameters;
            }

        }

        public PaymentgatewayParametersClass PaymentgatewayParameters { get; set; }
        public DecentralParametersResultClass PaymentgatewayResults { get; set; }
        public NmkrPayTransactionTypes TransactionType { get; set; }
        public PaymentTransactionSubstates PaymentTransactionSubstate { get; set; }
        public PaymentTransactionTypes PaymentGatewayType { get; set; }
    }

    public class PaymentTransactionResultBaseClass
    {
        public string PaymentTransactionUid { get; set; }
        public string ProjectUid { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }
        public PaymentTransactionsStates State { get; set; }
        public DateTime PaymentTransactionCreated { get; set; }
        public string Customeripaddress { get; set; }
        public string Referer { get; set; }
        public string TxHash { get; set; }
        public DateTime? Expires { get; set; }
       
        public string NMKRPayUrl { get; set; }
    }

}
