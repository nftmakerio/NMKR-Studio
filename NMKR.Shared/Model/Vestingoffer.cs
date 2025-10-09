namespace NMKR.Shared.Model;

public partial class Vestingoffer
{
    public int Id { get; set; }

    public int Periodindays { get; set; }

    public bool Iagonenabled { get; set; }

    public long Maxfilesize { get; set; }

    public long Maxfiles { get; set; }

    public long Maxstorage { get; set; }

    public bool Extendedapienabled { get; set; }

    public string Vesttokenpolicyid { get; set; }

    public string Vesttokenassetname { get; set; }

    public long Vesttokenquantity { get; set; }

    public long Vesttokenada { get; set; }

    public string Description { get; set; }
}
