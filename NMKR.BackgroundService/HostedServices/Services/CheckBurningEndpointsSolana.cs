using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckBurningEndpointsSolana : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkburningaddresses;
            if (server.Checkforburningendpoints == false)
                return;


            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);

            var addresses = await (from a in db.Burnigendpoints
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Settings)
                    .AsSplitQuery()
                where a.Validuntil > DateTime.Now && a.State == "active" &&
                      a.Blockchain == Blockchain.Solana.ToString()
                select a).ToListAsync(cancellationToken: cancellationToken);

            foreach (var adr in addresses)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await StaticBackgroundServerClass.LogAsync(db,
                    $"Check Burning Address: {adr.Address} - Project: {adr.Nftproject.Id} {adr.Nftproject.Projectname}",
                    "", serverid);


                var assets = await SolanaFunctions.GetAllAssetsInWalletAsync(redis, adr.Address);
                foreach (var assetsAssociatedWithAccount in assets)
                {
                    var paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db, serverid, "CheckBurningEndpointsSolana",null,Coin.SOL);
                    if (paywallet == null)
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Mint and Send - All Paywallets are blocked - waiting",
                            "", serverid);
                        break;
                    }

                    var res = await SolanaFunctions.BurnSolanaNftAsync(paywallet, adr,
                        assetsAssociatedWithAccount.Address, new BuildTransactionClass());
                    if (!string.IsNullOrEmpty(res.TxHash))
                    {
                        await ShowNmkrStudioNotification(db, cancellationToken, adr);
                    }
                    else
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Error while burning NFT - ",
                            res.LogFile, serverid);
                    }

                    await GlobalFunctions.UnlockPaywalletAsync(db, paywallet);
                    await db.SaveChangesAsync(cancellationToken);
                }

            }
        }

        private static async Task ShowNmkrStudioNotification(EasynftprojectsContext db,
            CancellationToken cancellationToken,
            Burnigendpoint adr)
        {
            if (adr.Shownotification)
            {
                Onlinenotification on = new()
                {
                    Created = DateTime.Now,
                    CustomerId = adr.Nftproject.CustomerId,
                    Notificationmessage =
                        "The token(s), you have send to the burning address were burned",
                    State = "new",
                    Color = "success"
                };
                await db.Onlinenotifications.AddAsync(on, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
        }

    }
}
