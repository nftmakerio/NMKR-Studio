using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.Shared.Functions
{
    public static class RewardsClass
    {
        private static async Task<long> GetStakePoolRewards(EasynftprojectsContext db, IConnectionMultiplexer redis, string senderaddress)
        {
            var c=await (from a  in db.Stakepoolrewards
                         select a).CountAsync();
            if (c == 0) return 0;

            long stakerewards = 0;
            var stakinformation = await KoiosFunctions.GetStakePoolInformationAsync(redis, senderaddress);
            if (stakinformation == null) return stakerewards;
            var rewards = await(from a in db.Stakepoolrewards
                where a.Stakepoolid == stakinformation.DelegatedPool
                select a).FirstOrDefaultAsync();
            if (rewards != null)
                stakerewards = rewards.Reward;
            return stakerewards;
        }

        public static async Task<StakeAndTokenRewardClass> GetTokenAndStakeRewards(EasynftprojectsContext db, IConnectionMultiplexer redis, string senderaddress)
        {
            StakeAndTokenRewardClass res = new StakeAndTokenRewardClass
            {
                StakeReward = await GetStakePoolRewards(db, redis, senderaddress)
            };

            var rewards = await (from a in db.Tokenrewards  
                where a.State=="active"
                orderby a.Reward descending 
                select a).AsNoTracking().ToListAsync();

            if (!rewards.Any()) return res;

            var assets =
                await ConsoleCommand.GetAllAssetsInWalletAsync(redis, senderaddress);
              
            if (assets==null || !assets.Any()) return res;
            foreach (var reward in rewards.OrEmptyIfNull())
            {
                var t = string.IsNullOrEmpty(reward.Tokennameinhex)
                    ? assets.ToList().Find(x =>
                        x.PolicyIdOrCollection.Contains(reward.Policyid))
                    : assets.ToList().Find(x =>
                        x.Unit == (reward.Policyid + reward.Tokennameinhex ?? ""));
                if (t == null) continue;
                if (t.Quantity != null && t.Quantity >= reward.Mincount)
                {
                    res.TokenReward += reward.Reward;
                }
            }

            return res;
        }
      
    }
}
