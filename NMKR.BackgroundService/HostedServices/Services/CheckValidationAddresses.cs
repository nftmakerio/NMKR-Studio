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
    public class CheckValidationAddresses : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkvalidationaddresses;
            if (server.Checkvalidationaddresses == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            var addresses = await (from a in db.Validationaddresses
                    .Include(a => a.Validationamounts)
                    .AsSplitQuery()
                where a.State == "active"
                select a).ToListAsync(cancellationToken: cancellationToken);

            foreach (var validationaddress in addresses)
            {
                var utxo = await ConsoleCommand.GetNewUtxoAsync(validationaddress.Address);
                if (utxo == null || utxo.TxIn == null)
                    continue;

                foreach (var txin in utxo.TxIn)
                {
                    var ll = txin.Lovelace;

                    var senderaddress = await ConsoleCommand.GetSenderAsync(txin.TxHash);


                    var valamount = validationaddress.Validationamounts.FirstOrDefault(x => x.Lovelace == ll);
                    if (valamount != null)
                    {
                        valamount.State = "validated";
                        valamount.Senderaddress = senderaddress;
                        await db.SaveChangesAsync(cancellationToken);
                    }

                    if (ll >= 1300000)
                        await SendBackValidationAmount(db, validationaddress, senderaddress, txin.TxHashId, serverid,
                            mainnet, redis);

                }
            }



            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private async Task SendBackValidationAmount(EasynftprojectsContext db, Validationaddress validationaddress,
            string senderaddress, string txHash, int serverid, bool mainnet, IConnectionMultiplexer redis)
        {
            BuildTransactionClass buildtransaction = new();
            string s = SendAllAdaAndTokensFromValidationAddresses(db, redis, validationaddress,
                senderaddress, mainnet, ref buildtransaction, txHash, 0);
            if (s == "OK")
                await StaticBackgroundServerClass.LogAsync(db,
                    $"SendBackValidationAmount successful {validationaddress} {senderaddress}", "", serverid);
            else
                await StaticBackgroundServerClass.LogAsync(db,
                    $"SendBackFromProjectAddress failed {validationaddress} {senderaddress}", s, serverid);

        }
        public static string SendAllAdaAndTokensFromValidationAddresses(EasynftprojectsContext db, IConnectionMultiplexer redis,
            Validationaddress validationaddress, string senderaddress, bool mainnet,
            ref BuildTransactionClass buildtransaction, string txHash, int maxtx)
        {
            return CardanoSharpFunctions.SendAllAdaAndTokens(db, redis, validationaddress.Address, validationaddress.Privateskey, validationaddress.Privatevkey,
                validationaddress.Password + GeneralConfigurationClass.Masterpassword, senderaddress,
                mainnet, ref buildtransaction, txHash, maxtx);
        }

    }
}
