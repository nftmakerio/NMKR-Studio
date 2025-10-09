using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Cardano_Sharp;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Blockfrost;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckLegacyAuctionAddresses : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checklegacyauctions;
            if (server.Checklegacyauctions == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            var auctionaddresses = await (from a in db.Legacyauctions
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Smartcontractssettings)
                    .AsSplitQuery()
                    .Include(a => a.LegacyauctionsNfts)
                    .AsSplitQuery()
                where a.State == "active" || a.State == "finished" || a.State == "waitforlock"
                select a).ToListAsync(cancellationToken: cancellationToken);

            foreach (var auctionaddress in auctionaddresses)
            {

                // End the auction, if there was no NFT locking
                if (auctionaddress.State == "waitforlock")
                {
                    if (auctionaddress.Runsuntil < DateTime.Now)
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Set auction to ended - {auctionaddress.Address}", "", serverid);
                        auctionaddress.State = "ended";
                        await db.SaveChangesAsync(cancellationToken);
                        continue;
                    }
                }
                if (auctionaddress.Runsuntil < DateTime.Now.AddDays(-2) && auctionaddress.State == "finished")
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Set auction to ended - {auctionaddress.Address}", "", serverid);
                    auctionaddress.State = "ended";
                    await db.SaveChangesAsync(cancellationToken);
                    continue;
                }

                await StaticBackgroundServerClass.LogAsync(db,
                    $"Check Legacy Auction Address {auctionaddress.Address}", "", serverid);
                var utxo = await ConsoleCommand.GetNewUtxoAsync(auctionaddress.Address);
                if (utxo == null)
                    continue;
                if (utxo.TxIn == null || !utxo.TxIn.Any())
                    continue;

                foreach (var txInClass in utxo.TxIn.OrderByDescending(x => x.Lovelace))
                {
                    var legacyhistory = await (from a in db.Legacyauctionshistories
                        where a.Txhash == txInClass.TxHashId
                        select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);
                    if (legacyhistory != null && (legacyhistory.State == "seller" || legacyhistory.State == "outbid" ||
                                                  legacyhistory.State == "invalid" || legacyhistory.State == "expired"))
                        continue;

                    var txdate = DateTime.Now;
                    var senderaddress = await ConsoleCommand.GetSenderAsync(txInClass.TxHash);
                    var transaction = await ConsoleCommand.GetTransactionAsync(txInClass.TxHash);
                    if (transaction != null)
                    {
                        txdate = GlobalFunctions.UnixTimeStampToDateTime(Convert.ToDouble(transaction.BlockTime));
                    }

                    // Lock in the NFT
                    if (legacyhistory == null && !auctionaddress.LegacyauctionsNfts.Any() &&
                        auctionaddress.State == "waitforlock")
                    {
                        if (utxo.TokensSum == 0 && utxo.LovelaceSummary > 1500000)
                        {
                            var rettx = await SendBackFromLegacyAuctions(db, auctionaddress, txInClass.TxHashId,
                                "First send the NFT to this address", txInClass.Lovelace, senderaddress, txInClass,
                                serverid, mainnet, redis, cancellationToken);
                            await UpdateLegacyHistory(db, legacyhistory, txInClass, senderaddress, auctionaddress.Id,
                                "invalid", rettx, txdate, cancellationToken);
                            continue;
                        }

                        if (utxo.TokensSum > 1)
                        {
                            var rettx = await SendBackFromLegacyAuctions(db, auctionaddress, txInClass.TxHashId,
                                "Just send one NFT", txInClass.Lovelace, senderaddress, txInClass, serverid, mainnet,
                                redis, cancellationToken);
                            await UpdateLegacyHistory(db, legacyhistory, txInClass, senderaddress, auctionaddress.Id,
                                "invalid", rettx, txdate, cancellationToken);
                            continue;
                        }

                        if (utxo.TokensSum == 1 && utxo.LovelaceSummary < 2000000)
                        {
                            var rettx = await SendBackFromLegacyAuctions(db, auctionaddress, txInClass.TxHashId,
                                "Send your NFT and 2 ADA to lock it", txInClass.Lovelace, senderaddress, txInClass,
                                serverid, mainnet, redis, cancellationToken);
                            await UpdateLegacyHistory(db, legacyhistory, txInClass, senderaddress, auctionaddress.Id,
                                "invalid", rettx, txdate, cancellationToken);
                            continue;
                        }

                        string policyid = utxo.TxIn.First().Tokens.First().PolicyId;
                        string tokennamehex = utxo.TxIn.First().Tokens.First().TokennameHex;
                        string tokenname = utxo.TxIn.First().Tokens.First().Tokenname;
                        long tokencount = utxo.TxIn.First().Tokens.First().Quantity;
                        string metadata = "";
                        string ipfs = await BlockfrostFunctions.GetIpfsFromMetadata(policyid, tokennamehex);

                        if (!string.IsNullOrEmpty(ipfs))
                            ipfs = ipfs.Replace(GeneralConfigurationClass.IPFSGateway, "");

                        var royalties = await KoiosFunctions.GetRoyaltiesFromPolicyIdAsync(policyid);
                        if (royalties != null)
                        {
                            auctionaddress.Royaltyaddress = royalties.Address;
                            auctionaddress.Royaltyfeespercent = royalties.Percentage;
                        }

                        await db.LegacyauctionsNfts.AddAsync(new()
                        {
                            Policyid = policyid,
                            Tokennamehex = tokennamehex,
                            Tokencount = tokencount,
                            LegacyauctionId = auctionaddress.Id,
                            Ipfshash = ipfs ?? "",
                            Metadata = metadata
                        }, cancellationToken);
                        auctionaddress.Locknftstxinhashid = utxo.GetFirstTxHash();
                        auctionaddress.State = "active";



                        await db.Legacyauctionshistories.AddAsync(new()
                        {
                            Bidamount = txInClass.Lovelace,
                            Created = DateTime.Now,
                            LegacyauctionId = auctionaddress.Id,
                            Senderaddress = senderaddress,
                            State = "seller",
                            Txhash = txInClass.TxHashId
                        }, cancellationToken);

                        await db.SaveChangesAsync(cancellationToken);

                        continue;
                    }


                    if (auctionaddress.Locknftstxinhashid.Contains(txInClass.TxHash))
                    {
                        await UpdateLegacyHistory(db, legacyhistory, txInClass, senderaddress, auctionaddress.Id,
                            "seller", null, txdate, cancellationToken);
                        continue;
                    }

                    if (txInClass.TxHashId == auctionaddress.Highestbidder)
                        continue;

                    if (txInClass.Lovelace < 1500000)
                        continue;

                    if (string.IsNullOrEmpty(senderaddress))
                        continue;


                    if (auctionaddress.State == "waitforlock")
                    {
                        var rettx = await SendBackFromLegacyAuctions(db, auctionaddress, txInClass.TxHashId,
                            "Auction is not ready", txInClass.Lovelace, senderaddress, txInClass, serverid, mainnet,
                            redis, cancellationToken);
                        await UpdateLegacyHistory(db, legacyhistory, txInClass, senderaddress, auctionaddress.Id,
                            "invalid", rettx, txdate, cancellationToken);
                        continue;
                    }


                    if (txInClass.Lovelace < auctionaddress.Minbet)
                    {

                        var rettx = await SendBackFromLegacyAuctions(db, auctionaddress, txInClass.TxHashId,
                            "Your bid was too low", txInClass.Lovelace, senderaddress, txInClass, serverid, mainnet,
                            redis, cancellationToken);
                        await UpdateLegacyHistory(db, legacyhistory, txInClass, senderaddress, auctionaddress.Id,
                            "outbid", rettx, txdate, cancellationToken);
                        continue;
                    }

                    if (txdate > auctionaddress.Runsuntil)
                    {
                        var rettx = await SendBackFromLegacyAuctions(db, auctionaddress, txInClass.TxHashId,
                            "Auction is already ended", txInClass.Lovelace, senderaddress, txInClass, serverid, mainnet,
                            redis, cancellationToken);
                        await UpdateLegacyHistory(db, legacyhistory, txInClass, senderaddress, auctionaddress.Id,
                            "expired", rettx, txdate, cancellationToken);
                        continue;
                    }

                    if (auctionaddress.State == "finished")
                    {
                        var rettx = await SendBackFromLegacyAuctions(db, auctionaddress, txInClass.TxHashId,
                            "Auction is already ended", txInClass.Lovelace, senderaddress, txInClass, serverid, mainnet,
                            redis, cancellationToken);
                        await UpdateLegacyHistory(db, legacyhistory, txInClass, senderaddress, auctionaddress.Id,
                            "expired", rettx, txdate, cancellationToken);
                        continue;
                    }

                    if (txInClass.Tokens != null && txInClass.Tokens.Any())
                    {
                        var rettx = await SendBackFromLegacyAuctions(db, auctionaddress, txInClass.TxHashId,
                            "Only send ADA - no Tokens for this auction", txInClass.Lovelace, senderaddress, txInClass,
                            serverid, mainnet, redis, cancellationToken);
                        await UpdateLegacyHistory(db, legacyhistory, txInClass, senderaddress, auctionaddress.Id,
                            "invalid", rettx, txdate, cancellationToken);
                        continue;
                    }


                    if (txInClass.Lovelace <= auctionaddress.Actualbet)
                    {
                        var rettx = await SendBackFromLegacyAuctions(db, auctionaddress, txInClass.TxHashId,
                            "You have been outbid. Your bid was too low.", txInClass.Lovelace, senderaddress, txInClass,
                            serverid, mainnet, redis, cancellationToken);
                        await UpdateLegacyHistory(db, legacyhistory, txInClass, senderaddress, auctionaddress.Id,
                            "outbid", rettx, txdate, cancellationToken);

                        continue;
                    }

                    auctionaddress.Actualbet = txInClass.Lovelace;
                    auctionaddress.Highestbidder = txInClass.TxHashId;
                    await db.SaveChangesAsync(cancellationToken);
                    await UpdateLegacyHistory(db, legacyhistory, txInClass, senderaddress, auctionaddress.Id, "buyer",
                        null, txdate, cancellationToken);
                }

                if (auctionaddress.Runsuntil < DateTime.Now && auctionaddress.State == "active")
                {
                 

                    if (string.IsNullOrEmpty(auctionaddress.Highestbidder))
                    {
                        var selleraddress = await ConsoleCommand.GetSenderAsync(auctionaddress.Locknftstxinhashid);
                        var newutxo= utxo.TxIn.FirstOrDefault(x => x.TxHashId == auctionaddress.Locknftstxinhashid);

                        await SendBackFromLegacyAuctions(db, auctionaddress, auctionaddress.Locknftstxinhashid,
                            "Sorry, but your NFT was not sold.", 0, selleraddress, newutxo, serverid, mainnet, redis,
                            cancellationToken);
                    }
                    else
                    {
                        var selleraddress = await ConsoleCommand.GetSenderAsync(auctionaddress.Locknftstxinhashid);
                        var newtxinseller =
                            utxo.TxIn.FirstOrDefault(x => x.TxHashId == auctionaddress.Locknftstxinhashid);

                        var buyeraddress = await ConsoleCommand.GetSenderAsync(auctionaddress.Highestbidder);

                        await FinishLegacyAuctionSuccessfully(db,redis, auctionaddress,
                            "Congratulations. Your NFT was sold.", selleraddress, utxo, mainnet);

                        await SendBackFromLegacyAuctions(db, auctionaddress, auctionaddress.Locknftstxinhashid,
                            "Congratulations. You won the auction.", 0, buyeraddress, newtxinseller, serverid, mainnet,
                            redis, cancellationToken);
                    }
                    auctionaddress.State = "finished";
                    await db.SaveChangesAsync(cancellationToken);
                }

              
            }




            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }


        private async Task<string> SendBackFromLegacyAuctions(EasynftprojectsContext db, Legacyauction auctionaddress,
            string txHashId, string sendbackmessage, long lovelace, string senderaddress,
            TxInClass txin, int serverid, bool mainnet, IConnectionMultiplexer redis,
            CancellationToken cancellationToken)
        {
            // First save to txinhashes - so we prevent it from double processing
            try
            {
                await db.Database.ExecuteSqlRawAsync($"delete from projectaddressestxhashes where txhash='{txHashId}'", cancellationToken: cancellationToken);
                await db.Projectaddressestxhashes.AddAsync(new()
                {
                    Address = auctionaddress.Address,
                    Created = DateTime.Now,
                    Lovelace = lovelace,
                    Txhash = txHashId,
                    Tokens = ""
                }, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                await StaticBackgroundServerClass.EventLogException(db, 37, e, serverid);
            }

            BuildTransactionClass buildtransaction = new();

            var s = CardanoSharpFunctions.SendAllAdaAndTokens(db, redis, auctionaddress.Address, auctionaddress.Skey,auctionaddress.Vkey,
                auctionaddress.Salt + GeneralConfigurationClass.Masterpassword, senderaddress,
                mainnet, ref buildtransaction, txHashId, 1, 0, sendbackmessage);
            /*
            auctionaddress.Log ??= "";
            auctionaddress.Log += buildtransaction.LogFile;
            await db.SaveChangesAsync(cancellationToken);
              */
            if (s == "OK")
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"SendBackFromLegacyAuctions successful {auctionaddress.Address} {senderaddress} {txHashId}", "",
                    serverid);
                return buildtransaction.TxHash;
            }
            else
                await StaticBackgroundServerClass.LogAsync(db,
                    $"SendBackFromLegacyAuctions failed {auctionaddress.Address} {senderaddress} {txHashId}", s,
                    serverid);

            return null;
        }



        private async Task UpdateLegacyHistory(EasynftprojectsContext db, Legacyauctionshistory legacyhistory,
            TxInClass txInClass, string senderaddress, int legacyid, string state, string returntxhash, DateTime txdate,
            CancellationToken cancellationToken)
        {
            if (legacyhistory == null)
            {
                Legacyauctionshistory lah = new()
                {
                    Bidamount = txInClass.Lovelace,
                    Created = txdate,
                    LegacyauctionId = legacyid,
                    Senderaddress = senderaddress,
                    State = state,
                    Txhash = txInClass.TxHashId,
                    Returntxhash = returntxhash
                };
                await db.Legacyauctionshistories.AddAsync(lah, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                var lah = await (from a in db.Legacyauctionshistories
                    where a.Id == legacyhistory.Id
                    select a).FirstOrDefaultAsync(cancellationToken);
                if (lah != null)
                {
                    lah.State = state;
                    lah.Returntxhash = returntxhash;
                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }

        private async Task FinishLegacyAuctionSuccessfully(EasynftprojectsContext db, IConnectionMultiplexer redis, Legacyauction auctionaddress,
            string sendbackmessage, string selleraddress, TxInAddressesClass utxo, bool mainnet)
        {
            BuildTransactionClass bt = new();
            List<TxOutClass> txouts = new();
            // Marketplace
            if (auctionaddress.Nftproject != null && auctionaddress.Nftproject.Smartcontractssettings != null)
            {
                txouts.Add(new()
                {
                    ReceiverAddress = auctionaddress.Nftproject.Smartcontractssettings.Address,
                    Amount = Math.Max(1000000, Convert.ToInt64(auctionaddress.Actualbet / 100 *
                                                                 auctionaddress.Nftproject.Smartcontractssettings
                                                                     .Percentage))
                });
            }

            // Royalites
            if (!string.IsNullOrEmpty(auctionaddress.Royaltyaddress) && auctionaddress.Royaltyfeespercent != null)
            {
                txouts.Add(new()
                {
                    ReceiverAddress = auctionaddress.Royaltyaddress,
                    Amount = Math.Max(1000000, Convert.ToInt64(auctionaddress.Actualbet / 100 *
                                                                 auctionaddress.Royaltyfeespercent))
                });
            }


            var ok = ConsoleCommand.FinishLegacyAuction(db,redis,auctionaddress, txouts, selleraddress,
                utxo.TxIn.First(x => x.TxHashId == auctionaddress.Highestbidder), sendbackmessage, mainnet, ref bt);

            if (auctionaddress.Log == null)
                auctionaddress.Log = "";
            auctionaddress.Log += bt.LogFile;
            await db.SaveChangesAsync();

            if (ok == "OK")
            {
                // TODO: Save TX to Transactions
            }
            else
            {


            }
        }
    }
}
