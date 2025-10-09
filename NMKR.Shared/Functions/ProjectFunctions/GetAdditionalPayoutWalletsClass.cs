using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Classes.Projects;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.ProjectFunctions
{
    public static class GetAdditionalPayoutWalletsClass
    {
        public static async Task<IEnumerable<AdditionalPayoutWalletsClass>> GetAdditionalPayoutWallets(EasynftprojectsContext db, Nftproject project, IConnectionMultiplexer redis)
        {
            var pl = await (from a in db.Nftprojectsadditionalpayouts
                    .Include(a => a.Wallet)
                where a.NftprojectId == project.Id
                select new AdditionalPayoutWalletsClass()
                {
                    Custompropertycondition = a.Custompropertycondition,
                    Coin = a.Coin.ToEnum<Coin>(), Valuepercent = a.Valuepercent, Valuetotal = a.Valuetotal,
                    WalletAddress = a.Wallet.Walletaddress,
                    Blockchain = GlobalFunctions.ConvertToBlockchain(a.Coin.ToEnum<Coin>())
                }).ToListAsync();

            return pl;
        }
    }
}
