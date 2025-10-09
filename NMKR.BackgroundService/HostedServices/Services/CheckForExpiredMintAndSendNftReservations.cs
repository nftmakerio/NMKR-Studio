

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
    public class CheckForExpiredMintAndSendNftReservations : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkexpiredreservations;
            if (server.Checkforexpirednfts == false || counter % 4 !=0)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            var rn = await (from a in db.Nftreservations
                            where
                                a.Reservationdate <
                                DateTime.Now.AddMinutes(-70) &&
                                a.Mintandsendcommand == true
                            group a by a.Reservationtoken
                into rt
                            select new { tokenname = rt.Key }).AsNoTracking().ToListAsync(cancellationToken: cancellationToken);

            foreach (var nftreservation in rn)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Cleaning up nftreservations {nftreservation.tokenname}", "", serverid);

                var z = await (from a in db.Mintandsends
                    where a.Reservationtoken == nftreservation.tokenname
                    select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (z is {State: "execute" or "inprogress"})
                {
                    continue;
                }

                var u = await (from a in db.Nftreservations
                    where a.Reservationtoken == nftreservation.tokenname
                    select a).AsNoTracking().ToListAsync(cancellationToken: cancellationToken);

                var release = true;
                foreach (var nn1 in u)
                {
                    var nft = await (from a in db.Nfts
                            .Include(a => a.Nftproject)
                            .AsSplitQuery()
                        where a.Id == nn1.NftId
                        select a).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (nft.State != "reserved")
                    {
                        await NftReservationClass.DeleteTokenAsync(db, nftreservation.tokenname);
                        release = false;
                        continue;
                    }


                    if (nft.State!="sold" && StaticBackgroundServerClass.FoundNftInBlockchain(nft, nft.Nftproject, mainnet, out var bfq, out var resultjson))
                    {
                        nft.Checkpolicyid = true;
                        // This should not be happen - but we will check it before minting on blockfrost
                        if (bfq >= nft.Nftproject.Maxsupply || z==null || z.State == "success")
                        {
                            if (nft.Nftproject.Maxsupply == 1)
                            {
                                nft.Soldcount = 1;
                                nft.State = "sold";
                                nft.Reservedcount = 0;
                            }

                            release = false;

                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Error - NFT is marked as sold from blockfrost:  Nft-Id: {nft.Id} - {nft.Name}",
                                "", serverid);
                            await db.SaveChangesAsync(cancellationToken);
                        }
                    }
                }

                await StaticBackgroundServerClass.LogAsync(db,
                    $"Remove expired reservation - is longer than 1 hours {nftreservation.tokenname}", "",
                    serverid);
                if (release)
                    await NftReservationClass.ReleaseAllNftsAsync(db, redis, nftreservation.tokenname, serverid);
            }




            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }
    }
}
