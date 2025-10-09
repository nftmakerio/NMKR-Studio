using System;

namespace NMKR.Shared.Model;

public partial class Backgroundtaskslog
{
    public long Id { get; set; }

    public string Logmessage { get; set; }

    public DateTime Created { get; set; }

    public string Additionaldata { get; set; }
}
