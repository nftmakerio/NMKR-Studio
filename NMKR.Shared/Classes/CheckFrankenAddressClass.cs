using NMKR.Shared.Classes.Koios;

namespace NMKR.Shared.Classes
{
    public class CheckFrankenAddressClass
    {
        public string Address { get; set; }
        public KoiosAddressTransactionsClass AddressTransactions { get; set; }

        /*
        public long LowestBlockHeight
        {
            get
            {
                var bh = AddressInformation.UtxoSet.OrderBy(x => x.BlockHeight).FirstOrDefault();
                if (bh!=null && bh.BlockHeight!=null)
                    return (long)bh.BlockHeight;

                return long.MaxValue;
            }
        }*/
    }
}
