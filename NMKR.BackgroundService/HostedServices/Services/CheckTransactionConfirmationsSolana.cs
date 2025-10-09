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
    public class CheckTransactionConfirmationsSolana : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checktransactionconfirmations;
            if (server.Checktransactionconfirmations == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            var transactions = await (from a in db.Transactions
                    .Include(a=>a.TransactionNfts).AsSplitQuery()
                                      where a.Confirmed == false && a.Checkforconfirmdate == null && a.Created > DateTime.Now.AddHours(-12) && a.Coin == nameof(Coin.SOL)
                                      orderby a.Created
                                      select a).ToListAsync(cancellationToken: cancellationToken);


            foreach (var transaction in transactions)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Check Solana Transaction {transaction.Transactionid} - {transaction.Created}", "", serverid);


                var solanatx = await SolanaFunctions.GetTransactionAsync(transaction.Transactionid);

                if (solanatx != null)
                {
                    DateTime currentTime = DateTime.UtcNow;
                    long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
                    var txdate = solanatx.BlockTime ?? unixTime;
                    transaction.Confirmed = true;
                    transaction.Checkforconfirmdate = DateTime.Now;
                    transaction.Transactionblockchaintime = txdate;
                    try
                    {
                        await db.SaveChangesAsync(cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await GlobalFunctions.LogExceptionAsync(db, $"Exception CheckTransactionConfirmationSolana",
                            ex.Message, serverid);
                        GlobalFunctions.ResetContextState(db);
                    }

                    if (transaction.PreparedpaymenttransactionId != null)
                    {
                        var preparedpaymenttransaction = await (from a in db.Preparedpaymenttransactions
                                                                where a.Id == transaction.PreparedpaymenttransactionId
                                                                select a).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                        if (preparedpaymenttransaction != null)
                        {
                            preparedpaymenttransaction.Smartcontractstate = "confirmed";
                            await db.SaveChangesAsync(cancellationToken: cancellationToken);
                        }
                    }


                    await CheckTransactionNftsAsync(db, transaction, serverid, cancellationToken);


                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Transaction {transaction.Transactionid} confirmed - send to Rabbit MQ",
                        transaction.Transactionid, serverid);


                    // Send the TXID to RabbitMQ 
                    await bus.Publish(
                        new RmqTransactionClass
                        {
                            TransactionId = transaction.Id,
                            ProjectId = transaction.NftprojectId,
                            EventType = NotificationEventTypes.transactionconfirmed
                        }, cancellationToken);



                    if (transaction.Transactiontype == nameof(TransactionTypes.mintfromnftmakeraddress) ||
                        transaction.Transactiontype == nameof(TransactionTypes.mintfromcustomeraddress))
                    {
                        var mintandsend = await (from a in db.Mintandsends
                                                 where a.Transactionid == transaction.Transactionid && a.Confirmed == false
                                                 select a).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                        if (mintandsend != null)
                        {
                            mintandsend.Confirmed = true;
                            await db.SaveChangesAsync(cancellationToken: cancellationToken);
                        }
                    }

                }
            }


            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private async Task CheckTransactionNftsAsync(EasynftprojectsContext db, Transaction transaction, int serverid,
            CancellationToken cancellationToken)
        {
            foreach (var nft in transaction.TransactionNfts)
            {
                if (string.IsNullOrEmpty(nft.Txhash))
                    continue;

                var solanatx = await SolanaFunctions.GetTransactionAsync(nft.Txhash);
                if (solanatx != null)
                {
                    DateTime currentTime = DateTime.UtcNow;
                    long unixTime = ((DateTimeOffset) currentTime).ToUnixTimeSeconds();
                    var txdate = solanatx.BlockTime ?? unixTime;
                    nft.Confirmed = true;
                    nft.Checkforconfirmdate = DateTime.Now;
                    nft.Transactionblockchaintime = txdate;
                    try
                    {
                        await db.SaveChangesAsync(cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await GlobalFunctions.LogExceptionAsync(db, $"Exception 2 CheckTransactionConfirmationSolana",
                            ex.Message, serverid);
                        GlobalFunctions.ResetContextState(db);
                    }
                }
            }
        }
    }
}
