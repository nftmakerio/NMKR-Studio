using System;

namespace NMKR.Shared.Classes
{
    public class MintAndSendJobsExportClass
    {
            public int NftprojectId { get; set; }

            public string Receiveraddress { get; set; }

            public DateTime Created { get; set; }

            public string State { get; set; }

            public DateTime? Executed { get; set; }

            public string Transactionid { get; set; }
            public string StakeAddress { get; set; }
    }
}
