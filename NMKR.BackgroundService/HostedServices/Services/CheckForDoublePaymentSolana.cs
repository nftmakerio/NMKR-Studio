using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckForDoublePaymentSolana : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkdoublepayments;
            if (server.Checkdoublepayments == false || counter % 10 != 0)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);



            int take = 100;

            var addresses1 = await (from a in db.Getaddressesfordoublepayments
                where a.Coin == Coin.SOL.ToString() && a.State!="error"
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
                    p = $" - Paydate: {((DateTime) adr.Paydate).ToShortDateString()} {((DateTime) adr.Paydate).ToLongTimeString()}";
                if (adr.Lastcheckforutxo != null)
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"{zr} of {addresses1.Count} - Check Addresses for Double Payment - State: {adr.State}{p} - Address: {adr.Address} Lastchecked: {((DateTime) adr.Lastcheckforutxo).ToShortDateString()} - {((DateTime) adr.Lastcheckforutxo).ToLongTimeString()} - Created: {((DateTime) adr.Created).ToShortDateString()} - {((DateTime) adr.Lastcheckforutxo).ToLongTimeString()}",
                        "", serverid);

                adr.Lastcheckforutxo = DateTime.Now;
                adr.Checkfordoublepayment = false;
                await db.SaveChangesAsync(cancellationToken);
                var sol = await SolanaFunctions.GetWalletBalanceAsync(adr.Address);
                if (sol == 0)
                {
                    continue;
                }
                var senderaddress = await SolanaFunctions.GetSenderAsync(adr);
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Found Double/expired Payment on Address {adr.Address} - Lamports: {sol} - Send SOL back to sender",
                    "", serverid);

                adr.Utxo = (long) sol;
                await db.SaveChangesAsync(cancellationToken);

                if (string.IsNullOrEmpty(senderaddress))
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Can not determinate Senderaddress for double payment on Address {adr.Address} - Lamports: {sol}",
                        "", serverid);
                    continue;
                }

                BuildTransactionClass buildtransaction = new BuildTransactionClass();

                var s = await SolanaFunctions.SendAllCoinsAndTokensFromNftaddress(adr,
                    string.IsNullOrEmpty(adr.Refundreceiveraddress) ? senderaddress : adr.Refundreceiveraddress,
                    buildtransaction,
                    "Double or expired Payment.Amount returned.");


                await StaticBackgroundServerClass.LogAsync(db,
                    $"SendAllAdaAndTokensFromNftAddresses {adr.Address} {adr.State} {senderaddress} - {s} - {sol}",
                    JsonConvert.SerializeObject(adr,
                        new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}) +
                    Environment.NewLine +
                    buildtransaction.LogFile, serverid);

                if (!string.IsNullOrEmpty(s.TxHash))
                {
                    adr.Utxo = 0;
                    await db.SaveChangesAsync(cancellationToken);
                    try
                    {
                        if (adr.Nftproject == null)
                        {
                            await StaticBackgroundServerClass.SaveTransactionToDatabase(db, redis,
                                buildtransaction,
                                null,
                                adr.Id, adr.NftprojectId,
                                nameof(TransactionTypes.doublepaymentsendbacktobuyer), null, serverid,
                                Coin.SOL);
                        }
                        else
                        {
                            await StaticBackgroundServerClass.SaveTransactionToDatabase(db, redis,
                                buildtransaction,
                                adr.Nftproject.CustomerId,
                                adr.Id, adr.NftprojectId,
                                nameof(TransactionTypes.doublepaymentsendbacktobuyer), null, serverid,
                                Coin.SOL);
                        }
                    }
                    catch (Exception e)
                    {
                        await StaticBackgroundServerClass.EventLogException(db, 12, e, serverid);
                    }

                    try
                    {
                        await db.SaveChangesAsync(cancellationToken);
                        // Save Refundlog
                        if (adr.NftprojectId != null)
                            await GlobalFunctions.SaveRefundLogAsync(db, adr.Address, senderaddress,
                                s.TxHash,
                                !string.IsNullOrEmpty(s.TxHash), buildtransaction.TxHash,
                                "Double Payment or Expired", (int) adr.NftprojectId,
                                buildtransaction.LogFile, (long) sol, buildtransaction.Fees,
                                buildtransaction.NmkrCosts, Coin.SOL);
                    }
                    catch (Exception e)
                    {
                        await StaticBackgroundServerClass.EventLogException(db, 22, e, serverid);
                    }
                }
                else
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Can not determinate Senderaddress for double payment on Address {adr.Address} - Lamports: {sol}",
                        "", serverid);
                }


            }



            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }
    }
}
