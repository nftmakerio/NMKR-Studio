using System;

namespace NMKR.Shared.Model;

public partial class Ratelimit
{
    public int Id { get; set; }

    public string Apikey { get; set; }

    public DateTime Created { get; set; }
}
