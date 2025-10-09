using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.SaleConditions;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckApiAddressesSolana : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkpaymentaddressessolana;
            if (server.Checkpaymentaddressessolana == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);

            List<Nftaddress> addresses = new List<Nftaddress>();
            try
            {
                // First look for the addresses where the payment is received
                addresses = await (from a in db.Nftaddresses
                                   where (a.State == "payment_received") &&
                                         (a.Serverid == null || a.Serverid == serverid) && a.Coin == Coin.SOL.ToString()
                                   orderby a.Lastcheckforutxo
                                   select a).AsNoTracking().ToListAsync(cancellationToken: cancellationToken);


                // If no one is found, then look for the active addresses
                if (addresses.Count == 0)
                {
                    addresses = await (from a in db.Nftaddresses
                                       where (a.State == "active" || a.State == "payment_received") &&
                                             (a.Serverid == null || a.Serverid == serverid) && a.Coin == Coin.SOL.ToString()
                                       orderby a.Lastcheckforutxo
                                       select a).Take(150).AsNoTracking().ToListAsync(cancellationToken: cancellationToken);
                }
            }
            catch (Exception e)
            {
                await StaticBackgroundServerClass.EventLogException(db, 50, e, serverid);
                GlobalFunctions.ResetContextState(db);
            }
            var addresseslist = (from a in addresses
                                 select a.Address).ToArray();

         /*   await StaticBackgroundServerClass.LogAsync(db,
                $"Checking Solana Payment Addresses - {addresses.Count}", JsonConvert.SerializeObject(addresseslist), serverid);*/

            int index = 0;
            foreach (var nftaddress in addresses)
            {
                index++;
                await CheckAddress(db, cancellationToken, mainnet, serverid, redis, nftaddress,
                    index + 1);
            }

       /*     await StaticBackgroundServerClass.LogAsync(db,
                $"Checking Solana Payment Addresses finished - {addresses.Count}", JsonConvert.SerializeObject(addresseslist), serverid);*/

            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private async Task CheckAddress(EasynftprojectsContext db, CancellationToken cancellationToken, bool mainnet,
            int serverid, IConnectionMultiplexer redis, Nftaddress address, int c)
        {

            var sol=await SolanaFunctions.GetWalletBalanceAsync(address.Address);

            if (sol == 0)
            {
                string sql =
                    $"update nftaddresses set lastcheckforutxo=now(), addresscheckedcounter=addresscheckedcounter+1, state='active' where id={address.Id}";
                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken: cancellationToken);
                return;
            }


            var addr = await (from a in db.Nftaddresses
                    .Include(a => a.Nfttonftaddresses)
                    .ThenInclude(a => a.Nft)
                    .AsSplitQuery()
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Customer)
                    .AsSplitQuery()
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Settings)
                    .AsSplitQuery()
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Solanacustomerwallet)
                    .AsSplitQuery()
                    .Include(a => a.Referer)
                    .AsSplitQuery()
                              where a.Id == address.Id
                              select a).FirstOrDefaultAsync(cancellationToken: cancellationToken);


            if (addr == null)
            {
                return;
            }

            // If the address is paid, then check if the txid is available. When there is already a txid, just continue. The address is already served.
            if (addr.State == "payment_received" && !string.IsNullOrEmpty(addr.Outgoingtxhash))
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Payment Address - {addr.Address} already paid - Price: {addr.Price} - Expires: {addr.Expires} - State: {addr.State}",
                    JsonConvert.SerializeObject(addr,
                        new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), serverid);
                addr.State = "paid";
                await db.SaveChangesAsync(cancellationToken);
                return;
            }


            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(
                BackgroundTaskEnums.checkpaymentaddressessolana, mainnet, serverid, redis, c);

           
            BuildTransactionClass bt = new BuildTransactionClass("Check Api Addresses Solana");
            bt.Log($"Check API/PGW Address");
            bt.Log(addr.Address);


            await StaticBackgroundServerClass.LogAsync(db,
                $"Checking Payment Address - {addr.Address} Lamports: {sol} - Price: {addr.Price} - Expires: {addr.Expires} - State: {addr.State}","", serverid);

            // Then get sender and Txid


                addr.Lovelace = (long)sol;
                IBlockchainFunctions blockchain = new SolanaBlockchainFunctions();
                addr.Senderaddress = await blockchain.GetLastSenderAddressAsync(addr.Address);
                addr.Addresscheckedcounter++;
                addr.Lastcheckforutxo = DateTime.Now;
                await db.SaveChangesAsync(cancellationToken);

                if (string.IsNullOrEmpty(addr.Senderaddress))
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"GetSender is null - {addr.Address} - TxId: {addr.Txid} Lamports: {sol} - Price: {addr.Price} - Expires: {addr.Expires} - State: {addr.State}",
                        "", serverid);
                    return;
                }
           


            await StaticBackgroundServerClass.LogAsync(db,
                $"Processing Payment Address - Lamports: {sol} - Price: {addr.Price} - {addr.Address} - Expires: {addr.Expires} - Project:{addr.Nftproject.Projectname} {addr.NftprojectId} - Customer: {addr.Nftproject.Customer.Firstname} {addr.Nftproject.Customer.Lastname} Company: {addr.Nftproject.Customer.Company}" ??
                "", "", serverid);


           if (!await CheckForCorrectAmountOfLamportAsync(db, cancellationToken, serverid, redis,
                    sol, addr))
            {
                return;
            }

            await StaticBackgroundServerClass.LogAsync(db,
                $"Processing Payment Address (2) - Lamports: {sol} - Price: {addr.Price} - {addr.Address} - Expires: {addr.Expires} - Project:{addr.Nftproject.Projectname} {addr.NftprojectId} - Customer: {addr.Nftproject.Customer.Firstname} {addr.Nftproject.Customer.Lastname} Company: {addr.Nftproject.Customer.Company}" ??
                "", "", serverid);


            // Check for Saleconditions. If not met, reject it
            var cond = await CheckSalesConditionClass.CheckForSaleConditionsMet(db, redis, addr.NftprojectId ?? 0,
                addr.Senderaddress,
                Math.Max(1, addr.Nfttonftaddresses.Sum(x => x.Tokencount)) *
                Math.Max(1, addr.Nftproject.Multiplier), serverid, addr.Nftproject.Usefrankenprotection, Blockchain.Solana);
         

            if (!await CheckForSaleConditions(db, cancellationToken, serverid, redis, cond, addr, sol))
                return;

            await StaticBackgroundServerClass.LogAsync(db,
                $"Processing Payment Address (3) - Lovelace: {sol} - Price: {addr.Price} - {addr.Address} - Expires: {addr.Expires} - Project:{addr.Nftproject.Projectname} {addr.NftprojectId} - Customer: {addr.Nftproject.Customer.Firstname} {addr.Nftproject.Customer.Lastname} Company: {addr.Nftproject.Customer.Company}" ??
                "", "", serverid);
       

            // If there are additional Payout wallets - check if the rest amount is enough - if not, reject it

        /*        long restprice = await StaticBackgroundServerClass.CheckRestPrice(db, addr.Nftproject,
                    addr.Nfttonftaddresses.Count, lovelace, addr.Senderaddress, addr.Reservationtoken);
                if (!await CheckRestPriceEnough(db, cancellationToken, mainnet, serverid, redis, restprice, addr, lovelace, txhash,
                        utxo))
                {
                    return;
                }*/

        
            if (await CheckForAlreadyMintedOnBlockchain(db, cancellationToken, mainnet, serverid, redis, addr)) return;
           

            var additionalPayouts = await (from a in db.Nftprojectsadditionalpayouts
                    .Include(a => a.Wallet)
                    .AsSplitQuery()
                where a.NftprojectId == addr.NftprojectId &&
                      a.Coin == Coin.SOL.ToString() &&
                      (a.Custompropertycondition == null || a.Custompropertycondition == "" || a.Custompropertycondition == addr.Customproperty)
                select a).AsNoTracking().ToArrayAsync(cancellationToken: cancellationToken);


            Pricelistdiscount discount = null;
            try
            {
                if (addr.Price > 0)
                    discount = await PriceListDiscountClass.GetPricelistDiscount(db, redis, addr.NftprojectId ?? 0,
                        addr.Senderaddress,  addr.Refererstring, addr.Customproperty, serverid, Blockchain.Solana);
                /*   stakerewards = await RewardsClass.GetStakePoolRewards(db, redis, addr.Senderaddress);
                   tokenrewards = await RewardsClass.GetTokenRewards(db, redis, addr.Senderaddress);*/
            }
            catch (Exception e)
            {
                bt.Log("Exception 88: " + e.Message);
                await StaticBackgroundServerClass.EventLogException(db, 88, e, serverid);
            }


            //
            // Make the Transaction
            //

            await StaticBackgroundServerClass.LogAsync(db,
                $"Send NFTs/Tokens for Address {addr.Address} - Project: {addr.Nftproject.Projectname}",
                "", serverid);

            var usedid = await WhitelistFunctions.SaveUsedAddressesToWhitelistSaleCondition(db,
                addr.NftprojectId ?? 0,
                addr.Senderaddress,
                addr.Senderaddress,  // cond.SendBackAddress?.OriginatorAddress, cond.SendBackAddress?.StakeAddress
                "", 
                bt.TxHash,
                Math.Max(1, addr.Nfttonftaddresses.Sum(x => x.Tokencount)) *
                Math.Max(1, addr.Nftproject.Multiplier));


            bt.Log("Start Minting");

            await StaticBackgroundServerClass.LogAsync(db,
                $"Mint from NFTAddress {addr.Address} - Receiver: {(!string.IsNullOrEmpty(addr.Optionalreceiveraddress) ? addr.Optionalreceiveraddress : addr.Senderaddress)} - Discount: {discount}",
                bt.LogFile, serverid);
            bool success = false;
            bt= await SolanaFunctions.MintFromNftAddressCoreAsync(sol, addr, !string.IsNullOrEmpty(addr.Optionalreceiveraddress) ? addr.Optionalreceiveraddress : addr.Senderaddress, discount, additionalPayouts, bt);

            if (!string.IsNullOrEmpty(bt.TxHash))
            {
                success = true;
                await StaticBackgroundServerClass.LogAsync(db, $"Minting successful {addr.Address} Transaction-ID: {bt.TxHash}",
                    bt.LogFile,
                    serverid);
              
            }
            else
            {
                await StaticBackgroundServerClass.LogAsync(db, $"Minting error {addr.Address}",
                    bt.LogFile,
                    serverid);
            }

            await StaticBackgroundServerClass.LogAsync(db,
                $"Transaction {addr.Address}{(success ? " - successful" : " - failed")}",
                bt.LogFile, serverid);
            addr.Submissionresult = bt.SubmissionResult;


            if (!success)
            {
                await SetAddressToError(db, cancellationToken, serverid, redis, addr, bt, sol, usedid);
                await GlobalFunctions.LogMessageAsync(db, "Solana Transaction error", bt.ErrorMessage);
            }
            else
            {
                await SetAddressToSuccess(db, cancellationToken, mainnet, serverid, redis, addr, bt, 0,0,DateTime.Now,null ,null,usedid);

                await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(
                    BackgroundTaskEnums.checkpaymentaddressessolana, mainnet, serverid, redis);
            }

        }

        private async Task<bool> CheckForCorrectAmountOfLamportAsync(EasynftprojectsContext db,
            CancellationToken cancellationToken,  int serverid, IConnectionMultiplexer redis, ulong lovelace,
            Nftaddress addr)
        {
            // Check if the amount was correct. If not reject it
            if (addr.Price <= 0 || (long)lovelace == addr.Price) return true;

            if ((long)lovelace > addr.Price && addr.Lovelaceamountmustbeexact == false)
                return true;


            {
                await SendTooLessOrTooMuchBackFromNftAddressesNewAsync(db, redis, addr, addr.Senderaddress, 
                    (long)lovelace > addr.Price
                        ? "Amount was too much - rejected"
                        : "Not enough ADA/Tokens sent - rejected", serverid, lovelace);
            }


            string rejectparameter =
                $"Expected {addr.Price} lovelace - received {lovelace} lovelace";
            await UpdateNftAddressesWithSqlCommand(db, cancellationToken, addr, rejectparameter, "amountwaswrong", "rejected");


            await StaticBackgroundServerClass.LogAsync(db,
                $"Amount was not correct: {addr.Address} - Project-Id:{addr.NftprojectId} - {addr.Senderaddress} - {lovelace}",
                "", serverid);

            await GlobalFunctions.ReleaseNftAsync(db, redis, addr.Id);

            return false;

        }

        // We check also against blockfrost to prevent double mints on different blockchains
        private async Task<bool> CheckForAlreadyMintedOnBlockchain(EasynftprojectsContext db,
         CancellationToken cancellationToken, bool mainnet, int serverid, IConnectionMultiplexer redis, Nftaddress addr)
        {
            // Last Bastion - check on Blockfrost and Helius - if there are some nfts already sold - reject it and set the nfts to sold - to prevent double mints

            bool failed = false;
            string resultjsonString = "";
            // Check on blockfrost if the NFT are really really really free
            foreach (var adrx in addr.Nfttonftaddresses)
            {
                if (StaticBackgroundServerClass.FoundNftInBlockchain(adrx.Nft, addr.Nftproject, mainnet,
                        out var bfq, out var resultjson))
                {
                    if (!(bfq + adrx.Tokencount > addr.Nftproject.Maxsupply)) continue;
                    failed = true;
                    adrx.Nft.Checkpolicyid = true;
                    if (addr.Nftproject.Maxsupply == 1)
                    {
                        adrx.Nft.Soldcount = 1;
                        adrx.Nft.State = "sold";
                        adrx.Nft.Reservedcount = 0;
                    }

                    resultjsonString += JsonConvert.SerializeObject(resultjson) + Environment.NewLine;
                }
            }

            if (failed)
            {
                addr.State = "error";
                await db.SaveChangesAsync(cancellationToken);
                await StaticBackgroundServerClass.LogAsync(db, $"Releasing NFTs: {addr.Address}", resultjsonString,
                    serverid);
                await GlobalFunctions.ReleaseNftAsync(db, redis, addr.Id);
                addr.Errormessage = "Error - One or more NFT marked as already sold";
                addr.Lastcheckforutxo = DateTime.Now;
                addr.Submissionresult = resultjsonString;
                await db.SaveChangesAsync(cancellationToken);
                await SetPreparedPaymenttransactionAsync(db, redis, PaymentTransactionsStates.error,
                    addr.Id, "", string.IsNullOrEmpty(addr.Optionalreceiveraddress) ? addr.Optionalreceiveraddress : addr.Senderaddress, addr, 0);

             /*   await SendTooLessOrTooMuchBackFromNftAddressesNewAsync(db, redis, addr, addr.Senderaddress, txhash,
                    "Error while preparing NFT. Please try again", utxo, serverid, mainnet, lovelace); */
                return true;
            }

            return false;
        }
    
        private async Task SetAddressToSuccess(EasynftprojectsContext db, CancellationToken cancellationToken, bool mainnet,
            int serverid, IConnectionMultiplexer redis, Nftaddress addr, BuildTransactionClass bt,
            long stakerewards, long tokenrewards, DateTime started, CheckConditionsResultClass cond, PromotionClass promotion,
            int?[] usedid)
        {
            await StaticBackgroundServerClass.LogAsync(db,
                $"Marking Transaction as PAID {addr.Address} - Project: {addr.NftprojectId} {addr.Nftproject.Projectname}",
                bt.LogFile, serverid);
            addr.State = "paid";
            addr.Paydate = DateTime.Now;
            addr.Stakereward = stakerewards > 0 ? stakerewards : null;
            addr.Tokenreward = tokenrewards > 0 ? tokenrewards : null;
            addr.Discount = bt.Discount;
            addr.Outgoingtxhash = bt.TxHash;
            await db.SaveChangesAsync(cancellationToken);

            await SetPreparedPaymenttransactionAsync(db, redis, PaymentTransactionsStates.finished,
                addr.Id, bt.TxHash, string.IsNullOrEmpty(addr.Optionalreceiveraddress) ? addr.Optionalreceiveraddress : addr.Senderaddress, addr, bt.Fees);

           

            await MarkAsSoldAsync(db, redis, addr,
                !string.IsNullOrEmpty(addr.Optionalreceiveraddress) ? addr.Optionalreceiveraddress : addr.Senderaddress,
                bt, serverid, mainnet);

            if (addr.Nftproject.Maxsupply == 1)
                StaticBackgroundServerClass.SaveMarkAsSoldToRedis(addr.Nfttonftaddresses, mainnet);

            bt.BuyerTxOut = new TxOutClass() {Amount = 0, ReceiverAddress = addr.Senderaddress, };


            TimeSpan ts = DateTime.Now - started;
            await StaticBackgroundServerClass.SaveTransactionToDatabase(db, redis, bt,
                addr.Nftproject.CustomerId,
                addr.Id, addr.NftprojectId, nameof(TransactionTypes.paidonftaddress),
                addr.Nftproject.SolanacustomerwalletId, serverid, Coin.SOL, (long)ts.TotalMilliseconds,
                addr, cond, promotion, bt.SignedTransaction, addr.Ipaddress, customproperty: addr.Customproperty, discountrefererstring: addr.Refererstring);

            await WhitelistFunctions.UpdateUsedAddressesToWhitelistSaleCondition(db, usedid, bt.TxHash);
        }

        private async Task SetAddressToError(EasynftprojectsContext db, CancellationToken cancellationToken, 
            int serverid, IConnectionMultiplexer redis, Nftaddress addr, BuildTransactionClass bt, ulong lamports, int?[] usedid)
        {
            addr.State = "error";
            addr.Submissionresult = bt.LogFile;
            await SetPreparedPaymenttransactionAsync(db, redis, PaymentTransactionsStates.error,
                addr.Id, "", string.IsNullOrEmpty(addr.Optionalreceiveraddress) ? addr.Optionalreceiveraddress : addr.Senderaddress, addr, 0);
            await db.SaveChangesAsync(cancellationToken);
            await StaticBackgroundServerClass.LogAsync(db,
                $"SOLANA - Marking NFT as Error because of Sale Problems (Api Addresses) {addr.Address} - Project: {addr.NftprojectId} {addr.Nftproject.Projectname}",
                bt.LogFile, serverid);
            await MarkAsErrorAsync(db, addr, bt.LogFile, serverid);

            await SendTooLessOrTooMuchBackFromNftAddressesNewAsync(db, redis, addr, addr.Senderaddress,
                "Error while minting NFT. Please try again",  serverid, lamports);
            await WhitelistFunctions.DeleteUsedAddressesToWhitelistSaleCondition(db, usedid);
        }

        private async Task<bool> CheckForSaleConditions(EasynftprojectsContext db, CancellationToken cancellationToken,
            int serverid, IConnectionMultiplexer redis, CheckConditionsResultClass cond, Nftaddress addr, ulong lamports)
        {
            if (cond.ConditionsMet != false)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Conditions met - {addr.Address} - {lamports} - Price: {addr.Price}Project-Id:{addr.NftprojectId}",
                    JsonConvert.SerializeObject(cond), serverid);
                return true;
            }
            await StaticBackgroundServerClass.LogAsync(db,
                $"Conditions not met - {addr.Address} - Sending back Adaamount: {lamports} - Price: {addr.Price} Project-Id:{addr.NftprojectId}",
                JsonConvert.SerializeObject(cond), serverid);
            await RejectDueToConditonsNotMetFromNftAddressesNewAsync(db, cancellationToken, redis, addr,
                addr.Senderaddress,
                lamports, cond, "Payment conditions not met. Contact the seller.", serverid);
               
            return false;

        }

        private static async Task UpdateNftAddressesWithSqlCommand(EasynftprojectsContext db,
            CancellationToken cancellationToken, Nftaddress addr, string rejectparameter, string rejectreason, string state)
        {
            string sql =
                $"update nftaddresses set state='{state}', paydate=now(), lastcheckforutxo=now(), rejectreason='{rejectreason}', rejectparameter='{rejectparameter}' where id={addr.Id}";
            try
            {
                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken: cancellationToken);
            }
            catch
            {
                await Task.Delay(500, cancellationToken);
                await db.Database.ExecuteSqlRawAsync(sql, cancellationToken: cancellationToken);
            }
        }

        private async Task MarkAsSoldAsync(EasynftprojectsContext db, IConnectionMultiplexer redis, Nftaddress addr, string senderaddress,
          BuildTransactionClass buildtransaction, int serverid, bool mainnet)
        {
            var ad = (from a in db.Nftaddresses
                    .Include(a => a.Nfttonftaddresses)
                    .ThenInclude(a => a.Nft)
                    .AsSplitQuery()
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Customer)
                    .AsSplitQuery()
                      where a.Id == addr.Id
                      select a).FirstOrDefault();

            if (ad == null)
                return;

            await StaticBackgroundServerClass.LogAsync(db, $"Mark all buyed NFT as Sold - {addr.Address}", "", serverid);
            try
            {
                foreach (var a in ad.Nfttonftaddresses)
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Mark as Sold NFT {a.Nft.Nftproject.Tokennameprefix}{a.Nft.Name} - Project: {a.Nft.Nftproject.Projectname} - {addr.Address}",
                        "", serverid);
                }

                if (ad.Nftproject.Maxsupply == 1)
                {
                    StaticBackgroundServerClass.SaveMarkAsSoldToRedis(ad.Nfttonftaddresses, mainnet);
                }

                try
                {
                    await NftReservationClass.MarkAllNftsAsSold(db, ad.Reservationtoken, addr.Nftproject.Cip68, senderaddress, Blockchain.Solana, !string.IsNullOrEmpty(addr.Nftproject.Solanacollectiontransaction) ? "mustbeadded" : "nocollection", buildtransaction);
                }
                catch (Exception e)
                {
                    await StaticBackgroundServerClass.EventLogException(db, 44, e, serverid);
                    await Task.Delay(1000);
                    GlobalFunctions.ResetContextState(db);

                    // Sometime this does not work
                    await using EasynftprojectsContext db2 =
                        new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
                    await GlobalFunctions.LogExceptionAsync(db, "Try to lock again: " + ad.Reservationtoken, e.Message,
                        serverid);
                    await NftReservationClass.MarkAllNftsAsSold(db2, ad.Reservationtoken, addr.Nftproject.Cip68,senderaddress, Blockchain.Solana, !string.IsNullOrEmpty(addr.Nftproject.Solanacollectiontransaction) ? "mustbeadded" : "nocollection", buildtransaction);
                }
            }
            catch (Exception e)
            {
                GlobalFunctions.ResetContextState(db);
                await StaticBackgroundServerClass.EventLogException(db, 11, e, serverid);
            }


            try
            {
                // Check if the routine has worked - this is, whem the mysql procedure has not marked it as sold
                if (ad.Nftproject.Maxsupply == 1)
                {
                    foreach (var a in ad.Nfttonftaddresses)
                    {
                        var nftx = await (from a1 in db.Nfts
                                          where a1.Id == a.NftId
                                          select a1).AsNoTracking().FirstOrDefaultAsync();

                        if (nftx == null)
                            continue;

                        if (nftx.State == "sold") continue;

                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Ups - the Mysql Routine has not marked this NFT als sold - {addr.Address} - {nftx.Id} {nftx.Name} {nftx.State}", "", serverid);

                        var nftx2 = await (from a1 in db.Nfts
                                           where a1.Id == a.NftId
                                           select a1).FirstOrDefaultAsync();

                        nftx2.State = "sold";
                        nftx2.Soldcount = 1;
                        nftx2.Receiveraddress = senderaddress;
                        nftx2.Buildtransaction = buildtransaction.Command;
                        nftx2.Soldby = "normal";
                        var id = nftx2.InstockpremintedaddressId;
                        nftx2.InstockpremintedaddressId = null;
                        nftx2.Instockpremintedaddress = null;
                        nftx2.Verifiedcollectionsolana = !string.IsNullOrEmpty(addr.Nftproject.Solanacollectiontransaction) ? "mustbeadded" : "nocollection";
                        nftx2.Selldate = DateTime.Now;
                        nftx2.Mintedonblockchain = "Solana";
                        
                        if (id != null)
                        {
                            await StaticBackgroundServerClass.LogAsync(db, $"Reset PremintedAddress to free: {id}", "", serverid);
                            await GlobalFunctions.ClearInstockPremintedAddressAsync(db, id);
                        }

                        await db.SaveChangesAsync();
                    }
                }
                await db.Database.ExecuteSqlRawAsync(
                    $"DELETE from nftreservations where reservationtoken='{ad.Reservationtoken}'");

                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                GlobalFunctions.ResetContextState(db);
                await StaticBackgroundServerClass.EventLogException(db, 133, e, serverid);
            }
        }



        private async Task MarkAsErrorAsync(EasynftprojectsContext db, Nftaddress addr, string logfile, int serverid)
        {
            await StaticBackgroundServerClass.LogAsync(db, $"Mark all buyed NFT as Error {addr.Address} {addr.NftprojectId}", logfile, serverid);
            try
            {
                var ad = (from a in db.Nftaddresses
                          .Include(a => a.Nfttonftaddresses)
                          .ThenInclude(a => a.Nft)
                          .AsSplitQuery()
                          .Include(a => a.Nftproject)
                          .ThenInclude(a => a.Customer)
                          .AsSplitQuery()
                          where a.Id == addr.Id
                          select a).FirstOrDefault();

                if (ad != null)
                {
                    foreach (var a in ad.Nfttonftaddresses)
                    {
                        if (ad.Nftproject.Maxsupply != 1) continue;
                        if (a.Nft.State != "sold")
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Mark as Error NFT {a.Nft.Name} - {a.Nft.Id}- {addr.Address}", logfile, serverid);
                            a.Nft.State = "error";
                            a.Nft.Errorcount = 1;
                            a.Nft.Reservedcount = 0;
                            a.Nft.Soldcount = 0;
                            a.Nft.Markedaserror = DateTime.Now;
                            a.Nft.Selldate = DateTime.Now;
                            a.Nft.Buildtransaction = logfile; // for easy check whats went wrong
                        }
                        else
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"NFT is already marked as sold. Leaving state {a.Nft.Name} - {a.Nft.Id}- {addr.Address}", logfile, serverid);
                        }
                    }
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                GlobalFunctions.ResetContextState(db);
                await StaticBackgroundServerClass.EventLogException(db, 137, e, serverid);
            }

            await db.Database.ExecuteSqlRawAsync(
                $" DELETE from nftreservations where reservationtoken='{addr.Reservationtoken}'");
            await db.Database.ExecuteSqlRawAsync($"delete from nfttonftaddresses where nftaddresses_id={addr.Id}");

            //   await GlobalFunctions.ReleaseNft(db, addr.Id);
        }

        private async Task SetPreparedPaymenttransactionAsync(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionsStates state, int id, string txhash, string buyeraddress, Nftaddress addr, long fee)
        {
            var prepared = await (from a in db.Preparedpaymenttransactions
                                  where a.NftaddressesId == id
                                  orderby a.Id descending
                                  select a).FirstOrDefaultAsync();
            if (prepared != null)
            {
                if (!string.IsNullOrEmpty(prepared.Reservationtoken) && (state == PaymentTransactionsStates.expired || state == PaymentTransactionsStates.error))
                    await NftReservationClass.ReleaseAllNftsAsync(db, redis, prepared.Reservationtoken);

                // Because of Concurrent problems, we have to do this also via SQL
                /*     string sql = $"update preparedpaymenttransactions set state='{state}', txhash='{txhash}',submitteddate=NOW(),buyeraddress='{buyeraddress}' where id=" + prepared.Id;
                     await db.Database.ExecuteSqlRawAsync(sql);*/


                prepared.State = state.ToString();
                prepared.Txhash = txhash;
                prepared.Submitteddate = DateTime.Now;
                prepared.Buyeraddress = buyeraddress;
                prepared.Fee = fee;
                prepared.Selleraddress = addr.Address;
                prepared.Buyerpkh = GlobalFunctions.GetPkhFromAddress(buyeraddress);
                prepared.Buyeraddresses = buyeraddress;
                prepared.Expires = addr.Expires;
                prepared.Discount = addr.Discount;
                prepared.Smartcontractstate = "submitted";
                prepared.Paymentgatewaystate = "submitted";
                await db.SaveChangesAsync();



                var referenced = await (from a in db.Preparedpaymenttransactions
                                        where a.ReferencedprepearedtransactionId == prepared.Id
                                        orderby a.Id descending
                                        select a).FirstOrDefaultAsync();
                if (referenced != null)
                {/*
                    sql =
                        $"update preparedpaymenttransactions set state='{state}', txhash='{txhash}',submitteddate=NOW(),buyeraddress='{buyeraddress}', fee='{fee}' where id=" +
                        referenced.Id;
                    await db.Database.ExecuteSqlRawAsync(sql);
                    */

                    referenced.State = state.ToString();
                    referenced.Txhash = txhash;
                    referenced.Submitteddate = DateTime.Now;
                    referenced.Buyeraddress = buyeraddress;
                    referenced.Fee = fee;
                    referenced.Selleraddress = addr.Address;
                    referenced.Buyerpkh = GlobalFunctions.GetPkhFromAddress(buyeraddress);
                    referenced.Buyeraddresses = buyeraddress;
                    referenced.Expires = addr.Expires;
                    referenced.Discount = addr.Discount;
                    referenced.Smartcontractstate = "submitted";
                    referenced.Paymentgatewaystate = "submitted";
                    await db.SaveChangesAsync();




                    if (state == PaymentTransactionsStates.finished)
                    {
                        foreach (var adrx in addr.Nfttonftaddresses)
                        {

                            await db.PreparedpaymenttransactionsNfts.AddAsync(new()
                            {
                                Count = adrx.Tokencount,
                                NftId = adrx.NftId,
                                PreparedpaymenttransactionsId = prepared.Id,
                                Lovelace = 0,
                                Policyid = addr.Nftproject.Policyid,
                                Tokenname = (addr.Nftproject.Tokennameprefix ?? "") + adrx.Nft.Name,
                                Tokennamehex =
                                    GlobalFunctions.ToHexString((addr.Nftproject.Tokennameprefix ?? "") +
                                                                adrx.Nft.Name),
                                Nftuid = adrx.Nft.Uid
                            });
                            await db.SaveChangesAsync();

                        }
                    }
                }



            }
        }



        private async Task RejectDueToConditonsNotMetFromNftAddressesNewAsync(EasynftprojectsContext db,
            CancellationToken cancellationToken,
            IConnectionMultiplexer redis, Nftaddress addr,
            string senderaddress, ulong lamports, CheckConditionsResultClass cond, string sendbackmessage, int serverid)
        {
            if (string.IsNullOrEmpty(senderaddress))
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Error while sending back - Can not find senderaddress - Set State to error - {addr.Address}",
                    null, serverid);
                addr.State = "error";
                await db.SaveChangesAsync();
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Because of error state - Releasing all reserved Nft to Free - {addr.Address}", "", serverid);
                await GlobalFunctions.ReleaseNftAsync(db, redis, addr.Id);
                return;
            }

            await StaticBackgroundServerClass.LogAsync(db,
                $"Sending back all ADA and Tokens from address {addr.Address} to address {addr.Senderaddress}", "",
                serverid);
            BuildTransactionClass buildtransaction = new BuildTransactionClass();


            // When pay with tokens
            Adminmintandsendaddress paywallet = null;
            // Minus 1 means, that the user can send everything to the address in lamports. Its for payments in tokens - we need to use the minting addresses also
            if (addr.Price == -1)
            {
                paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db, 0, "CheckApiAddressSolana",addr.Reservationtoken,Coin.SOL);
                if (paywallet == null)
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"CheckApiAddresses - All Paywallets are blocked - waiting: {addr.Address} - {addr.Nftproject.Projectname}",
                        "", serverid);
                    return;
                }
            }

            buildtransaction.MetadataStandard = "solana";
            var s = await SolanaFunctions.SendAllCoinsAndTokensFromNftaddress(addr,
                string.IsNullOrEmpty(addr.Refundreceiveraddress) ? senderaddress : addr.Refundreceiveraddress,
                buildtransaction, sendbackmessage);
            try
            {
                // Change to sql update because of concurrent db access errors
                await UpdateNftAddressesWithSqlCommand(db, cancellationToken, addr, cond.RejectParameter,
                    cond.RejectReason, "rejected");


                await db.SaveChangesAsync(cancellationToken);
                await GlobalFunctions.SaveRefundLogAsync(db, addr.Address, senderaddress, addr.Txid,
                    !string.IsNullOrEmpty(s.TxHash), s.TxHash,
                    cond.RejectReason, (int) addr.NftprojectId,
                    s.LogFile, (long) lamports, s.Fees, s.NmkrCosts, Coin.SOL);
                await UpdateAdminMintAndSendAddresses(db, serverid, paywallet, s);

                if (paywallet != null)
                    await GlobalFunctions.UnlockPaywalletAsync(db, paywallet);
            }
            catch (Exception e)
            {
                await GlobalFunctions.LogExceptionAsync(db, e.Message, e.InnerException?.Message, serverid);
            }



            await StaticBackgroundServerClass.LogAsync(db, $"Releasing all reserved Nft to Free - {addr.Address}", "",
                serverid);
            await GlobalFunctions.ReleaseNftAsync(db, redis, addr.Id);
        }

        private static async Task UpdateAdminMintAndSendAddresses(EasynftprojectsContext db, int serverid,
            Adminmintandsendaddress paywallet,  BuildTransactionClass buildtransaction)
        {
            if (paywallet != null)
            {
                if (!string.IsNullOrEmpty(buildtransaction.TxHash))
                    await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                        $"update adminmintandsendaddresses set addressblocked=1,blockcounter=0,lasttxhash='{buildtransaction.TxHash}', lasttxdate=NOW() where id='{paywallet.Id}'",
                        serverid);
                else
                    await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                        $"update adminmintandsendaddresses set addressblocked=0,blockcounter=0,lasttxhash='', lasttxdate=NOW() where id='{paywallet.Id}'",
                        serverid);
            }
        }


        private async Task SendTooLessOrTooMuchBackFromNftAddressesNewAsync(EasynftprojectsContext db, IConnectionMultiplexer redis, Nftaddress addr, string senderaddress,
            string sendbackmessage, int serverid, ulong lamports)
        {
            await StaticBackgroundServerClass.LogAsync(db,
                $"Sending back all SOL and Tokens from address {addr.Address} to address {senderaddress}", "", serverid);
            BuildTransactionClass buildtransaction = new BuildTransactionClass();

            var s = await SolanaFunctions.SendAllCoinsAndTokensFromNftaddress( addr, string.IsNullOrEmpty(addr.Refundreceiveraddress) ? senderaddress : addr.Refundreceiveraddress, buildtransaction, sendbackmessage);
            try
            {
                await db.SaveChangesAsync();
                await GlobalFunctions.SaveRefundLogAsync(db, addr.Address, senderaddress, "",
                    !string.IsNullOrEmpty(s.TxHash), buildtransaction.TxHash,
                    sendbackmessage, (int)addr.NftprojectId,
                    buildtransaction.LogFile, (long)lamports, buildtransaction.Fees, buildtransaction.NmkrCosts, Coin.SOL);
            }
            catch
            {
            }
        }

    }
}
