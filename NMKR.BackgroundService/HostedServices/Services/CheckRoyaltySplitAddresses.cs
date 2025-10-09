using System;
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
    public class CheckRoyaltySplitAddresses : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkroyaltysplitaddresses;
            if (server.Checkroyaltysplitaddresses == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            var splitaddresses = await (from a in db.Splitroyaltyaddresses
                    .Include(a => a.Splitroyaltyaddressessplits)
                    .AsSplitQuery()
                    .Include(a => a.Customer)
                    .ThenInclude(a => a.Defaultsettings)
                    .AsSplitQuery()
                where a.State == "active" && (a.Lastcheck == null || a.Lastcheck < DateTime.Now.AddMinutes(-5))
                select a).ToListAsync(cancellationToken);

            var adahandles = await (from a in db.Adahandles
                select a).AsNoTracking().ToListAsync(cancellationToken: cancellationToken);

            foreach (var address in splitaddresses)
            {
                address.Lastcheck = DateTime.Now;
                await db.SaveChangesAsync(cancellationToken);


                var utxo = await ConsoleCommand.GetNewUtxoAsync(address.Address);
                utxo = ConsoleCommand.RemoveAdaHandles(utxo, adahandles);
                if (utxo.LovelaceSummary <= address.Minthreshold*1000000)
                    continue;

                // Create Transaction

                // Check for Adahandles
                var address1 = CheckRoyaltySplitAddressForAdahandes(db, address, mainnet);

                BuildTransactionClass bt = new();
             //   string ok = ConsoleCommand.SendAllAdaRoyalitySplit(db, redis, utxo, address1, mainnet,40, out var txouts, ref bt);
                string ok = CardanoSharpFunctions.SendAllAdaRoyalitySplit(db, redis, utxo, address1, mainnet, 40, out var txouts, ref bt);
                // Save Transaction
                if (ok == "OK")
                {
                    // Save to transactions (for invoices)
                    var transaction = await StaticBackgroundServerClass.SaveTransactionToDatabase(db, redis,bt,
                        address1.CustomerId,
                        null,
                        TransactionTypes.royaltsplit,
                        null,
                        null, serverid, Coin.ADA);

                    // Save to own tx database for statistics
                    Splitroyaltyaddressestransaction transactionx = new()
                    {
                        Created = DateTime.Now,
                        Fee = bt.Fees,
                        Amount = utxo.LovelaceSummary,
                        Costs = address1.Customer.Defaultsettings.Mintingcosts,
                        Costsaddress = address1.Customer.Defaultsettings.Mintingaddress,
                        Changeaddress = address1.Splitroyaltyaddressessplits.First(x => x.IsMainReceiver == true)
                            .Address,
                        Txid = bt.TxHash,
                        SplitroyaltyaddressesId = address1.Id
                    };
                    await db.Splitroyaltyaddressestransactions.AddAsync(transactionx, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);
                    foreach (var splitroyaltyaddressessplit in txouts)
                    {
                        await db.Splitroyaltyaddressestransactionssplits.AddAsync(
                            new()
                            {
                                SplitroyaltyaddressestransactionsId = transactionx.Id,
                                Amount = splitroyaltyaddressessplit.Amount,
                                Splitaddress = splitroyaltyaddressessplit.ReceiverAddress,
                                Percentage = splitroyaltyaddressessplit.Percentage,
                            }, cancellationToken);
                        await db.SaveChangesAsync(cancellationToken);
                    }

                }
            }


            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private Splitroyaltyaddress CheckRoyaltySplitAddressForAdahandes(EasynftprojectsContext db,
            Splitroyaltyaddress address, bool mainnet)
        {
            foreach (var split in address.Splitroyaltyaddressessplits)
            {
                ConsoleCommand.CheckIfAddressIsValid(db, split.Address, mainnet, out string outaddress, out Blockchain blockchain, true);
                split.Address = outaddress;
            }

            return address;
        }

    }
}
