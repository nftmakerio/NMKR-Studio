using CardanoSharp.Wallet.Enums;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;

namespace NMKR.Shared.Classes
{
    public sealed class ReserveAddressQueueClass
    {
       public string ApiKey { get; set; }
        public int? NftprojectId { get; set; }
        public string NftprojectUId { get; set; }
        public long CountNft { get; set; }
        public long? CardanoLovelace { get; set; }
        public long? Price { get; set; }
        public long? PriceInToken { get; set; }
        public string TokenPolicyId { get; set; }
        public string TokenAssetId { get; set; }
        public string CustomerIpAddress { get; set; }
        public string Uid { get; set; } = GlobalFunctions.GetGuid();
        public string Referer { get; set; }
        public ReserveMultipleNftsClassV2 Reservenfts { get; set; }
        public string CustomProperty { get; set; }
        public long? TotalTokens { get; set; }
        public long? Multiplier { get; set; }
        public long Decimals { get; set; }
        public string OptionalReceiverAddress { get; set; }
        public string TokenAssetIdHex { get; set; }
        //  public IPAddress RemoteIpAddress { get; set; }
        public string RemoteIpAddress { get; set; }
        public string OptionalRefundAddress { get; set; }
        public bool? AcceptHeigherAmounts { get; set; }
        public AddressType Addresstype { get; set; }
        public Coin Coin { get; set; }=Coin.ADA;
        public long? SolanaLamport { get; set; }
        public long? AptosOcta;
        public uint? ReservationTimeInMinutes { get; set; }
        public bool Freemint { get; set; }
        public long? BitcoinSatoshi { get; set; }
    }

    public sealed class ReserveAddressQueueResultClass
    {
        public int StatusCode { get; set; }
        public ApiErrorResultClass ApiError { get; set; }
        public GetPaymentAddressResultClass SuccessResult { get; set; }
    }




}
