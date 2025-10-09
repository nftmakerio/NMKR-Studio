using System;
using System.Collections.Generic;

namespace NMKR.Shared.DbSyncModel;

public partial class HdbCronEvent
{
    public string Id { get; set; }

    public string TriggerName { get; set; }

    public DateTime ScheduledTime { get; set; }

    public string Status { get; set; }

    public int Tries { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? NextRetryAt { get; set; }

    public virtual ICollection<HdbCronEventInvocationLog> HdbCronEventInvocationLogs { get; set; } = new List<HdbCronEventInvocationLog>();
}
