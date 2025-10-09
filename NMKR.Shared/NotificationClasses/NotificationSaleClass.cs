using System;
using System.Collections.Generic;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NMKR.Shared.NotificationClasses
{
   

    public enum NotificationEventTypes
    {
        transactionconfirmed,
        transactionfinished,
        transactioncanceled,
        addressexpired
    }


    public class NotificationSaleNft
    {
        public int NftId { get; set; }
        public string NftUid { get; set; }
        public string NftName { get; set; }
        public string NftNameInHex { get; set; }
        public string AssetId { get; set; }
        public string PolicyId { get; set; }
        public long Count { get; set; }
        public long Multiplier { get; set; }
        public string Fingerprint { get; set; }
    }

    public class CustomPropertyClass
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
    public class NotificationSaleClass
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationEventTypes EventType { get; set; }
        public string ProjectName { get; set; }
        public string ProjectUid { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionTypes SaleType { get; set; }
        public DateTime SaleDate { get; set; }
        public long Price { get; set; }
        public long? MintingCosts { get; set; }
        public long? SendbackCosts { get; set; }
        public long? NetworkFees { get; set; }
        public long? NMKRRewards { get; set; }
        public long? Discount { get; set; }
        public NotificationSaleNft[] NotificationSaleNfts { get; set; }
        public string TxHash { get; set; }
        public string ReceiverAddress { get; set; }
        public string OriginatorAddress { get; set; }
        public string StakeAddressReceiver { get; set; }
        public string SenderAddress { get; set; }
        public object DetailResults { get; set; }
        public string Metadata { get; set; }
        public string PreparedTransactionUid { get; set; }
        public List<CustomPropertyClass> CustomProperties { get; set; }
        public DateTime? AddressExpiration { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Blockchain BlockChain { get; set; }
        public string DiscountCode { get; set; }
        public string MetadataStandard { get; set; }
        public string Cip68ReferenceTokenAddress { get; set; }
    }


    public class NotificationSmartcontracrtClass
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentTransactionTypes TransactionType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DatumTemplateTypes EventType { get; set; }
        public string ProjectName { get; set; }
        public string ProjectUid { get; set; }
        public string InitiatorAddress { get; set; }
        public DateTime Created { get; set; }
        public string TxHash { get; set; }
        public object DetailResults { get; set; }
        public NotificationSaleNft[] NotificationSaleNfts { get; set; }
    }

}
