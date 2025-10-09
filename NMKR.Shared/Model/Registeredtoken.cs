using System;

namespace NMKR.Shared.Model;

public partial class Registeredtoken
{
    public int Id { get; set; }

    public DateTime Created { get; set; }

    public string Subject { get; set; }

    public string Policyid { get; set; }

    public string Url { get; set; }

    public string Name { get; set; }

    public string Ticker { get; set; }

    public int? Decimals { get; set; }

    public string Logo { get; set; }

    public string Description { get; set; }
}
