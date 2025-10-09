using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Metadatatemplate
{
    public int Id { get; set; }

    public string Metadatatemplate1 { get; set; }

    public string Title { get; set; }

    public string Logo { get; set; }

    public string State { get; set; }

    public string Description { get; set; }

    public string Projecttype { get; set; }

    public virtual ICollection<Metadatatemplateadditionalfile> Metadatatemplateadditionalfiles { get; set; } = new List<Metadatatemplateadditionalfile>();

    public virtual ICollection<NftsArchive> NftsArchives { get; set; } = new List<NftsArchive>();
}
