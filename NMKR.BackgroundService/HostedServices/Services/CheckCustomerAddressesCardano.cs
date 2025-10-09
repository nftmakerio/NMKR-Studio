using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Cardano_Sharp;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckCustomerAddressesCardano : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkcustomeraddresses;
            if (server.Checkcustomeraddresses == false || counter % 5 != 0)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask,mainnet,serverid,redis);
            await StaticBackgroundServerClass.LogAsync(db,$"{backgroundtask} {Environment.MachineName}", "", serverid);

            //c = 0;
            var cust = await (from a in db.Customers
                    .Include(a => a.Loggedinhashes)
                    .Include(a=>a.Defaultsettings)
                    .AsSplitQuery()
                where a.State == "active" && a.Adaaddress != "" && a.Adaaddress != null && ((a.Loggedinhashes.Any() &&  a.Loggedinhashes.OrderByDescending(x => x.Id).First().Lastlifesign > DateTime.Now.AddMinutes(-30)) ||
                    a.Checkaddressalways || a.Checkaddresscount > 0 || a.Lastcheckforutxo == null || a.Addressblocked ||
                    a.Lastcheckforutxo < DateTime.Now.AddDays(-5))
                orderby a.Lastcheckforutxo
                select a).Take(1000).ToListAsync(cancellationToken: cancellationToken);

            int c = 0;
            foreach (var cust1 in cust)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                c++;
                if (c % 5 == 0 && await StaticBackgroundServerClass.CheckStopServerAsync(db, serverid, cancellationToken))
                    break;

                if (cancellationToken.IsCancellationRequested)
                    break;

                var bg = await (from a in db.Backgroundservers
                    where a.Actualprojectid == cust1.Id && a.Checkcustomeraddresses == true &&
                          a.Actualtask == "checkcustomeraddresses"
                    select a).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (bg != null)
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"{c} of {cust.Count} - Customer Address will be checked by an other server - skipping {cust1.Adaaddress} Blockcounter: {cust1.Blockcounter} Blocked: {cust1.Addressblocked}",
                        "", serverid);
                    continue;
                }

                // This is only for Display in the admintool - we use the projectid here
                await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.checkcustomeraddresses,mainnet,serverid,redis, cust1.Id);


                await StaticBackgroundServerClass.LogAsync(db,
                    $"{c} of {cust.Count} - Checking Account Address {cust1.Adaaddress}Blockcounter: {cust1.Blockcounter} Blocked: {cust1.Addressblocked}",
                    "", serverid);

                if (cust1.Checkaddresscount > 0)
                {
                    cust1.Checkaddresscount--;
                    await db.SaveChangesAsync(cancellationToken);
                }

                // Check both APIs - when one api has not the same amount of lovelace - we skip this address
                var utxo = await ConsoleCommand.GetNewUtxoAsync(cust1.Adaaddress);
              
                await StaticBackgroundServerClass.LogAsync(db,
                    $"{c} of {cust.Count} - Lovelace: {utxo.LovelaceSummary}", "", serverid);


                if (cust1.Addressblocked)
                {
                    cust1.Blockcounter++;
                    await db.SaveChangesAsync(cancellationToken);
                }
              

                int maxvalue = 100;
                try
                {
                    maxvalue =
                        Convert.ToInt32(
                            await GlobalFunctions.GetWebsiteSettingsStringAsync(db, "customerblockcountermaxvalue"));
                }
                catch
                {
                    maxvalue = 100;
                }


                long adaamount = utxo.LovelaceSummary;
                if (cust1.Lovelace != adaamount || cust1.Blockcounter >= maxvalue || (!string.IsNullOrEmpty(cust1.Lasttxhash) && await LastTxHashConfirmedAsync(cust1.Lasttxhash)) )
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"{cust1.Id} Lovelace {cust1.Adaaddress} {adaamount} Blockcounter: {cust1.Blockcounter}", "",
                        serverid);
                    try
                    {
                        if (!cust1.Internalaccount && adaamount > 1500000)
                        {
                            string payskey = Encryption.DecryptString(cust1.Privateskey, cust1.Salt);
                            string payvkey = Encryption.DecryptString(cust1.Privatevkey, cust1.Salt);

                            BuildTransactionClass bt = new();
                            string receiveraddress = cust1.Defaultsettings.Mintingaddress;
                            foreach (var txInClass in utxo.TxIn)
                            {

                                if (txInClass.Lovelace<1500000)
                                    continue;


                                if (txInClass.Tokens != null && txInClass.Tokens.Any())
                                {
                                    await SendBackMintPurchaseAsync(db,redis,mainnet,cust1.Adaaddress, txInClass, payskey,payvkey,"Please send no tokens",serverid, cancellationToken);
                                    continue;
                                }

                                var transaction = await (from a in db.Transactions
                                    where a.Transactionid == txInClass.TxHashId
                                    select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);
                                if (transaction!=null)
                                    continue;


                                var res = CardanoSharpFunctions.SendAllAdaAndTokens(db, redis, cust1.Adaaddress, payskey, payvkey,"",
                                    receiveraddress,
                                    GlobalFunctions.IsMainnet(), ref bt, txInClass.TxHashId, 0);


                                if (res == "OK")
                                {
                                    float priceMintCoupons = cust1.Defaultsettings.Pricemintcoupons;
                                    if (priceMintCoupons == 0)
                                        priceMintCoupons = 4500000;

                                    cust1.Purchasedmints += Convert.ToInt32(txInClass.Lovelace / priceMintCoupons);
                                    cust1.Newpurchasedmints += (txInClass.Lovelace / priceMintCoupons);
                                    await db.SaveChangesAsync(cancellationToken);

                                    await SaveBuyMintsTransaction(db,redis, cust1, txInClass, serverid,
                                        Convert.ToInt32(txInClass.Lovelace / priceMintCoupons),Coin.ADA, cancellationToken);
                                }

                                cust1.Blockcounter = 0;
                                cust1.Addressblocked = true;
                                cust1.Lasttxhash = bt.TxHash;
                                await db.SaveChangesAsync(cancellationToken);
                             
                                await StaticBackgroundServerClass.LogAsync(db,
                                    $"Reset Blockcounter and save new Lovelace: {txInClass.Lovelace} OLD: {cust1.Lovelace} - {cust1.Adaaddress}",
                                    "", serverid);
                            }
                        }
                        else
                        {
                            var cust2 = await (from a in db.Customers
                                where a.Id == cust1.Id
                                select a).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                            cust2.Lovelace = adaamount;
                            cust2.Blockcounter = 0;
                            cust2.Addressblocked = false;
                            cust2.Lasttxhash = "";
                            await db.SaveChangesAsync(cancellationToken);
                         
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Reset Blockcounter and save new Lovelace: {adaamount} OLD: {cust1.Lovelace} - {cust1.Adaaddress}",
                                "", serverid);

                       
                        }
                    }
                    catch (Exception e)
                    {
                        await StaticBackgroundServerClass.EventLogException(db, 7, e, serverid);
                    }

                }

                if (cust1.Internalaccount && adaamount > 0)
                {
                    await db.SaveChangesAsync(cancellationToken);
                    await CheckCountTxIn(db, cust1, utxo, serverid, mainnet, redis, Coin.ADA, cancellationToken);
                }

                await StaticBackgroundServerClass.UpdateCustomerLastCheckForUtxo(db, cust1.Id, serverid, Coin.ADA);
            }

            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,redis);
        }

        private async Task SendBackMintPurchaseAsync(EasynftprojectsContext db, IConnectionMultiplexer redis,bool mainnet, string senderaddress, TxInClass txInClass, string payskey, string payvkey, string sendbackMessage, int serverid, CancellationToken cancellationToken)
        {
            string receiveraddress = await ConsoleCommand.GetSenderAsync(txInClass.TxHashId);
            if (!string.IsNullOrEmpty(receiveraddress))
            {
                BuildTransactionClass bt = new BuildTransactionClass();
                var ok= CardanoSharpFunctions.SendAllAdaAndTokens(db, redis, senderaddress, payskey, payvkey, "",
                    receiveraddress,
                    mainnet, ref bt, txInClass.TxHashId, 0, 0, sendbackMessage);
                if (ok != "OK")
                {
                    await GlobalFunctions.LogMessageAsync(db, $"Error while sending back mint purcahse {txInClass.TxHashId}", ok + Environment.NewLine+
                        JsonConvert.SerializeObject(txInClass) + Environment.NewLine+bt.LogFile, serverid);
                }
            }
            else
            {
                await GlobalFunctions.LogMessageAsync(db, $"Could not determine recevier {txInClass.TxHashId}",
                    JsonConvert.SerializeObject(txInClass), serverid);
            }
        }

        private async Task SaveBuyMintsTransaction(EasynftprojectsContext db, IConnectionMultiplexer redis,
            Customer cust1, TxInClass txin, int serverid, int nftcount,Coin coin, CancellationToken cancellationToken)
        {

            var rates = await GlobalFunctions.GetNewRatesAsync(redis, coin);

            long ada = txin.Lovelace;
            Transaction t = new Transaction()
            {
                Ada = 0,
                Created = DateTime.Now,
                Discount = 0,
                Cbor = "",
                CustomerId = cust1.Id,
                Projectada = 0,
                Confirmed = true,
                Checkforconfirmdate = DateTime.Now,
                Receiveraddress = cust1.Adaaddress,
                State = "submitted",
                Mintingcostsada = ada,
                Mintingcostsaddress = "",
                Projectaddress = "",
                Transactionid = txin.TxHashId,
                Transactiontype = "buymints",
                Fee = 0,
                Coin = coin.ToString(),
                Eurorate =(float)rates.EurRate,
                Serverid = serverid,
                Nftcount = nftcount,
            };
            await db.Transactions.AddAsync(t, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);


            Onlinenotification on = new()
            {
                Created = DateTime.Now,
                CustomerId = cust1.Id,
                Notificationmessage =
                    $"{nftcount} Mint coupons credited",
                State = "new",
                Color = "success"
            };
            await db.Onlinenotifications.AddAsync(on, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

        }

        private async Task<bool> LastTxHashConfirmedAsync(string txhash)
        {
            if (string.IsNullOrEmpty(txhash)) return true;
            var txinfo = await ConsoleCommand.GetTransactionAsync(txhash);
            return txinfo != null;
        }

        private async Task CheckCountTxIn(EasynftprojectsContext db, Customer cust1, TxInAddressesClass utxo, int serverid, bool mainnet, IConnectionMultiplexer redis,Coin coin,
        CancellationToken cancellationToken)
        {
            int counttxin = utxo.TxIn == null ? 0 : utxo.TxIn.Length;
            int MaxTinInBeforConsolidate = 35;
            var payoutrequests = await (from a in db.Payoutrequests
                                        where a.CustomerId == cust1.Id && a.State == "execute"
                                        select a).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (payoutrequests != null)
            {
                if (counttxin < 50)
                    return;
            }

            await StaticBackgroundServerClass.LogAsync(db, $"Count TX-In on Address: {cust1.Adaaddress} = {counttxin}", "",
                serverid);
            if (counttxin >= MaxTinInBeforConsolidate)
            {
                for (int i = 0; i < Convert.ToInt32(Math.Ceiling((double)(counttxin / MaxTinInBeforConsolidate))); i++)
                {
                    if (i > 50)
                        break;

                    BuildTransactionClass buildtransaction = new();
                    await StaticBackgroundServerClass.LogAsync(db, $"Too many TX-IN - Consolidate Account {cust1.Adaaddress}", "",
                        serverid);

                    var ok = CardanoSharpFunctions.SendAllAdaAndTokens(db, redis, cust1.Adaaddress,cust1.Privateskey,cust1.Privatevkey,cust1.Salt,cust1.Adaaddress, mainnet,
                        ref buildtransaction,null, MaxTinInBeforConsolidate, i, null,utxo);

                    if (ok == "OK")
                    {
                        // Changed to sql because of some problems with concurrent access
                        //    string sql="update customers set addressblocked=1,blockcounter=0,checkaddresscount=100 where id="+cust1.Id;
                        //   await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
                        try
                        {
                            GlobalFunctions.ResetContextState(db);
                            cust1.Addressblocked = true;
                            cust1.Blockcounter = 0;
                            cust1.Checkaddresscount = 100;
                            await db.SaveChangesAsync(cancellationToken);
                        }
                        catch (Exception e)
                        {
                            await StaticBackgroundServerClass.EventLogException(db, 8, e, serverid);
                        }

                        await StaticBackgroundServerClass.LogAsync(db, $"Consolidate successful {cust1.Adaaddress}", "",
                            serverid);
                        if (buildtransaction.BuyerTxOut == null)
                            return;
                        try
                        {
                            var rates = await GlobalFunctions.GetNewRatesAsync(redis, coin);
                            Transaction t = new()
                            {
                                Senderaddress = buildtransaction.SenderAddress,
                                Receiveraddress = buildtransaction.BuyerTxOut.ReceiverAddress,
                                Ada = buildtransaction.BuyerTxOut.Amount,
                                Created = DateTime.Now,
                                CustomerId = cust1.Id,
                                NftaddressId = null,
                                NftprojectId = null,
                                Transactiontype = "consolitecustomeraddress",
                                Transactionid = buildtransaction.TxHash,
                                Discount = 0,
                                Stakereward = 0,
                                Fee = buildtransaction.Fees,
                                Projectaddress = buildtransaction.ProjectTxOut?.ReceiverAddress,
                                Projectada = buildtransaction.ProjectTxOut?.Amount,
                                Mintingcostsaddress = buildtransaction.MintingcostsTxOut?.ReceiverAddress,
                                Mintingcostsada = buildtransaction.MintingcostsTxOut?.Amount ??0,
                                State = "submitted",
                                Serverid = serverid,
                                Coin = coin.ToString(),
                                Eurorate = (float)rates.EurRate,
                            };
                            await db.AddAsync(t, cancellationToken);
                            await db.SaveChangesAsync(cancellationToken);
                        }
                        catch (Exception e)
                        {
                            await StaticBackgroundServerClass.EventLogException(db, 9, e, serverid);
                        }
                    }
                    else
                    {
                        await StaticBackgroundServerClass.LogAsync(db, $"Consolidate FAILED {cust1.Adaaddress}", "", serverid);
                    }
                }
            }
        }
      
    }
}
