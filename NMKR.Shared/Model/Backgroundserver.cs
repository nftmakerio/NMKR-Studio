using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Backgroundserver
{
    public int Id { get; set; }

    public string Ipaddress { get; set; }

    public string Url { get; set; }

    public string Name { get; set; }

    public string State { get; set; }

    public bool Checkpaymentaddresses { get; set; }

    public bool Checkdoublepayments { get; set; }

    public bool Checkpolicies { get; set; }

    public bool Executedatabasecommands { get; set; }

    public bool Checkforfreepaymentaddresses { get; set; }

    public bool Checkcustomeraddresses { get; set; }

    public bool Checkforpremintedaddresses { get; set; }

    public bool Executepayoutrequests { get; set; }

    public bool Checkfordoublepayments { get; set; }

    public bool Checkforexpirednfts { get; set; }

    public bool Checkforburningendpoints { get; set; }

    public bool Checkprojectaddresses { get; set; }

    public bool Checkmintandsend { get; set; }

    public bool Checklegacyauctions { get; set; }

    public bool Checklegacydirectsales { get; set; }

    public bool Checknotificationqueue { get; set; }

    public bool Executesubmissions { get; set; }

    public bool Checkdecentralsubmits { get; set; }

    public bool Checkroyaltysplitaddresses { get; set; }

    public bool Checktransactionconfirmations { get; set; }

    public bool Checkbuyinsmartcontractaddresses { get; set; }

    public bool Checkcustomerchargeaddresses { get; set; }

    public bool Stopserver { get; set; }

    public bool Pauseserver { get; set; }

    public int Ratelimitperminute { get; set; }

    public int Mintxcheckdoublepayments { get; set; }

    public DateTime Lastlifesign { get; set; }

    public string Runningversion { get; set; }

    public string Nodeversion { get; set; }

    public bool Monitorthisserver { get; set; }

    public string Actualtask { get; set; }

    public bool Checkvalidationaddresses { get; set; }

    public int? Actualprojectid { get; set; }

    public string Syncprogress { get; set; }

    public string Epoch { get; set; }

    public string Slot { get; set; }

    public string Block { get; set; }

    public string Era { get; set; }

    public string Operatingsystem { get; set; }

    public bool Checkpaymentaddressessolana { get; set; }

    public bool Checkmintandsendsolana { get; set; }

    public bool Checkpoliciessolana { get; set; }

    public string Installedmemory { get; set; }

    public bool Checkcustomeraddressessolana { get; set; }

    public bool Checkrates { get; set; }

    public string Checkpaymentaddressescoin { get; set; }

    public string Checkmintandsendcoin { get; set; }

    public string Checkpoliciescoin { get; set; }

    public string Checkcustomeraddressescoin { get; set; }

    public bool Digitaloceanserver { get; set; }

    public virtual ICollection<Serverexception> Serverexceptions { get; set; } = new List<Serverexception>();

    public virtual ICollection<Soldnft> Soldnfts { get; set; } = new List<Soldnft>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
