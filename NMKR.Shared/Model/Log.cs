using System;

namespace NMKR.Shared.Model;

public partial class Log
{
    public int Id { get; set; }

    public string Logtext { get; set; }

    public DateTime Created { get; set; }

    public string Ipaddress { get; set; }
}
