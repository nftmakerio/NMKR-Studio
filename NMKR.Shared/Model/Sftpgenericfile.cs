using System;

namespace NMKR.Shared.Model;

public partial class Sftpgenericfile
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public DateTime Created { get; set; }

    public string State { get; set; }

    public string Mimetype { get; set; }
}
