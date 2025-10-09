using System;

namespace NMKR.Shared.Model;

public partial class Serverexception
{
    public int Id { get; set; }

    public string Logmessage { get; set; }

    public string Data { get; set; }

    public DateTime Created { get; set; }

    public int? ServerId { get; set; }

    public virtual Backgroundserver Server { get; set; }
}
