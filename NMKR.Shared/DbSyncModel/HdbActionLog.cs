using System;

namespace NMKR.Shared.DbSyncModel;

public partial class HdbActionLog
{
    public Guid Id { get; set; }

    public string ActionName { get; set; }

    public string InputPayload { get; set; }

    public string RequestHeaders { get; set; }

    public string SessionVariables { get; set; }

    public string ResponsePayload { get; set; }

    public string Errors { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResponseReceivedAt { get; set; }

    public string Status { get; set; }
}
