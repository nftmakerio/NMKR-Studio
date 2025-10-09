using System;

namespace NMKR.Shared.Classes
{
    public class GetRefundsClass
    {
        public DateTime Created { get; set; }
       public string State { get; set; }
        public int NftprojectId { get; set; }
        public string Senderaddress { get; set; }
        public string Receiveraddress { get; set; }
        public string Refundreason { get; set; }
        public string Txhash { get; set; }
        public string Outgoingtxhash { get; set; }
        public long Lovelace { get; set; }
        public long Fee { get; set; }
    }
}
