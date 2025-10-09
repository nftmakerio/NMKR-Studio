using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using NMKR.Shared.NotificationClasses;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckForExpiredAddressesSolana : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkexpiredpaymentaddresses;
            if (server.Checkpaymentaddressessolana == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);

            try
            {
                var exp = await (from a in db.Nftaddresses
                                 where (a.State == "active" || a.State == "error") && a.Expires < DateTime.Now.AddMinutes(-1) &&
                                       a.Addresscheckedcounter >= 4 && (a.Serverid == null || a.Serverid == serverid) && a.Coin == Coin.SOL.ToString()
                                 select a).ToListAsync(cancellationToken: cancellationToken);

                await StaticBackgroundServerClass.LogAsync(db, $"{exp.Count} Expired Addresses to check");
                int c = 0;
                foreach (var exp1 in exp)
                {
                    c++;
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await StaticBackgroundServerClass.LogAsync(db,
                        $"{c} of {exp.Count} - Expired Payment Address {exp1.Address} - Expired: {exp1.Expires} - State: {exp1.State}",
                        "", serverid);
                    // First Check utxo
                    var sol = await SolanaFunctions.GetWalletBalanceAsync(exp1.Address);

                    if (sol > 0 && exp1.State != "error")
                    {

                        exp1.Utxo = (long)sol;
                        exp1.Lastcheckforutxo = DateTime.Now;
                        await db.SaveChangesAsync(cancellationToken);

                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Found SOL on expired address: {exp1.Address} - Lamport: {sol}", "",
                            serverid);
                    }

                    // Nothing found - delete
                    exp1.State = "expired";
                    try
                    {
                        await db.SaveChangesAsync(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        await StaticBackgroundServerClass.EventLogException(db, 975, e, serverid);

                    }

                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Set Address to expired - send to Rabbit MQ: {exp1.Address}", exp1.Address,
                        serverid);
                    // Send Notifications via NotificationServer
                    await bus.Publish(new RmqTransactionClass { AddressId = exp1.Id, ProjectId = exp1.NftprojectId, EventType = NotificationEventTypes.addressexpired }, cancellationToken);

                    await GlobalFunctions.ReleaseNftAsync(db, redis, exp1.Id);
                }
            }
            catch (Exception e)
            {
                GlobalFunctions.ResetContextState(db);
                await StaticBackgroundServerClass.EventLogException(db, 3, e, serverid);
            }


            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);

        }
    }
}
