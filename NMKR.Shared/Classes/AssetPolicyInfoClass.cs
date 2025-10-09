    using System;
    using Newtonsoft.Json;

    public partial class AssetPolicyInfoClass
    {
        [JsonProperty("policy_id", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId { get; set; }

        [JsonProperty("asset_name", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }

        [JsonProperty("asset_name_ascii", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetNameAscii { get; set; }

        [JsonProperty("fingerprint", NullValueHandling = NullValueHandling.Ignore)]
        public string Fingerprint { get; set; }

        [JsonProperty("minting_tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string MintingTxHash { get; set; }

        [JsonProperty("mint_cnt", NullValueHandling = NullValueHandling.Ignore)]
        public long? MintCnt { get; set; }

        [JsonProperty("burn_cnt", NullValueHandling = NullValueHandling.Ignore)]
        public long? BurnCnt { get; set; }

      //  [JsonProperty("minting_tx_metadata", NullValueHandling = NullValueHandling.Ignore)]
      //  public object[] MintingTxMetadata { get; set; }

        [JsonProperty("token_registry_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public NMKR.Shared.Classes.Koios.TokenRegistryMetadata TokenRegistryMetadata { get; set; }

        [JsonProperty("total_supply", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalSupply { get; set; }

        [JsonProperty("creation_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? CreationTime { get; set; }

    }

   
    public partial class Jsonx
    {
        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public string Image { get; set; }

        [JsonProperty("pattern", NullValueHandling = NullValueHandling.Ignore)]
        public string Pattern { get; set; }

        [JsonProperty("collection", NullValueHandling = NullValueHandling.Ignore)]
        public string Collection { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
    }

    public partial class TokenRegistryMetadata
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("ticker", NullValueHandling = NullValueHandling.Ignore)]
        public string Ticker { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("logo", NullValueHandling = NullValueHandling.Ignore)]
        public string Logo { get; set; }

        [JsonProperty("decimals", NullValueHandling = NullValueHandling.Ignore)]
        public long? Decimals { get; set; }
    }




