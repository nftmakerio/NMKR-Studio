using System;

namespace NMKR.Shared.DbSyncModel;

public partial class PoolOfflineFetchError
{
    public long Id { get; set; }

    public long PoolId { get; set; }

    public DateTime FetchTime { get; set; }

    public long PmrId { get; set; }

    public string FetchError { get; set; }

    public int RetryCount { get; set; }
}
