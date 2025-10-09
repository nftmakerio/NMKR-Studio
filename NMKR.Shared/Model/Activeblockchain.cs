using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Activeblockchain
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Image { get; set; }

    public bool Enabled { get; set; }

    public string Smallestentity { get; set; }

    public string Coinname { get; set; }

    public string Explorerurladdress { get; set; }

    public string Explorerurltx { get; set; }

    public string Explorerurlcollection { get; set; }

    public long Factor { get; set; }

    public bool? Hasnft { get; set; }

    public bool Hasft { get; set; }

    public bool Collectionmustbecreatedonnewproject { get; set; }

    public bool Collectionaddressmustbefunded { get; set; }

    public virtual ICollection<Nftprojectsendpremintedtoken> Nftprojectsendpremintedtokens { get; set; } = new List<Nftprojectsendpremintedtoken>();

    public virtual ICollection<Premintedpromotokenaddress> Premintedpromotokenaddresses { get; set; } = new List<Premintedpromotokenaddress>();
}
