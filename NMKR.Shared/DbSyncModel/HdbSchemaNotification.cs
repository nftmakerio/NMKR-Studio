using System;

namespace NMKR.Shared.DbSyncModel;

public partial class HdbSchemaNotification
{
    public int Id { get; set; }

    public string Notification { get; set; }

    public int ResourceVersion { get; set; }

    public Guid InstanceId { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
