using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.Cardano;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Blockchains;
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
    public class CheckCustomerAddresses : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkcustomeraddresses;
            if (string.IsNullOrEmpty(server.Checkcustomeraddressescoin) || counter % 5 != 0)
                return;


            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);
            foreach (var coin in server.Checkcustomeraddressescoin.Trim().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).OrEmptyIfNull())
            {
                await CheckCustomerAddressesAsync(db, cancellationToken, mainnet, serverid, redis, coin.ToEnum<Coin>());
            }

            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid, redis);
        }

        private async Task CheckCustomerAddressesAsync(EasynftprojectsContext db, CancellationToken cancellationToken,
            bool mainnet, int serverid, IConnectionMultiplexer redis, Coin coin)
        {

            IBlockchainFunctions blockchain = null;
            switch (coin)
            {
                case Coin.ADA:
                    blockchain = new CardanoBlockchainFunctions();
                    break;
                case Coin.SOL:
                    blockchain = new SolanaBlockchainFunctions();
                    break;
                case Coin.APT:
                    blockchain = new AptosBlockchainFunctions();
                    break;
            }

            if (blockchain == null)
                return;


            var cust=await blockchain.GetCustomersAsync(db, cancellationToken);

            int c = 0;
            foreach (var cust1 in cust)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                c++;
                if (cancellationToken.IsCancellationRequested)
                    break;


                // This is only for Display in the admintool - we use the projectid here
                await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.checkcustomeraddresses, mainnet, serverid, redis, cust1.CustomerId);


                await StaticBackgroundServerClass.LogAsync(db,
                    $"{c} of {cust.Length} - Checking Customer Address {cust1.Address} ",
                    "", serverid);


                string sql= $"update customers set checkaddresscount=checkaddresscount-1 where id={cust1.CustomerId} and checkaddresscount>0";
                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);


                var amount = await blockchain.GetWalletBalanceAsync(cust1.Address);

                await StaticBackgroundServerClass.LogAsync(db,
                    $"{c} of {cust.Length} - Amount: {amount}", "", serverid);


                string sql1 = $"update customers set blockcounter=blockcounter+1 where id={cust1.CustomerId} and addressblocked=1";
                await db.Database.ExecuteSqlRawAsync(sql1, cancellationToken);

                int maxvalue =
                    await GlobalFunctions.GetWebsiteSettingsIntAsync(db, "customerblockcountermaxvalue", 100);


                if ((cust1.Amount != (long)amount || cust1.Blockcounter >= maxvalue) && cust1.InternalAccount==false)
                {
                    var customer=await (from a in db.Customers
                            .Include(a=>a.Defaultsettings)
                                        where a.Id == cust1.CustomerId
                                        select a).FirstOrDefaultAsync(cancellationToken);


                    if (amount == 0)
                    {
                        customer.Blockcounter = 0;
                        customer.Addressblocked = false;

                        switch (coin)
                        {
                            case Coin.ADA:
                                customer.Lovelace = 0;
                                break;
                            case Coin.SOL:
                                customer.Lamports = 0;
                                break;
                            case Coin.APT:
                                customer.Octas = 0;
                                break;
                        }

                        await db.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"{cust1.CustomerId} Amount {cust1.Address} {amount} Blockcounter: {cust1.Blockcounter}",
                            "",
                            serverid);
                        try
                        {
                            BuildTransactionClass bt = new();
                            CreateNewPaymentAddressClass wallet = new CreateNewPaymentAddressClass()
                            {
                                Address = cust1.Address,
                                Blockchain = coin.ToBlockchain(),
                                SeedPhrase = Encryption.DecryptString(cust1.Seed, cust1.Salt),
                                privateskey = Encryption.DecryptString(cust1.PrivateKey, cust1.Salt),
                                privatevkey = Encryption.DecryptString(cust1.PublicKey, cust1.Salt),
                            };


                            var res = await blockchain.SendAllCoinsAndTokens(amount, wallet,
                                cust1.MintCouponsReceiverAddress, bt, "");


                            if (!string.IsNullOrWhiteSpace(res.TxHash))
                            {
                                await SaveBuyMintsTransaction(db, redis, cust1, amount, res.TxHash, serverid,
                                    amount / cust1.PriceMintCoupons, coin, cancellationToken);

                                customer.Purchasedmints += Convert.ToInt32((amount / cust1.PriceMintCoupons));
                                customer.Newpurchasedmints += (amount / cust1.PriceMintCoupons);
                                customer.Blockcounter = 0;
                                customer.Addressblocked = true;
                                customer.Lasttxhash = res.TxHash;
                                switch (coin)
                                {
                                    case Coin.ADA:
                                        customer.Lovelace = (long)amount;
                                        break;
                                    case Coin.SOL:
                                        customer.Lamports = (long)amount;
                                        break;
                                    case Coin.APT:
                                        customer.Octas = (long)amount;
                                        break;
                                }

                                await db.SaveChangesAsync(cancellationToken);

                                await StaticBackgroundServerClass.LogAsync(db,
                                    $"Reset Blockcounter and save new Amount: {amount} - {cust1.Address}",
                                    "", serverid);
                            }
                        }
                        catch (Exception e)
                        {
                            await StaticBackgroundServerClass.EventLogException(db, 744, e, serverid);
                        }
                    }

                }

                await StaticBackgroundServerClass.UpdateCustomerLastCheckForUtxo(db, cust1.CustomerId, serverid, coin);
            }
        }

        private async Task SaveBuyMintsTransaction(EasynftprojectsContext db, IConnectionMultiplexer redis, CheckCustomerAddressAddressesClass cust1, ulong utxo, string txhash, int serverid, float nftcount, Coin coin, CancellationToken cancellationToken)
        {
            var rates = await GlobalFunctions.GetNewRatesAsync(redis, coin);
            Transaction t = new Transaction()
            {
                Ada = 0,
                Created = DateTime.Now,
                Discount = 0,
                Cbor = "",
                CustomerId = cust1.CustomerId,
                Projectada = 0,
                Confirmed = true,
                Checkforconfirmdate = DateTime.Now,
                Receiveraddress = cust1.Address,
                State = "submitted",
                Mintingcostsada = (long)utxo,
                Mintingcostsaddress = "",
                Projectaddress = "",
                Transactionid = txhash,
                Transactiontype = "buymints",
                Fee = 0,
                Eurorate = (float)rates.EurRate,
                Serverid = serverid,
                Nftcount = Convert.ToInt32(nftcount),
                Coin = coin.ToString(),
                Paymentmethod =coin.ToString(),
            };
            await db.Transactions.AddAsync(t, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            Onlinenotification on = new()
            {
                Created = DateTime.Now,
                CustomerId = cust1.CustomerId,
                Notificationmessage =
                    $"{nftcount} Mint coupons credited",
                State = "new",
                Color = "success"
            };
            await db.Onlinenotifications.AddAsync(on, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
