using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Smartcontract
{
    public int Id { get; set; }

    public string Smartcontractname { get; set; }

    public string Filename { get; set; }

    public string Hashaddress { get; set; }

    public string State { get; set; }

    public string Type { get; set; }

    public string Address { get; set; }

    public string Sourcecode { get; set; }

    public string Plutus { get; set; }

    public long? Timevalue { get; set; }

    public long? Memvalue { get; set; }

    /// <summary>
    /// The project we will use if no other project is specified. The project is only for the settings
    /// </summary>
    public int? DefaultprojectId { get; set; }

    public virtual Nftproject Defaultproject { get; set; }

    public virtual ICollection<Directsale> Directsales { get; set; } = new List<Directsale>();

    public virtual ICollection<Nftproject> Nftprojects { get; set; } = new List<Nftproject>();

    public virtual ICollection<Preparedpaymenttransaction> Preparedpaymenttransactions { get; set; } = new List<Preparedpaymenttransaction>();

    public virtual ICollection<Smartcontractsjsontemplate> Smartcontractsjsontemplates { get; set; } = new List<Smartcontractsjsontemplate>();
}
