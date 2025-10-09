using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Cardano_Sharp;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckBurningEndpointsCardano : IBackgroundServices
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
                where a.Validuntil > DateTime.Now && a.State == "active" && a.Blockchain==Blockchain.Cardano.ToString()
                select a).ToListAsync(cancellationToken: cancellationToken);

            foreach (var adr in addresses)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await StaticBackgroundServerClass.LogAsync(db,
                    $"Check Burning Address: {adr.Address} - Project: {adr.Nftproject.Id} {adr.Nftproject.Projectname}",
                    "", serverid);

                BuildTransactionClass buildTransaction = new();

                var utx1 = await ConsoleCommand.GetNewUtxoAsync(adr.Address);

                if (utx1.LovelaceSummary > 0)
                {
                    long adaamount = utx1.LovelaceSummary;
                    adr.Lovelace = adaamount;
                    await db.SaveChangesAsync(cancellationToken);
                }

                if (adr.Lovelace == 0)
                    continue;



                foreach (var txInClass in utx1.TxIn)
                {

                    var projectaddresshash = await (from a in db.Projectaddressestxhashes
                        where a.Txhash == txInClass.TxHashId
                        select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (projectaddresshash != null)
                        continue;


                    await db.Projectaddressestxhashes.AddAsync(new()
                    {
                        Address = adr.Address, Created = DateTime.Now, Lovelace = txInClass.Lovelace,
                        Txhash = txInClass.TxHashId
                    }, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);
                 

                    if (txInClass.Tokens!=null && txInClass.Tokens.Any())
                    {
                        // Burn Token
                        CardanoTransactionClass ctc;
                        var ok = ConsoleCommand.BurnTokens(db,redis, adr.Address, adr.Nftproject.Settings.Mintingaddress,
                            adr.Nftproject.Policyskey, adr.Nftproject.Password, adr.Nftproject.Policyscript,
                            adr.Privateskey, adr.Salt, mainnet, txInClass, out ctc, true, 0);

                        if (ok == "OK")
                        {
                            await ResetTheNft(db,redis, cancellationToken, serverid, txInClass, adr, ctc);
                            await ShowNmkrStudioNotification(db, cancellationToken, adr);
                        }
                        else
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Error while burning NFT - {adr.NftprojectId}",
                                ok+Environment.NewLine+ctc.buildtransaction.LogFile, serverid);
                        }
                        await db.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Burn was not possible - {adr.NftprojectId}",
                            JsonConvert.SerializeObject(txInClass) + Environment.NewLine+Environment.NewLine+JsonConvert.SerializeObject(utx1), serverid);
                        if (txInClass.Lovelace <= 1300000) continue;
                        var sender = await ConsoleCommand.GetSenderAsync(txInClass.TxHash);
                        CardanoSharpFunctions.SendAllAdaAndTokens(db, redis, utx1.Address, adr.Privateskey,
                            adr.Privatevkey, adr.Salt + GeneralConfigurationClass.Masterpassword, sender, mainnet,
                            ref buildTransaction, txInClass.TxHashId, 0,
                            0, "Burn not possible", utx1);
                    }
                }

            }
        }

        private static async Task ShowNmkrStudioNotification(EasynftprojectsContext db, CancellationToken cancellationToken,
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

        private static async Task ResetTheNft(EasynftprojectsContext db, IConnectionMultiplexer redis, CancellationToken cancellationToken, int serverid,
            TxInClass txInClass, Burnigendpoint adr, CardanoTransactionClass ctc)
        {
            if (txInClass.Tokens != null && txInClass.Tokens.Any())
            {
                foreach (var btc in txInClass.Tokens)
                {
                    var assetid = GlobalFunctions.GetAssetId(btc.PolicyId, btc.Tokenname, "");
                    var nft = await (from a in db.Nfts
                            .Include(a => a.Nftproject)
                            .AsSplitQuery()
                        where a.NftprojectId == adr.NftprojectId &&
                              (a.Assetid == assetid || a.Name == btc.Tokenname) &&
                              a.State != "deleted"
                        select a).FirstOrDefaultAsync(cancellationToken: cancellationToken);
                    if (nft == null)
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"NFT was burned, but not found - {assetid} - {btc.Tokenname} - {adr.NftprojectId}",
                            "", serverid);
                        continue;
                    }

                    nft.Checkpolicyid = true;
                    nft.Markedaserror = DateTime.Now;
                    if (nft is not {Isroyaltytoken: false}) continue;
                    if (adr.Nftproject.Maxsupply == 1)
                    {
                        if (!adr.Fixnfts)
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Set NFT {nft.Id} - {btc.Tokenname} - {assetid} to burned ", "",
                                serverid);
                            nft.State = "burned";
                            nft.Reservedcount = 0;
                            nft.Soldcount = 0;
                            nft.Errorcount = 0;
                            nft.Burncount = 1;
                            nft.Minted = false;
                        }
                    }
                    else
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Burn{btc.Quantity} Tokens of NFT {nft.Id} - {btc.Tokenname} - {assetid}",
                            "", serverid);
                        nft.Soldcount = nft.Soldcount - btc.Quantity;
                        if (nft.State == "sold")
                            nft.State = "free";
                        if (nft.Soldcount < 0)
                            nft.Soldcount = 0;
                    }

                    await db.SaveChangesAsync(cancellationToken);

                    // Save the burning tx to the database
                    ctc.MintingFees = ctc.buildtransaction.BuyerTxOut.Amount;
                    ctc.buildtransaction.MintingcostsTxOut = new()
                    {
                        Amount = ctc.buildtransaction.BuyerTxOut.Amount -
                                   ctc.buildtransaction.Fees,
                        ReceiverAddress = adr.Nftproject.Settings.Mintingaddress
                    };
                    ctc.buildtransaction.BuyerTxOut.Amount = 0;

                    if (ctc.buildtransaction.MintingcostsTxOut?.Amount > 0)
                    {
                        await StaticBackgroundServerClass.SaveTransactionToDatabase(db, redis,
                            ctc.buildtransaction,
                            adr.Nftproject.CustomerId, null,
                            adr.NftprojectId, nameof(TransactionTypes.burning), null, serverid, Coin.ADA);
                    }
                }
            }
        }
    }
}

