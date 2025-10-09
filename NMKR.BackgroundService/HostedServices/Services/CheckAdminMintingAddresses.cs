using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.BITCOIN;
using NMKR.Shared.Blockchains.Cardano;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckAdminMintingAddresses : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkadminmintingaddresses;
            if (server.Checkmintandsend == false)
                return;
         
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            var activeblockchains = await (from a in db.Activeblockchains
                where a.Enabled == true 
                select a).AsNoTracking().ToListAsync(cancellationToken: cancellationToken);


          

            foreach (var activeblockchain in activeblockchains)
            {
                var countadminmintandsendaddresses =
                    await GlobalFunctions.GetWebsiteSettingsIntAsync(db, $"countadminmintandsendaddresses_{activeblockchain.Name}", 10);

                var countMintingAddresses = await (from a in db.Adminmintandsendaddresses
                    where a.Coin == activeblockchain.Coinname
                    select a).CountAsync(cancellationToken);


                var adminMintingAddresses = await (from a in db.Adminmintandsendaddresses
                    where a.Coin == activeblockchain.Coinname && (a.Lastcheckforutxo<DateTime.Now.AddMinutes(-2) || a.Addressblocked || !string.IsNullOrEmpty(a.Lasttxhash) || a.Blockcounter>0)
                                                   select a).ToListAsync(cancellationToken: cancellationToken);

                IBlockchainFunctions blockchainFunctions = activeblockchain.Name.ToEnum<Blockchain>() switch
                {
                    Blockchain.Cardano => new CardanoBlockchainFunctions(),
                    Blockchain.Solana => new SolanaBlockchainFunctions(),
                    Blockchain.Aptos => new AptosBlockchainFunctions(),
                    Blockchain.Bitcoin => new BitcoinBlockchainFunctions(),
                    _ => null
                };

                if (blockchainFunctions == null)
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"No Blockchain Functions found for {activeblockchain.Name}", "", serverid);
                    continue;
                }


                if (countMintingAddresses < countadminmintandsendaddresses)
                {
                    CryptographyProcessor cp = new();
                    string password = cp.CreateSalt(30);

                    var adr = blockchainFunctions.CreateNewWallet();

                    db.Adminmintandsendaddresses.Add(new()
                    {
                        Address = adr.Address,
                        Addressblocked = false,
                        Blockcounter = 0,
                        Lovelace = 0,
                        Privateskey = Encryption.EncryptString(adr.privateskey, GeneralConfigurationClass.Masterpassword + password),
                        Privatevkey = Encryption.EncryptString(adr.privatevkey, GeneralConfigurationClass.Masterpassword + password),
                        Seed = Encryption.EncryptString(adr.SeedPhrase, GeneralConfigurationClass.Masterpassword + password),
                        Salt = password,
                        Coin = activeblockchain.Coinname, 
                    });
                    try
                    {
                        await db.SaveChangesAsync(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        await StaticBackgroundServerClass.EventLogException(db, 173, e, serverid);
                    }
                }


                foreach (var adminmintandsendaddress in adminMintingAddresses)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // Set Last Check for UTXO to now, so we do not check it again in the next 2 minutes - until it is used and the address is blocked
                    adminmintandsendaddress.Lastcheckforutxo = DateTime.Now;
                    await db.SaveChangesAsync(cancellationToken);


                    var utxo = await blockchainFunctions.GetWalletBalanceAsync(adminmintandsendaddress.Address);
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Check Admin Minting Address {activeblockchain.Name} - {adminmintandsendaddress.Address}",
                        "", serverid);
                    if (utxo <= 0) continue;

                    if (adminmintandsendaddress.Addressblocked)
                    {
                        adminmintandsendaddress.Blockcounter++;
                        await db.SaveChangesAsync(cancellationToken);
                    }

                    int maxvalue = await GlobalFunctions.GetWebsiteSettingsIntAsync(db,
                        "adminmintandsendaddressesblockcountermaxvalue", 100);


                    if ((string.IsNullOrEmpty(adminmintandsendaddress.Lasttxhash) &&
                         adminmintandsendaddress.Lovelace != (long)utxo) ||
                        adminmintandsendaddress.Blockcounter >= maxvalue ||
                        (adminmintandsendaddress.Lasttxdate != null &&
                         adminmintandsendaddress.Lasttxdate.Value < DateTime.Now.AddMinutes(-20)) ||
                        (!string.IsNullOrEmpty(adminmintandsendaddress.Lasttxhash) &&
                         await LastTxHashConfirmed(blockchainFunctions, adminmintandsendaddress.Lasttxhash)))
                    {

                        try
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Reset Admin Minting Address Blockcounter and save new Lovelace: {utxo} OLD: {adminmintandsendaddress.Lovelace} - {adminmintandsendaddress.Address}",
                                JsonConvert.SerializeObject(adminmintandsendaddress,
                                    new JsonSerializerSettings
                                        { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                                serverid);
                            adminmintandsendaddress.Blockcounter = 0;
                            adminmintandsendaddress.Addressblocked = false;
                            adminmintandsendaddress.Lovelace = (long)utxo;
                            adminmintandsendaddress.Lasttxdate = null;
                            adminmintandsendaddress.Lasttxhash = "";
                            adminmintandsendaddress.Reservationtoken = "";
                            await db.SaveChangesAsync(cancellationToken);
                        }
                        catch (Exception e)
                        {
                            await StaticBackgroundServerClass.EventLogException(db, 17, e, serverid);
                        }

                    }
                }
            }

            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }
        private async Task<bool> LastTxHashConfirmed(IBlockchainFunctions blockchainFunctions, string txhash)
        {
            if (string.IsNullOrEmpty(txhash)) return true;

            var txinfo = await blockchainFunctions.GetTransactionInformation(txhash);
            return txinfo != null;
        }
    }
}
