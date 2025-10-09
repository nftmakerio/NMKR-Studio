using System;

namespace NMKR.Shared.Model;

public partial class Onlinenotification
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Notificationmessage { get; set; }

    public DateTime Created { get; set; }

    public string State { get; set; }

    public string Color { get; set; }

    public virtual Customer Customer { get; set; }
}
