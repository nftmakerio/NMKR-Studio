using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Whitelabelstore
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int ProjectId { get; set; }

    public string Storename { get; set; }

    public string State { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual Nftproject Project { get; set; }

    public virtual ICollection<Whitelabelstorecollection> Whitelabelstorecollections { get; } = new List<Whitelabelstorecollection>();

    public virtual ICollection<Whitelabelstoresetting> Whitelabelstoresettings { get; } = new List<Whitelabelstoresetting>();
}
