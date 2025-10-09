using System;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class ExecuteDatabaseCommands : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter,Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.executedatabasecommands;
            if (server.Executedatabasecommands == false)
                return;


            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);

            DateTime start = DateTime.UtcNow;
                try
                {
                    await db.Database.ExecuteSqlRawAsync("delete from loggedinhashes where validuntil < NOW()", cancellationToken);

                  /* we will not reuse old addresses again
                await db.Database.ExecuteSqlRawAsync(
                        "update nftaddresses USE INDEX (lastcheckforutxo) set state='free', price=0, nftproject_id=null, preparedpaymenttransactions_id=null, lastcheckforutxo=null, created=NOW(), reservationtoken=null, rejectreason=null, rejectparameter=null where (state='expired' ) and lovelace=0 and utxo=0 and created < DATE_SUB(NOW(), INTERVAL 4 DAY) and txid is null and senderaddress is null and salt is not null and errormessage is null and submissionresult is null and lastcheckforutxo > DATE_SUB(NOW(),INTERVAL 1 DAY)", cancellationToken);
                  */

                    await db.Database.ExecuteSqlRawAsync(
                        "update burnigendpoints set state='notactive' where validuntil < NOW()", cancellationToken);
                    await db.Database.ExecuteSqlRawAsync(
                        "delete from blockedipaddresses where blockeduntil < NOW()", cancellationToken);
                    await db.Database.ExecuteSqlRawAsync(
                        "delete from projectaddressestxhashes where created < DATE_SUB(NOW(),INTERVAL 1 DAY)", cancellationToken);
                    await db.Database.ExecuteSqlRawAsync(
                        "update validationamounts set state='expired' where validuntil < NOW() and state='notvalidated'", cancellationToken);
                    await db.Database.ExecuteSqlRawAsync(
                        "update nftprojects set state='finished' where state='active' and policyexpire is not null and policyexpire < DATE_SUB(Now(), INTERVAL 7 DAY)", cancellationToken);
                    /*  await db.Database.ExecuteSqlRawAsync(
                          "update preparedpaymenttransactions set state='expired' where (state='prepared' or state='active') and (expires < NOW() or created < DATE_SUB(NOW(),INTERVAL 4 HOUR))",
                          cancellationToken);
                    */
                }
                catch 
                {
                  
                }

                // Reset the Display for the Admintool
                await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid, redis);
        }
    }
}
