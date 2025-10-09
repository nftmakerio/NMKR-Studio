using System;

namespace NMKR.Shared.Model;

public partial class Txhashcache
{
    public int Id { get; set; }

    public string Txhash { get; set; }

    public string Transactionobject { get; set; }

    public DateTime Created { get; set; }
}
