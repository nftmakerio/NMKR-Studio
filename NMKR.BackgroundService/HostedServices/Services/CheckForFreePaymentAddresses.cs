using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckForFreePaymentAddresses : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkfreepaymentaddresses;
            if (server.Checkforfreepaymentaddresses == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            await StaticBackgroundServerClass.LogAsync(db, "Check Free Address", "", serverid);
            var free = await (from a in db.Nftaddresses
                where a.State == "free" && a.Reservationtoken == null && a.Coin== Coin.ADA.ToString()
                              select a).CountAsync(cancellationToken: cancellationToken);
            await StaticBackgroundServerClass.LogAsync(db, $"Found {free} free addresses", "", serverid);
            if (free < 15000)
            {
                CryptographyProcessor cp = new();
                for (int i = 0; i < 100; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await StaticBackgroundServerClass.LogAsync(db, $"Create new Free Address {i} of 100", "",
                        serverid);
                    var cn = ConsoleCommand.CreateNewPaymentAddress(mainnet);
                    if (cn.ErrorCode != 0 || string.IsNullOrEmpty(cn.Address) ||
                        string.IsNullOrEmpty(cn.privateskey) || string.IsNullOrEmpty(cn.privatevkey)) continue;

                    await StaticBackgroundServerClass.LogAsync(db, $"New Address: {cn.Address}", "", serverid);

                    string salt = cp.CreateSalt(30);
                    string password = salt + GeneralConfigurationClass.Masterpassword;

                    Nftaddress newaddress = new()
                    {
                        Created = DateTime.Now, State = "free", Lovelace = 0,
                        Privatevkey = Encryption.EncryptString(cn.privatevkey, password),
                        Privateskey = Encryption.EncryptString(cn.privateskey, password), Price = 0,
                        Address = cn.Address, Expires = DateTime.Now.AddYears(1), NftprojectId = null, Salt = salt, Coin = Coin.ADA.ToString()
                    };
                    await db.Nftaddresses.AddAsync(newaddress, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);
                }
            }

            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }
    }
}
