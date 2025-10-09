using System;

namespace NMKR.Shared.Model;

public partial class Ipfsupload
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime Created { get; set; }

    public string Ipfshash { get; set; }

    public string Mimetype { get; set; }

    public string Name { get; set; }

    public long Filesize { get; set; }

    public virtual Customer Customer { get; set; }
}
