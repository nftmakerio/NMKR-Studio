using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckPremintedAddresses : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkpremintedaddresses;
            if (server.Checkforpremintedaddresses == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);

            var addresses = await (from a in db.Premintednftsaddresses
                    .Include(a => a.Nftproject)
                    .AsSplitQuery()
                where a.State == "reserved"
                select a).ToListAsync(cancellationToken: cancellationToken);

            foreach (var adr in addresses)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await StaticBackgroundServerClass.LogAsync(db,
                    $"Check Preminted Address - State: {adr.State} - Address: {adr.Address}", "", serverid);

                adr.Lastcheckforutxo = DateTime.Now;
                var utxo = (await ConsoleCommand.GetNewUtxoAsync(adr.Address));
                adr.Lovelace = utxo.LovelaceSummary;



                foreach (var txInClass in utxo.TxIn)
                {
                    if (txInClass.Tokens==null)
                        continue;
                    foreach (var token in txInClass.Tokens)
                    {
                        string policy = token.PolicyId;
                        string tok = token.Tokenname;

                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Received Token and ADA {tok} - ADA:{adr.Lovelace}", "", serverid);

                        if (adr.Nftproject.Policyid != policy)
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Wrong Policy ID - Token will be sended back{policy}", "", serverid);
                            // TODO: Send back
                            continue;
                        }

                        var assetid = GlobalFunctions.GetAssetId(policy, "", tok);


                        var nftx = await (from a in db.Nfts
                                .Include(a => a.Nftproject)
                                .AsSplitQuery()
                            where a.NftprojectId == adr.NftprojectId && a.MainnftId == null
                                                                     && (a.Assetid == assetid)
                            select a).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                        if (nftx == null)
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"NFT not found in our Database - TODO: Token will be sended back{policy}", "",
                                serverid);
                            // TODO: Send back
                            adr.State = "inuse";
                            await db.SaveChangesAsync(cancellationToken);

                            continue;
                        }

                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Token found - NFT ID:{nftx.Id} - Set NFT to Available (Free)", "", serverid);


                        nftx.InstockpremintedaddressId = adr.Id;
                        nftx.State = "free";
                        nftx.Receiveraddress = null;
                        nftx.Selldate = null;
                        nftx.Buildtransaction = null;
                        adr.State = "inuse";
                        await db.SaveChangesAsync(cancellationToken);

                        await GlobalFunctions.ReleaseNftAsync(db, redis, nftx.Id);
                    }
                }
            }

            await db.SaveChangesAsync(cancellationToken);



            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }
    }
}
