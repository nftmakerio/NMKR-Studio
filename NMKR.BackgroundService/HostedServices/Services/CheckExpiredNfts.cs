using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckExpiredNfts : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkexpiredreservations;
            if (server.Checkforexpirednfts == false || counter % 5 !=0)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            var nfts = await (from a in db.Nfts
                where a.State == "reserved" && a.Reserveduntil != null && a.Reserveduntil < DateTime.Now.AddMinutes(-30)
                select a).ToListAsync(cancellationToken: cancellationToken);

            foreach (var nft in nfts)
            {
                var reservations = await (from a in db.Nftreservations
                    where a.NftId == nft.Id
                    select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (reservations != null)
                    continue;

                var project = await (from a in db.Nftprojects
                    where a.Id == nft.NftprojectId
                    select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (project.Maxsupply > 1)
                    continue;


                if (!StaticBackgroundServerClass.FoundNftInBlockchain(nft, project, mainnet, out var bfq, out var resultjson))
                {
                    await db.Database.ExecuteSqlRawAsync($"delete from nfttonftaddresses where nft_id={nft.Id}",
                        cancellationToken: cancellationToken);
                    nft.State = "free";
                    nft.Reservedcount = 0;
                    nft.Errorcount = 0;
                    nft.Soldcount = 0;
                    GlobalFunctions.DeleteStringFromRedis(redis, "nft_" + nft.Id);
                }
                else
                {
                    nft.State = "sold";
                    nft.Reservedcount = 0;
                    nft.Errorcount = 0;
                    nft.Soldcount = 1;
                }

                await db.SaveChangesAsync(cancellationToken);
            }



            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }
    }
}
