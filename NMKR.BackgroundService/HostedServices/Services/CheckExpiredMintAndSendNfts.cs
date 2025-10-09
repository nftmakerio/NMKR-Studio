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
    public class CheckExpiredMintAndSendNfts : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkexpiredreservations;
            if (server.Checkforexpirednfts == false || counter % 4!=0)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            var nftreservations = await (from a in db.Nftreservations
                where a.Mintandsendcommand == true && a.Reservationdate < DateTime.Now.AddHours(-4)
                select a).ToListAsync(cancellationToken: cancellationToken);

            foreach (var nftreservation in nftreservations)
            {
                var mintandsend = await (from a in db.Mintandsends
                    where a.Reservationtoken == nftreservation.Reservationtoken
                    select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (mintandsend == null)
                {
                    await NftReservationClass.ReleaseAllNftsAsync(db, redis,nftreservation.Reservationtoken, serverid);
                    continue;
                }

                if (mintandsend.State == "error" || mintandsend.State == "canceled")
                {
                    await NftReservationClass.ReleaseAllNftsAsync(db, redis,nftreservation.Reservationtoken, serverid);
                }
            }


            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }
    }
}
