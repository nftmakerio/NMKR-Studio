namespace NMKR.Shared.Model;

public partial class Pricelistdiscount
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public string Condition { get; set; }

    public string Policyid { get; set; }

    public long? Minvalue { get; set; }

    public string State { get; set; }

    public string Description { get; set; }

    public string Policyprojectname { get; set; }

    public string Policyid2 { get; set; }

    public string Policyid3 { get; set; }

    public string Policyid4 { get; set; }

    public string Policyid5 { get; set; }

    public string Whitlistaddresses { get; set; }

    public float Sendbackdiscount { get; set; }

    public string Operator { get; set; }

    public long? Minvalue2 { get; set; }

    public long? Minvalue3 { get; set; }

    public long? Minvalue4 { get; set; }

    public long? Minvalue5 { get; set; }

    public string Referercode { get; set; }

    public string Couponcode { get; set; }

    public string Blockchain { get; set; }

    public virtual Nftproject Nftproject { get; set; }
}
