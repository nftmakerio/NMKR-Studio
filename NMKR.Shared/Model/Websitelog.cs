using System;

namespace NMKR.Shared.Model;

public partial class Websitelog
{
    public int Id { get; set; }

    public string Servername { get; set; }

    public int CustomerId { get; set; }

    public string Parameter { get; set; }

    public string Function { get; set; }

    public DateTime Created { get; set; }

    public virtual Customer Customer { get; set; }
}
