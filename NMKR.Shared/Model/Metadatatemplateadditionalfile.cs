namespace NMKR.Shared.Model;

public partial class Metadatatemplateadditionalfile
{
    public int Id { get; set; }

    public int MetadatatemplateId { get; set; }

    public string Filename { get; set; }

    public string Filetypes { get; set; }

    public virtual Metadatatemplate Metadatatemplate { get; set; }
}
