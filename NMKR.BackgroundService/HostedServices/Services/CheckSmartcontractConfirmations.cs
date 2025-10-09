using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using NMKR.Shared.NotificationClasses;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckSmartcontractConfirmations : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checktransactionconfirmations;
            if (server.Checktransactionconfirmations == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            var transactions = await (from a in db.PreparedpaymenttransactionsSmartcontractsjsons
                    .Include(a=>a.Preparedpaymenttransactions).AsSplitQuery()
                                      where a.Confirmed == false && a.Signedandsubmitted==true && !string.IsNullOrEmpty(a.Txid)  && a.Created > DateTime.Now.AddHours(-4)
                                      select a).Take(500).ToListAsync(cancellationToken: cancellationToken);


            foreach (var transaction in transactions)
            {
                await StaticBackgroundServerClass.LogAsync(db, $"Check Smartcontract Transaction {transaction.Preparedpaymenttransactions.Transactionuid}", "", serverid);
                var txinfo = await ConsoleCommand.GetTransactionAsync(transaction.Txid);

                if (txinfo != null)
                {
                    var txdate = DateTime.Now;

                        txdate = GlobalFunctions.UnixTimeStampToDateTime(Convert.ToDouble(txinfo.BlockTime));

                    transaction.Confirmed = true;
                    transaction.Checkforconfirmdate = txdate;
                    try
                    {
                        await db.SaveChangesAsync(cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await StaticBackgroundServerClass.LogAsync(db, $"Exception", ex.Message, serverid);
                    }

                    await StaticBackgroundServerClass.LogAsync(db, $"Smartcontract Transaction {transaction.Preparedpaymenttransactions.Transactionuid} confirmed", transaction.Preparedpaymenttransactions.Transactionuid, serverid);


                    // Send the TXID to RabbitMQ 
                    await bus.Publish(new RmqSmartcontractTransactionClass { PreparedJsonId = transaction.Id, PreparedTransactionId = transaction.Preparedpaymenttransactions.Id, EventType = NotificationEventTypes.transactionconfirmed }, cancellationToken);

                }
            }



            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }
    }
}
