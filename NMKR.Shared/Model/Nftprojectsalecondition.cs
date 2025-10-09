using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Nftprojectsalecondition
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public string Condition { get; set; }

    public string Policyid { get; set; }

    public long? Maxvalue { get; set; }

    public string State { get; set; }

    public string Description { get; set; }

    public string Policyprojectname { get; set; }

    public string Policyid2 { get; set; }

    public string Policyid3 { get; set; }

    public string Policyid4 { get; set; }

    public string Policyid5 { get; set; }

    public string Whitlistaddresses { get; set; }

    public bool Onlyonesaleperwhitlistaddress { get; set; }

    public string Usedwhitelistaddresses { get; set; }

    public string Blacklistedaddresses { get; set; }

    public string Operator { get; set; }

    public string Policyid6 { get; set; }

    public string Policyid7 { get; set; }

    public string Policyid8 { get; set; }

    public string Policyid9 { get; set; }

    public string Policyid10 { get; set; }

    public string Policyid11 { get; set; }

    public string Policyid12 { get; set; }

    public string Policyid13 { get; set; }

    public string Policyid14 { get; set; }

    public string Policyid15 { get; set; }

    public string Blockchain { get; set; }

    public virtual ICollection<Countedwhitelist> Countedwhitelists { get; set; } = new List<Countedwhitelist>();

    public virtual Nftproject Nftproject { get; set; }

    public virtual ICollection<Usedaddressesonsalecondition> Usedaddressesonsaleconditions { get; set; } = new List<Usedaddressesonsalecondition>();
}
