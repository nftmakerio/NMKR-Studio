using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class SmartcontractsUtxo
{
    public int Id { get; set; }

    public int SmartcontractId { get; set; }

    public string Txhash { get; set; }

    public long Txid { get; set; }

    public long Lovelace { get; set; }

    public string Datumhash { get; set; }

    public virtual Smartcontract Smartcontract { get; set; }

    public virtual ICollection<SmartcontractsUtxosToken> SmartcontractsUtxosTokens { get; } = new List<SmartcontractsUtxosToken>();
}
