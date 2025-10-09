using System;

namespace NMKR.Shared.DbSyncModel;

public partial class Metum
{
    public long Id { get; set; }

    public DateTime StartTime { get; set; }

    public string NetworkName { get; set; }

    public string Version { get; set; }
}
