using System.Collections.Generic;
using NMKR.Shared.Model;

namespace NMKR.Pro.Classes
{
    public class DashboardSalesClass
    {
        public List<Transaction> transactions = new();
        public string totalAda { get; set; }
        public string totalsold { get; set; }
        public string totalsales { get; set; }
        public string last24h { get; set; }
        public string saleslast24h { get; set; }
    }
}
