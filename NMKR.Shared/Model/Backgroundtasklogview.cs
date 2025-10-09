using System;

namespace NMKR.Shared.Model;

public partial class Backgroundtasklogview
{
    public long Id { get; set; }

    public string Logmessage { get; set; }

    public DateTime Created { get; set; }
}
