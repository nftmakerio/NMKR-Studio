using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Blockfrost;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Model;
using NMKR.Shared.NotificationClasses;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckTransactionConfirmations : IBackgroundServices
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
                where a.Confirmed == false && a.Checkforconfirmdate == null  && a.Created > DateTime.Now.AddHours(-12) && a.Coin==nameof(Coin.ADA)
                orderby a.Created 
                select a).ToListAsync(cancellationToken: cancellationToken);


            foreach (var transaction in transactions)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Check Transaction {transaction.Transactionid} - {transaction.Created}", "", serverid);

                var txinfobf = await ConsoleCommand.GetTransactionAsync(transaction.Transactionid);


                if (txinfobf != null)
                {
                    DateTime currentTime = DateTime.UtcNow;
                    long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
                    var txdate = txinfobf.BlockTime == null
                        ? unixTime
                        : txinfobf.BlockTime;
                    transaction.Confirmed = true;
                    transaction.Checkforconfirmdate = DateTime.Now;
                    transaction.Transactionblockchaintime = txdate;
                    try
                    {
                        await db.SaveChangesAsync(cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await GlobalFunctions.LogExceptionAsync(db, $"Exception CheckTransactionConfirmation",
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


                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Transaction {transaction.Transactionid} confirmed - send to Rabbit MQ",
                        transaction.Transactionid, serverid);


                    // Send the TXID to RabbitMQ 
                    await bus.Publish(
                        new RmqTransactionClass
                        {
                            TransactionId = transaction.Id, ProjectId = transaction.NftprojectId,
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
                else
                {
                    if (transaction.Created > DateTime.Now.AddMinutes(-2))
                        continue;
                    if (transaction.Created < DateTime.Now.AddMinutes(-60))
                        continue;

                    // Try to resubmit
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"CheckTransactionConfirmation - Transaction {transaction.Transactionid} not found on Blockfrost - resubmit "+transaction.Senderaddress,
                        transaction.Transactionid, serverid);
                    if (string.IsNullOrEmpty(transaction.Cbor) || transaction.Stopresubmitting) continue;
                   
                    string signedTxStr = ConsoleCommand.GetCbor(transaction.Cbor);
                    if (string.IsNullOrEmpty(signedTxStr))
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            "CheckTransactionConfirmation - CBOR is not valid " + transaction.Senderaddress,
                            transaction.Cbor, serverid);
                        transaction.Stopresubmitting = true;
                        await db.SaveChangesAsync(cancellationToken: cancellationToken);
                        continue;
                    }
                    await StaticBackgroundServerClass.LogAsync(db,
                        "CheckTransactionConfirmation - Submit again " + transaction.Senderaddress,
                        transaction.Cbor + Environment.NewLine);

                    var ok1 = await BlockfrostFunctions.SubmitTransactionAsync(
                        Convert.FromHexString(signedTxStr));
                    if (ok1.Success) continue;


                    await StaticBackgroundServerClass.LogAsync(db,
                        "CheckTransactionConfirmation - Submit via Blockfrost failed - Try again with Koios " + transaction.Senderaddress,
                        transaction.Cbor, serverid);

                    var ok2 = await KoiosFunctions.SubmitTransactionAsync(Convert.FromHexString(signedTxStr));
                    if (!ok2.Success)
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            "CheckTransactionConfirmation - Submit via Koios failed "+ transaction.Senderaddress,
                            transaction.Cbor + Environment.NewLine + ok2.ErrorMessage);
                    }

                    transaction.Stopresubmitting = true;
                    await db.SaveChangesAsync(cancellationToken: cancellationToken);
                }
            }


            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }
    }
}
