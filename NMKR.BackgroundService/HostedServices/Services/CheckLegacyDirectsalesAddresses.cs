using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Cardano_Sharp;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckLegacyDirectsalesAddresses : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checklegacydirectsales;
            if (server.Checklegacydirectsales == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            var directsalesaddresses = await (from a in db.Legacydirectsales
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Smartcontractssettings)
                    .AsSplitQuery()
                where a.State == "active" || a.State == "finished"
                select a).ToListAsync(cancellationToken: cancellationToken);

            foreach (var address in directsalesaddresses)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Check Legacy Directsale Address {address.Address}", "", serverid);

                var utxo = await ConsoleCommand.GetNewUtxoAsync(address.Address);
                if (utxo == null)
                    continue;
                if (utxo.TxIn == null || !utxo.TxIn.Any())
                    continue;

                foreach (var txInClass in utxo.TxIn)
                {
                 //   var txdate = DateTime.Now;
                    var senderaddress = await ConsoleCommand.GetSenderAsync(txInClass.TxHash);
                    var transaction = await ConsoleCommand.GetTransactionAsync(txInClass.TxHash);
                    if (transaction != null)
                    {
                     //   txdate = transaction.First().TxTimestamp;
                    }

                    if (txInClass.TxHash == address.Locknftstxinhashid)
                    {
                        continue;
                    }

                    if (txInClass.TxHashId == address.Buyer)
                        continue;

                    if (txInClass.Lovelace < 1500000)
                        continue;

                    if (string.IsNullOrEmpty(senderaddress))
                        continue;

                    if (txInClass.Lovelace != address.Price + address.Lockamount)
                    {
                        await SendBackFromLegacyDirectsale(db, address, txInClass.TxHashId,
                            $"The amount of ADA is not correct. You have to send {(address.Price + address.Lockamount)} lovelace",
                            txInClass.Lovelace, senderaddress, serverid, mainnet, redis, cancellationToken);
                        continue;
                    }

                    if (address.State == "finished")
                    {
                        await SendBackFromLegacyDirectsale(db, address, txInClass.TxHashId,
                            "Sale is already ended", txInClass.Lovelace, senderaddress, serverid, mainnet,
                            redis, cancellationToken);
                        continue;
                    }

                    if (txInClass.Tokens != null && txInClass.Tokens.Any())
                    {
                        await SendBackFromLegacyDirectsale(db, address, txInClass.TxHashId,
                            "Only send ADA - no Tokens for this auction", txInClass.Lovelace, senderaddress,
                            serverid, mainnet, redis,
                            cancellationToken);
                        continue;
                    }


                    address.Price = txInClass.Lovelace;
                    address.Buyer = txInClass.TxHashId;
                    address.State = "finished";
                    address.Solddate = DateTime.Now;
                    await db.SaveChangesAsync(cancellationToken);

                    var selleraddress = await ConsoleCommand.GetSenderAsync(address.Locknftstxinhashid);

                    var buyeraddress = await ConsoleCommand.GetSenderAsync(address.Buyer);

                    FinishLegacyDirectsaleSuccessfully(db,redis, address,
                        "Congratulations. Your NFT was sold.", selleraddress, utxo, mainnet);

                    await SendBackFromLegacyDirectsale(db, address, address.Locknftstxinhashid,
                        "Congratulations. You got the NFT.", 0, buyeraddress, serverid, mainnet, redis,
                        cancellationToken);

                }


                if (address.Solddate != null && address.Solddate < DateTime.Now.AddDays(-2) &&
                    address.State == "finished")
                {
                    address.State = "ended";
                    await db.SaveChangesAsync(cancellationToken);
                }
            }




            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private async Task<string> SendBackFromLegacyDirectsale(EasynftprojectsContext db,
            Legacydirectsale directsaleaddress, string txHashId, string sendbackmessage, long lovelace,
            string senderaddress, int serverid, bool mainnet,
            IConnectionMultiplexer redis, CancellationToken cancellationToken)
        {
            // First save to txinhashes - so we prevent it from double processing
            try
            {
                await db.Database.ExecuteSqlRawAsync($"delete from projectaddressestxhashes where txhash='{txHashId}'", cancellationToken: cancellationToken);
                await db.Projectaddressestxhashes.AddAsync(new()
                {
                    Address = directsaleaddress.Address,
                    Created = DateTime.Now,
                    Lovelace = lovelace,
                    Txhash = txHashId,
                    Tokens = ""
                }, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                await StaticBackgroundServerClass.EventLogException(db, 512, e, serverid);
            }

            BuildTransactionClass buildtransaction = new();

            var s = CardanoSharpFunctions.SendAllAdaAndTokens(db,redis, directsaleaddress.Address, directsaleaddress.Skey,directsaleaddress.Vkey,
                directsaleaddress.Salt + GeneralConfigurationClass.Masterpassword, senderaddress,
                mainnet, ref buildtransaction, txHashId, 1, 0, sendbackmessage);
            if (s == "OK")
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"SendBackFromLegacyAuctions successful {directsaleaddress.Address} {senderaddress} {txHashId}", "",
                    serverid);
                return buildtransaction.TxHash;
            }
            else
                await StaticBackgroundServerClass.LogAsync(db,
                    $"SendBackFromLegacyAuctions failed {directsaleaddress.Address} {senderaddress} {txHashId}", s,
                    serverid);

            return null;
        }



        private void FinishLegacyDirectsaleSuccessfully(EasynftprojectsContext db, IConnectionMultiplexer redis,
            Legacydirectsale direktsaleaddress, string sendbackmessage, string selleraddress, TxInAddressesClass utxo,
            bool mainnet)
        {
            BuildTransactionClass bt = new();
            List<TxOutClass> txouts = new();
            // Marketplace
            txouts.Add(new()
            {
                ReceiverAddress = direktsaleaddress.Nftproject.Smartcontractssettings.Address,
                Amount = Math.Max(1000000, Convert.ToInt64(direktsaleaddress.Price / 100 *
                                                             direktsaleaddress.Nftproject.Smartcontractssettings
                                                                 .Percentage))
            });

            // Royalites
            if (!string.IsNullOrEmpty(direktsaleaddress.Royaltyaddress) && direktsaleaddress.Royaltyfeespercent != null)
            {
                txouts.Add(new()
                {
                    ReceiverAddress = direktsaleaddress.Royaltyaddress,
                    Amount = Math.Max(1000000, Convert.ToInt64(direktsaleaddress.Price / 100 *
                                                                 direktsaleaddress.Royaltyfeespercent))
                });
            }


            var ok = ConsoleCommand.FinishLegacyDirectsale(db,redis,direktsaleaddress, txouts, selleraddress,
                utxo.TxIn.First(x => x.TxHashId == direktsaleaddress.Buyer), sendbackmessage, mainnet, ref bt);
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
