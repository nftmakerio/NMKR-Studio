using System;

namespace NMKR.Shared.Model;

public partial class Adminlogin
{
    public int Id { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string Salt { get; set; }

    public string State { get; set; }

    public DateTime Created { get; set; }

    public string Ipaddress { get; set; }

    public int Failedlogon { get; set; }

    public string Twofactor { get; set; }

    public string Mobilenumber { get; set; }

    public DateTime? Lockeduntil { get; set; }

    public string Pendingpassword { get; set; }

    public DateTime? Pendingpasswordcreated { get; set; }
}
