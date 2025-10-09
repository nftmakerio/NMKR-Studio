using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Blockfrost;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckMintedNftsAgainstBlockchainCardano : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkpolicies;
            if (server.Checkpolicies == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            await StaticBackgroundServerClass.LogAsync(db, "Check CheckMintedNftsAgainstBlockchain Cardano ", "", serverid);

            var nfts1 = await (from a in db.Getidsforpolicychecks
                               where a.Mintedonblockchain==Blockchain.Cardano.ToString()
                select a).Take(200).AsNoTracking().ToListAsync(cancellationToken);


            await StaticBackgroundServerClass.LogAsync(db, $"Found {nfts1.Count} open Nfts", "", serverid);


            foreach (var nft1 in nfts1)
            {
                var nft = await (from a in db.Nfts
                        .Include(a => a.Nftproject)
                        .AsSplitQuery()
                    where a.Id == nft1.Id
                    select a).FirstOrDefaultAsync(cancellationToken);

                if (nft == null)
                    continue;


                if (cancellationToken.IsCancellationRequested)
                    break;
             
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Checking Policy for  {nft.Nftproject.Policyid}.{(nft.Nftproject.Tokennameprefix ?? "")}{nft.Name} - {nft.Assetid} - {nft.Nftproject.Projectname} - {nft.NftprojectId} - NFTID: {nft.Id}",
                    "", serverid);

                BlockfrostAssetClass as1 = null;
                if (nft.Nftproject.Enabledcoins.Contains(Coin.ADA.ToString()) == true)
                    as1 = await ConsoleCommand.GetAssetFromBlockchainAsync(nft,nft.Nftproject);

                if (as1 == null)
                {
                    await TokenNotFound(db, cancellationToken, serverid, nft);
                    continue;
                }

                if ((nft.Fingerprint == null || nft.Minted == false || nft.Checkpolicyid == true) &&
                    !string.IsNullOrEmpty(as1.AssetName) && !string.IsNullOrEmpty(as1.Fingerprint))
                {
                    await ConfirmTokenAsMinted(db, cancellationToken, serverid, nft, as1);
                }

            }

            await StaticBackgroundServerClass.LogAsync(db, "Check Policyids on Cardano ended.", "", serverid);


            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private static async Task ConfirmTokenAsMinted(EasynftprojectsContext db, CancellationToken cancellationToken,
            int serverid, Nft nft, BlockfrostAssetClass as1)
        {
            await StaticBackgroundServerClass.LogAsync(db,
                $"NFT {nft.Nftproject?.Tokennameprefix}{nft.Name} is now confirmed as Minted - NFTID: {nft.Id}",
                "", serverid);
          //  nft.Series = as1.OnchainMetadata?.ArtworkTitle;
            nft.Assetname = as1.AssetName;
            nft.Fingerprint = as1.Fingerprint;
            nft.Initialminttxhash = as1.InitialMintTxHash;
         //   nft.Title = as1.OnchainMetadata?.Name;
            nft.Minted = true;

            if (nft.State != "burned")
            {
                if (as1.Quantity >= nft.Nftproject?.Maxsupply && nft.InstockpremintedaddressId == null)
                    nft.State = "sold";
            }

            bool disablecheck = true;
            if (nft.Nftproject?.Maxsupply == 1)
            {
                if (as1.Quantity == 0)
                {
                    if (as1.MintOrBurnCount == 2)
                    {
                        nft.Checkpolicyid = false;
                        nft.Lastpolicycheck = null;
                        nft.Soldcount = 0;
                        nft.Reservedcount = 0;
                        nft.Errorcount = 0;
                        nft.State = "burned";
                    }
                }
                else
                {
                    nft.Checkpolicyid = false;
                    nft.Lastpolicycheck = null;
                    nft.Soldcount = 1;
                    nft.Reservedcount = 0;
                    nft.Errorcount = 0;
                }
            }
            else
            {
                if (nft.Soldcount == (as1.Quantity ?? 0))
                {
                    nft.Lastpolicycheck = null;
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Soldcount is correct {nft.Id} - {nft.Nftproject?.Tokennameprefix}{nft.Name}", "",
                        serverid);
                }

                if (nft.Soldcount < (as1.Quantity ?? 0))
                {
                    nft.Soldcount = (as1.Quantity ?? 0);
                    nft.Lastpolicycheck = null;
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Found more NFT on Blockfrost {nft.Id} - {nft.Nftproject?.Tokennameprefix}{nft.Name} then in the database - new soldcount is now: {nft.Soldcount}",
                        "", serverid);
                }

                if (nft.Soldcount > (as1.Quantity ?? 0))
                {
                    disablecheck = false;
                    if (nft.Lastpolicycheck == null)
                    {
                        nft.Lastpolicycheck = DateTime.Now;
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Found less NFT on Blockfrost {nft.Id} - {nft.Nftproject?.Tokennameprefix}{nft.Name} setting next check time in 60 minutes",
                            "", serverid);
                    }

                    if (nft.Lastpolicycheck < DateTime.Now.AddMinutes(60))
                    {
                        nft.Lastpolicycheck = null;
                        nft.Soldcount = (as1.Quantity ?? 0);
                        disablecheck = true;
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Found less NFT on Blockfrost {nft.Id} - {nft.Nftproject?.Tokennameprefix}{nft.Name} then in the database - new soldcount is now: {nft.Soldcount}",
                            "", serverid);
                    }
                }
            }

            if (nft.Checkpolicyid == true && disablecheck)
                nft.Checkpolicyid = false;
            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task TokenNotFound(EasynftprojectsContext db, CancellationToken cancellationToken, int serverid,
            Nft nft)
        {
            await StaticBackgroundServerClass.LogAsync(db,
                $"Asset was minted, but not found on Blockchain {nft.Name} - {nft.Assetid} - NFTID: {nft.Id}", "", serverid);

            if (nft.Lastpolicycheck == null)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Setting ExpiringPolicyCheck for  {nft.Name} -  - NFTID: {nft.Id} - {nft.Assetid}in 2 hours",
                    "", serverid);

                nft.Lastpolicycheck = DateTime.Now;
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                if (nft.Lastpolicycheck < DateTime.Now.AddMinutes(-120))
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"LastPolicyCheck later than 120 minutes for  {nft.Name} - {nft.Assetid} - NFTID: {nft.Id}",
                        "", serverid);
                    if (nft.Soldcount > 0 || nft.State == "sold" ||
                        nft.State == "error" && nft.Nftproject.Maxsupply == 1)
                    {
                        var nto = await (from a in db.Nfttonftaddresses
                                .Include(a => a.Nftaddresses)
                                .AsSplitQuery()
                            where a.NftId == nft.Id
                            select a).AsNoTracking().ToListAsync(cancellationToken);

                        var tx1 = await (from a in db.TransactionNfts
                            where a.NftId == nft.Id
                            select a).AsNoTracking().ToListAsync(cancellationToken);

                        if (!nto.Any() && !tx1.Any())
                        {
                            if (nft.State == "sold")
                            {
                                if (!nft.Isroyaltytoken)
                                {
                                    await StaticBackgroundServerClass.LogAsync(db,
                                        $"Setting NFT to error (1) Cardano {nft.Name} - {nft.Assetid} - NFTID: {nft.Id}",
                                        nft.Id.ToString(), serverid);
                                    nft.Fingerprint = null;
                                    nft.State = "error";
                                    nft.Markedaserror = DateTime.Now;
                                    nft.Soldcount = 0;
                                    nft.Reservedcount = 0;
                                    nft.Errorcount = 0;
                                    nft.Initialminttxhash = null;
                                    nft.Series = null;
                                    nft.Lastpolicycheck = null;
                                    nft.Checkpolicyid = true;
                                    nft.Buildtransaction = "NFT was marked as 'sold' but the asset was not found on the blockchain";
                                }
                            }
                            else
                            {
                                await StaticBackgroundServerClass.LogAsync(db,
                                    $"Setting NFT to free (1) {nft.Name} - {nft.Assetid} - NFTID: {nft.Id}", "",
                                    serverid);
                                nft.Fingerprint = null;
                                nft.State = "free";
                                nft.Markedaserror = null;
                                nft.Soldcount = 0;
                                nft.Reservedcount = 0;
                                nft.Errorcount = 0;
                                nft.Initialminttxhash = null;
                                nft.Series = null;
                                nft.Lastpolicycheck = null;
                            }
                        }
                        else
                        {
                            bool settofree = false;
                            if (nto.Count == 1)
                            {
                                if (nto.First().Nftaddresses.State == "error" ||
                                    nto.First().Nftaddresses.State == "error2")
                                    settofree = true;
                            }

                            if (settofree)
                            {
                                await StaticBackgroundServerClass.LogAsync(db,
                                    $"Setting NFT to free (2){nft.Name} - {nft.Assetid} - NFTID: {nft.Id}", "",
                                    serverid);
                                await db.Database.ExecuteSqlRawAsync(
                                    $"delete from nfttonftaddresses where nft_id={nft.Id}",
                                    cancellationToken: cancellationToken);
                                nft.Fingerprint = null;
                                nft.State = "free";
                                nft.Soldcount = 0;
                                nft.Reservedcount = 0;
                                nft.Errorcount = 0;
                                nft.Initialminttxhash = null;
                                nft.Series = null;
                                nft.Lastpolicycheck = null;
                            }
                            else
                            {
                                await StaticBackgroundServerClass.LogAsync(db,
                                    $"Setting NFT to error Cardano {nft.Name} - {nft.Assetid} - NFTID: {nft.Id}", nft.Id.ToString(),
                                    serverid);
                                nft.Fingerprint = null;
                                nft.State = "error";
                                nft.Soldcount = 0;
                                nft.Reservedcount = 0;
                                nft.Errorcount = 1;
                                nft.Initialminttxhash = null;
                                nft.Series = null;
                                nft.Lastpolicycheck = null;
                                nft.Markedaserror = DateTime.Now;
                                nft.Buildtransaction = "NFT was marked as 'sold' but the asset was not found on the blockchain(2).";
                            }
                        }
                    }
                    else
                    {
                        nft.Checkpolicyid = false;
                    }

                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}
