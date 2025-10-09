namespace NMKR.Shared.Model;

public partial class PreparedpaymenttransactionsNft
{
    public int Id { get; set; }

    public int PreparedpaymenttransactionsId { get; set; }

    public int? NftId { get; set; }

    public long Count { get; set; }

    public string Policyid { get; set; }

    public string Tokenname { get; set; }

    public string Tokennamehex { get; set; }

    public long? Lovelace { get; set; }

    public string Nftuid { get; set; }

    public virtual Nft Nft { get; set; }

    public virtual Preparedpaymenttransaction Preparedpaymenttransactions { get; set; }
}
