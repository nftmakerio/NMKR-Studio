using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.Cardano;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckPremintedPromotokenAddresses : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkpremintedaddresses;
            if (server.Checkforpremintedaddresses == false)
                return;
         
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);

         

            var countaddresses = await GlobalFunctions.GetWebsiteSettingsIntAsync(db, "countadminmintandsendaddresses", 10);


            var premintedtokens1=await (from a in db.Nftprojectsendpremintedtokens
                    .Include(a=>a.Blockchain)
                                       where a.State=="active"
                                       select new{a.PolicyidOrCollection, a.Blockchain.Name, a.BlockchainId, a.Tokenname}).ToListAsync(cancellationToken: cancellationToken);

            if (!premintedtokens1.Any())
                return;

            var premintedtokens = premintedtokens1.GroupBy(a => new { a.PolicyidOrCollection, a.Name, a.BlockchainId, a.Tokenname })
                .Select(a => a.First()).ToList();



            // First look if enoungh preminted addresses are available
            foreach (var premintedtoken in premintedtokens)
            {
                var premintedPromoAddressesCount = await (from a in db.Premintedpromotokenaddresses
                    where a.State != "disabled" && a.BlockchainId == premintedtoken.BlockchainId
                                                          select a).CountAsync(cancellationToken: cancellationToken);

                if (premintedPromoAddressesCount < countaddresses)
                {
                    CryptographyProcessor cp = new();
                    string salt = cp.CreateSalt(30);



                    string s = premintedtoken.Name;
                    IBlockchainFunctions blockchainFunctions = GetBlockchainFunctions(s.ToEnum<Blockchain>());
                     

                    var adr = blockchainFunctions.CreateNewWallet();

                    db.Premintedpromotokenaddresses.Add(new()
                    {
                        Address = adr.Address,
                        Seedphrase = adr.SeedPhrase,
                        Privatekey = Encryption.EncryptString(adr.privateskey, GeneralConfigurationClass.Masterpassword + salt),
                        Publickey = Encryption.EncryptString(adr.privatevkey, GeneralConfigurationClass.Masterpassword + salt),
                        Salt = salt,
                        BlockchainId = premintedtoken.BlockchainId,
                        Tokenname = premintedtoken.Tokenname,
                        PolicyidOrCollection = premintedtoken.PolicyidOrCollection,
                        Totaltokens = 0,
                        State = "empty",
                        Lastcheck = DateTime.Now,
                    });
                    await db.SaveChangesAsync(cancellationToken);
                }
            }


            // Then check if the addresses are blocked

            var premintedPromoAddresses = await (from a in db.Premintedpromotokenaddresses
                    .Include(a => a.Blockchain)
                where a.State != "disabled" && (a.Blockdate != null || a.Reservationtoken != null ||
                                                a.Lastcheck < DateTime.Now.AddHours(-5))
                select a).ToListAsync(cancellationToken: cancellationToken);
            foreach (var promoAddress in premintedPromoAddresses)
            {
                bool freeAddress = false;
                IBlockchainFunctions blockchainFunctions = GetBlockchainFunctions(promoAddress.Blockchain.Name.ToEnum<Blockchain>());

                var assets=await blockchainFunctions.GetAllAssetsInWalletAsync(redis,promoAddress.Address);

                // If address is blocked and the lockdate is reached, then unblock the address - or if the amount has changed
                if (promoAddress.State== "blocked")
                {
                    if (promoAddress.Blockdate!=null && promoAddress.Blockdate.Value.AddMinutes(20) < DateTime.Now)
                    {
                        freeAddress= true;
                    }
                    else
                    {
                        if (promoAddress.Totaltokens != GetTokencount(promoAddress, assets))
                        {
                            freeAddress = true;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(promoAddress.Lasttxhash) && await TransactionFound(promoAddress.Lasttxhash, blockchainFunctions))
                            {
                                freeAddress = true;
                            }
                        }
                    }
                }
                else
                {
                    // If address is not blocked and the amount has changed, then set the new value in the db
                    freeAddress = true;
                }

                if (freeAddress)
                {
                    promoAddress.Blockdate = null;
                    promoAddress.Lastcheck = DateTime.Now;
                    promoAddress.Totaltokens = GetTokencount(promoAddress, assets);
                    promoAddress.State = promoAddress.Totaltokens == 0 ? "empty" : "active";
                    promoAddress.Lasttxhash = null;
                    promoAddress.Reservationtoken = null;
                    await db.SaveChangesAsync(cancellationToken);
                }
            }




            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private async Task<bool> TransactionFound(string txhash, IBlockchainFunctions blockchainFunctions)
        {
            if (string.IsNullOrEmpty(txhash))
                return false;

            return await blockchainFunctions.GetTransactionInformation(txhash) != null;
        }


        private long GetTokencount(Premintedpromotokenaddress promoAddress, AssetsAssociatedWithAccount[] assets)
        {
            if (assets == null)
                return 0;
            try
            {
                var l = assets.Where(a =>
                    a.PolicyIdOrCollection == promoAddress.PolicyidOrCollection &&
                    a.AssetName == promoAddress.Tokenname).Sum(x => x.Quantity);
                return l;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return 0;
        }

        private IBlockchainFunctions GetBlockchainFunctions(Blockchain bc)
        {
            return bc switch
            {
                Blockchain.Cardano => new CardanoBlockchainFunctions(),
                Blockchain.Solana => new SolanaBlockchainFunctions(),
                Blockchain.Aptos => new AptosBlockchainFunctions(),
                _ => new CardanoBlockchainFunctions()
            };
        }
    }
}
