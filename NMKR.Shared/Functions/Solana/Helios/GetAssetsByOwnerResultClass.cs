using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Functions.Solana.Helios
{
    public partial class GetAssetsByOwnerResultClass
    {
        [JsonProperty("jsonrpc", NullValueHandling = NullValueHandling.Ignore)]
        public string Jsonrpc { get; set; }

        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public Result Result { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
        public long? Total { get; set; }

        [JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
        public long? Limit { get; set; }

        [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
        public long? Page { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public SolanaItem[] Items { get; set; }
    }

    public partial class SolanaItem
    {
        [JsonProperty("interface", NullValueHandling = NullValueHandling.Ignore)]
        public string Interface { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public Content Content { get; set; }

        [JsonProperty("authorities", NullValueHandling = NullValueHandling.Ignore)]
        public Authority[] Authorities { get; set; }

        [JsonProperty("compression", NullValueHandling = NullValueHandling.Ignore)]
        public Compression Compression { get; set; }

        [JsonProperty("grouping", NullValueHandling = NullValueHandling.Ignore)]
        public Grouping[] Grouping { get; set; }

        [JsonProperty("royalty", NullValueHandling = NullValueHandling.Ignore)]
        public Royalty Royalty { get; set; }

        [JsonProperty("creators", NullValueHandling = NullValueHandling.Ignore)]
        public Creator[] Creators { get; set; }

        [JsonProperty("ownership", NullValueHandling = NullValueHandling.Ignore)]
        public Ownership Ownership { get; set; }

        [JsonProperty("supply")]
        public Supply Supply { get; set; }

        [JsonProperty("mutable", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Mutable { get; set; }

        [JsonProperty("burnt", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Burnt { get; set; }

        [JsonProperty("token_info", NullValueHandling = NullValueHandling.Ignore)]
        public TokenInfo TokenInfo { get; set; }

        [JsonProperty("mint_extensions", NullValueHandling = NullValueHandling.Ignore)]
        public MintExtensions MintExtensions { get; set; }
    }

    public partial class Authority
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("scopes", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Scopes { get; set; }
    }

    public partial class Compression
    {
        [JsonProperty("eligible", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Eligible { get; set; }

        [JsonProperty("compressed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Compressed { get; set; }

        [JsonProperty("data_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string DataHash { get; set; }

        [JsonProperty("creator_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string CreatorHash { get; set; }

        [JsonProperty("asset_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetHash { get; set; }

        [JsonProperty("tree", NullValueHandling = NullValueHandling.Ignore)]
        public string Tree { get; set; }

        [JsonProperty("seq", NullValueHandling = NullValueHandling.Ignore)]
        public long? Seq { get; set; }

        [JsonProperty("leaf_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? LeafId { get; set; }
    }

    public partial class Content
    {
        [JsonProperty("$schema", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Schema { get; set; }

        [JsonProperty("json_uri", NullValueHandling = NullValueHandling.Ignore)]
        public Uri JsonUri { get; set; }

        [JsonProperty("files", NullValueHandling = NullValueHandling.Ignore)]
        public File[] Files { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public Metadata Metadata { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public Links Links { get; set; }
    }

    public partial class File
    {
        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string Uri { get; set; }

        [JsonProperty("cdn_uri", NullValueHandling = NullValueHandling.Ignore)]
        public Uri CdnUri { get; set; }

        [JsonProperty("mime", NullValueHandling = NullValueHandling.Ignore)]
        public string Mime { get; set; }
    }

    public partial class Links
    {
        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public string Image { get; set; }

        [JsonProperty("external_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ExternalUrl { get; set; }
    }

    public partial class Metadata
    {
        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public Attribute[] Attributes { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
        public string Symbol { get; set; }

        [JsonProperty("token_standard", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenStandard { get; set; }
    }

    public partial class Attribute
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }

        [JsonProperty("trait_type", NullValueHandling = NullValueHandling.Ignore)]
        public string TraitType { get; set; }
    }

    public partial class Creator
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("share", NullValueHandling = NullValueHandling.Ignore)]
        public long? Share { get; set; }

        [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Verified { get; set; }
    }

    public partial class Grouping
    {
        [JsonProperty("group_key", NullValueHandling = NullValueHandling.Ignore)]
        public string GroupKey { get; set; }

        [JsonProperty("group_value", NullValueHandling = NullValueHandling.Ignore)]
        public string GroupValue { get; set; }
    }

    public partial class MintExtensions
    {
        [JsonProperty("transfer_fee_config", NullValueHandling = NullValueHandling.Ignore)]
        public TransferFeeConfig TransferFeeConfig { get; set; }
    }

    public partial class TransferFeeConfig
    {
        [JsonProperty("withheld_amount", NullValueHandling = NullValueHandling.Ignore)]
        public double? WithheldAmount { get; set; }

        [JsonProperty("newer_transfer_fee", NullValueHandling = NullValueHandling.Ignore)]
        public ErTransferFee NewerTransferFee { get; set; }

        [JsonProperty("older_transfer_fee", NullValueHandling = NullValueHandling.Ignore)]
        public ErTransferFee OlderTransferFee { get; set; }

        [JsonProperty("withdraw_withheld_authority")]
        public object WithdrawWithheldAuthority { get; set; }

        [JsonProperty("transfer_fee_config_authority")]
        public object TransferFeeConfigAuthority { get; set; }
    }

    public partial class ErTransferFee
    {
        [JsonProperty("epoch", NullValueHandling = NullValueHandling.Ignore)]
        public long? Epoch { get; set; }

        [JsonProperty("maximum_fee", NullValueHandling = NullValueHandling.Ignore)]
        public double? MaximumFee { get; set; }

        [JsonProperty("transfer_fee_basis_points", NullValueHandling = NullValueHandling.Ignore)]
        public long? TransferFeeBasisPoints { get; set; }
    }

    public partial class Ownership
    {
        [JsonProperty("frozen", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Frozen { get; set; }

        [JsonProperty("delegated", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Delegated { get; set; }

        [JsonProperty("delegate")]
        public object Delegate { get; set; }

        [JsonProperty("ownership_model", NullValueHandling = NullValueHandling.Ignore)]
        public string OwnershipModel { get; set; }

        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public string Owner { get; set; }
    }

    public partial class Royalty
    {
        [JsonProperty("royalty_model", NullValueHandling = NullValueHandling.Ignore)]
        public string RoyaltyModel { get; set; }

        [JsonProperty("target")]
        public object Target { get; set; }

        [JsonProperty("percent", NullValueHandling = NullValueHandling.Ignore)]
        public double? Percent { get; set; }

        [JsonProperty("basis_points", NullValueHandling = NullValueHandling.Ignore)]
        public long? BasisPoints { get; set; }

        [JsonProperty("primary_sale_happened", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PrimarySaleHappened { get; set; }

        [JsonProperty("locked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Locked { get; set; }
    }

    public partial class Supply
    {
        [JsonProperty("print_max_supply", NullValueHandling = NullValueHandling.Ignore)]
        public long? PrintMaxSupply { get; set; }

        [JsonProperty("print_current_supply", NullValueHandling = NullValueHandling.Ignore)]
        public long? PrintCurrentSupply { get; set; }

        [JsonProperty("edition_nonce", NullValueHandling = NullValueHandling.Ignore)]
        public long? EditionNonce { get; set; }
    }

    public partial class TokenInfo
    {
        [JsonProperty("balance", NullValueHandling = NullValueHandling.Ignore)]
        public long? Balance { get; set; }

        [JsonProperty("supply", NullValueHandling = NullValueHandling.Ignore)]
        public double? Supply { get; set; }

        [JsonProperty("decimals", NullValueHandling = NullValueHandling.Ignore)]
        public long? Decimals { get; set; }

        [JsonProperty("token_program", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenProgram { get; set; }

        [JsonProperty("associated_token_address", NullValueHandling = NullValueHandling.Ignore)]
        public string AssociatedTokenAddress { get; set; }

        [JsonProperty("mint_authority", NullValueHandling = NullValueHandling.Ignore)]
        public string MintAuthority { get; set; }

        [JsonProperty("freeze_authority", NullValueHandling = NullValueHandling.Ignore)]
        public string FreezeAuthority { get; set; }

        [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
        public string Symbol { get; set; }

        [JsonProperty("price_info", NullValueHandling = NullValueHandling.Ignore)]
        public PriceInfo PriceInfo { get; set; }
    }

    public partial class PriceInfo
    {
        [JsonProperty("price_per_token", NullValueHandling = NullValueHandling.Ignore)]
        public double? PricePerToken { get; set; }

        [JsonProperty("total_price", NullValueHandling = NullValueHandling.Ignore)]
        public double? TotalPrice { get; set; }

        [JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
        public string Currency { get; set; }
    }
}
