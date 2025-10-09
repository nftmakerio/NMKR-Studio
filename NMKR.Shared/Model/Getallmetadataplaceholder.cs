namespace NMKR.Shared.Model;

public partial class Getallmetadataplaceholder
{
    public int Id { get; set; }

    /// <summary>
    /// Tokenprefix (from Projects) + Name = Assetname
    /// </summary>
    public string Name { get; set; }

    public string Placeholdername { get; set; }

    public string Placeholdervalue { get; set; }
}
