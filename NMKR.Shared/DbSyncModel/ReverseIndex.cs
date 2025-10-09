namespace NMKR.Shared.DbSyncModel;

public partial class ReverseIndex
{
    public long Id { get; set; }

    public long BlockId { get; set; }

    public string MinIds { get; set; }
}
