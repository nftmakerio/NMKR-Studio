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
    public class CheckForDoublePaymentCardano : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkdoublepayments;
            if (server.Checkdoublepayments == false || counter % 10 !=0)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);



            int take = 50;

            var addresses1 = await (from a in db.Getaddressesfordoublepayments
                where a.Coin == Coin.ADA.ToString()
                                    select a).Take(take).AsNoTracking().ToListAsync(cancellationToken);


            int zr = 0;
            foreach (var adr1 in addresses1)
            {

                var adr = await (from a in db.Nftaddresses
                        .Include(a => a.Nftproject)
                        .AsSplitQuery()
                    where a.Id == adr1.Id 
                    select a).FirstOrDefaultAsync(cancellationToken);

                if (adr == null)
                    continue;

                zr++;
                if (cancellationToken.IsCancellationRequested)
                    break;

                string p = "";
                if (adr.State == "paid" && adr.Paydate != null)
                    p =
                        $" - Paydate: {((DateTime) adr.Paydate).ToShortDateString()} {((DateTime) adr.Paydate).ToLongTimeString()}";
                if (adr.Lastcheckforutxo != null)
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"{zr} of {addresses1.Count} - Check Addresses for Double Payment - State: {adr.State}{p} - Address: {adr.Address} Lastchecked: {((DateTime) adr.Lastcheckforutxo).ToShortDateString()} - {((DateTime) adr.Lastcheckforutxo).ToLongTimeString()} - Created: {((DateTime) adr.Created).ToShortDateString()} - {((DateTime) adr.Lastcheckforutxo).ToLongTimeString()}",
                        "", serverid);

                adr.Lastcheckforutxo = DateTime.Now;
                adr.Checkfordoublepayment = false;
                await db.SaveChangesAsync(cancellationToken);
                var utxo1 = await ConsoleCommand.GetNewUtxoAsync(adr.Address);

                var txin = utxo1.TxIn == null ? 0 : utxo1.TxIn.Length;

                for (int i = 0; i < txin; i++)
                {
                    var txhashx = utxo1.GetTxHashId(i);

                    long adaamount = utxo1.TxIn[i].Lovelace;

                    if (adaamount > 0)
                    {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Found Double/expired Payment on Address {adr.Address} - Lovelace: {adaamount} - Send ADA back to sender",
                                JsonConvert.SerializeObject(utxo1), serverid);

                        adr.Utxo = adaamount;
                        await db.SaveChangesAsync(cancellationToken);
                        if (!string.IsNullOrEmpty(txhashx))
                        {
                            string senderaddress = await ConsoleCommand.GetSenderAsync(txhashx);
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"GetSender - Senddoubleback:  {adr.Address} {adr.State} {senderaddress} - TXhash: {txhashx}", "",
                                serverid);
                            if (!string.IsNullOrEmpty(senderaddress))
                            {
                                BuildTransactionClass buildtransaction = new();

                                string password = adr.Nftproject?.Password;
                                if (!string.IsNullOrEmpty(adr.Salt))
                                    password = adr.Salt + GeneralConfigurationClass.Masterpassword;

                                // When pay with tokens
                                Adminmintandsendaddress paywallet = null;
                               // if (adr.Price == -1)
                                {
                                    paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db, serverid,"CheckForDoublePayments",null);
                                    if (paywallet == null)
                                    {
                                        await StaticBackgroundServerClass.LogAsync(db,
                                            $"CheckApiAddresses - All Paywallets are blocked - waiting: {adr.Address} - {adr.Nftproject.Projectname}",
                                            "", serverid);
                                        return;
                                    }
                                }

                                string s = CardanoSharpFunctions.SendAllAdaAndTokens(db,redis,adr.Address, adr.Privateskey,adr.Privatevkey, password,
                                    senderaddress, mainnet,  ref buildtransaction, txhashx,0,0,
                                    "Double or expired Payment. Amount returned.",null,paywallet);

                                await StaticBackgroundServerClass.LogAsync(db,
                                    $"SendAllAdaAndTokensFromNftAddresses {adr.Address} {adr.State} {senderaddress} - {s} - {adaamount}",JsonConvert.SerializeObject(adr,new JsonSerializerSettings{ ReferenceLoopHandling = ReferenceLoopHandling.Ignore })+Environment.NewLine+
                                    buildtransaction.LogFile, serverid);

                                if (s == "OK")
                                {
                                    adr.Utxo = 0;
                                    await db.SaveChangesAsync(cancellationToken);
                                    try
                                    {
                                        if (adr.Nftproject == null)
                                        {
                                            await StaticBackgroundServerClass.SaveTransactionToDatabase(db,redis,
                                                buildtransaction,
                                                null,
                                                adr.Id, adr.NftprojectId,
                                                nameof(TransactionTypes.doublepaymentsendbacktobuyer), null, serverid, Coin.ADA);
                                        }
                                        else
                                        {
                                            await StaticBackgroundServerClass.SaveTransactionToDatabase(db,redis,
                                                buildtransaction,
                                                adr.Nftproject.CustomerId,
                                                adr.Id, adr.NftprojectId,
                                                nameof(TransactionTypes.doublepaymentsendbacktobuyer), null, serverid, Coin.ADA);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        await StaticBackgroundServerClass.EventLogException(db, 127, e, serverid);
                                    }
                                }

                                try
                                {
                                    await db.SaveChangesAsync(cancellationToken);
                                    // Save Refundlog
                                    if (adr.NftprojectId != null)
                                        await GlobalFunctions.SaveRefundLogAsync(db, adr.Address, senderaddress, txhashx,
                                            s == "OK", buildtransaction.TxHash,
                                            "Double Payment or Expired", (int) adr.NftprojectId,
                                            buildtransaction.LogFile, adaamount, buildtransaction.Fees, buildtransaction.NmkrCosts, Coin.ADA);
                                }
                                catch (Exception e)
                                {
                                    await StaticBackgroundServerClass.EventLogException(db, 225, e, serverid);
                                }

                            }
                            else
                            {
                                await StaticBackgroundServerClass.LogAsync(db,
                                    $"Can not determinate Senderaddress for double payment on Address {adr.Address} - Lovelace: {adaamount}",
                                    "", serverid);
                            }
                        }


                    }

                }
            }



            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }
    }
}
