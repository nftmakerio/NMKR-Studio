using System;

namespace NMKR.Shared.Model;

public partial class Notificationqueue
{
    public int Id { get; set; }

    public string State { get; set; }

    public string Notificationtype { get; set; }

    public string Notificationendpoint { get; set; }

    public string Payload { get; set; }

    public int? ServerId { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Processed { get; set; }

    public string Result { get; set; }

    public int Counterrors { get; set; }

    public DateTime? Lasterror { get; set; }
}
