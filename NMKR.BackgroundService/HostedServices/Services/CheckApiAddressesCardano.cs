using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Cardano_Sharp;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.SaleConditions;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckApiAddressesCardano : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkpaymentaddresses;
            if (server.Checkpaymentaddresses == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);

            List<Nftaddress> addresses = new List<Nftaddress>();
            try
            {
                // First look for the addresses where the payment is received
                addresses = await (from a in db.Nftaddresses
                                   where (a.State == "payment_received") &&
                                         (a.Serverid == null || a.Serverid == serverid) && a.Coin== Coin.ADA.ToString()
                                   orderby a.Lastcheckforutxo
                                   select a).AsNoTracking().ToListAsync(cancellationToken: cancellationToken);


                // If no one is found, then look for the active addresses
                if (addresses.Count == 0)
                {
                    addresses = await (from a in db.Nftaddresses
                                       where (a.State == "active" || a.State == "payment_received") &&
                                             (a.Serverid == null || a.Serverid == serverid) && a.Coin == Coin.ADA.ToString()
                                       orderby a.Lastcheckforutxo
                                       select a).Take(150).AsNoTracking().ToListAsync(cancellationToken: cancellationToken);
                }
            }
            catch (Exception e)
            {
                await StaticBackgroundServerClass.EventLogException(db, 50, e, serverid);
                GlobalFunctions.ResetContextState(db);
            }
            var addresseslist=(from a in addresses
                               select a.Address).ToArray();

          /*  await StaticBackgroundServerClass.LogAsync(db,
                $"Checking Payment Addresses - {addresses.Count}",JsonConvert.SerializeObject(addresseslist), serverid);*/

            /*

            var maxsemaphorecount = await GlobalFunctions.GetWebsiteSettingsIntAsync(db, "maxsemaphorecount", 5);
            SemaphoreSlim semaphore = new(maxsemaphorecount);
            var tasks = addresses.Select((t, index) =>
            {
                semaphore.WaitAsync(cancellationToken);
                var tx = CheckAddress(cancellationToken, mainnet, serverid, redis, t.Id, addresses,
                    index + 1);
                semaphore.Release();
                return tx;
            }).ToList();
            await Task.WhenAll(tasks);
              */

            int index = 0;
            foreach (var nftaddress in addresses)
            {
                index++;
                await CheckAddress(db, cancellationToken, mainnet, serverid, redis, nftaddress, 
                    index + 1);
            }

          /*  await StaticBackgroundServerClass.LogAsync(db,
                $"Checking Payment Addresses finished - {addresses.Count}", JsonConvert.SerializeObject(addresseslist), serverid);*/

            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private async Task CheckAddress(EasynftprojectsContext db, CancellationToken cancellationToken, bool mainnet,
            int serverid, IConnectionMultiplexer redis, Nftaddress address, int c)
        {
            await StaticBackgroundServerClass.LogAsync(db,
                $"Check Address {address.Address}", "",serverid);
            var utxo = await ConsoleCommand.GetNewUtxoAsync(address.Address, Dataproviders.Default);
            if (utxo.LovelaceSummary == 0)
            {
                string sql =
                    $"update nftaddresses set lastcheckforutxo=now(), addresscheckedcounter=addresscheckedcounter+1, state='active' where id={address.Id}";
                    await db.Database.ExecuteSqlRawAsync(sql, cancellationToken: cancellationToken);
                return;
            }


            //   await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var addr = await (from a in db.Nftaddresses
                    .Include(a => a.Nfttonftaddresses)
                    .ThenInclude(a => a.Nft)
                    .ThenInclude(a => a.Instockpremintedaddress)
                    .AsSplitQuery()
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Customer)
                    .AsSplitQuery()
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Settings)
                    .AsSplitQuery()
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Customerwallet)
                    .AsSplitQuery()
                    .Include(a => a.Referer)
                    .AsSplitQuery()
                              where a.Id == address.Id
                              select a).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            DateTime started = DateTime.Now;

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
                BackgroundTaskEnums.checkpaymentaddresses, mainnet, serverid, redis, c);

            addr.Addresscheckedcounter++;
            addr.Lastcheckforutxo = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);

            BuildTransactionClass bt = new BuildTransactionClass();
            bt.LogFile += $"Check API/PGW Address{Environment.NewLine}";
            bt.LogFile += addr.Address + Environment.NewLine;


            
            string txhash = "";
            long lovelace = utxo.LovelaceSummary;

            await StaticBackgroundServerClass.LogAsync(db,
                $"Checking Payment Address - {addr.Address} Lovelace: {lovelace} - Price: {addr.Price} - Expires: {addr.Expires} - State: {addr.State} - Dataprovider: {utxo.DataProvider}",
                JsonConvert.SerializeObject(utxo), serverid);

            // Then get sender and Txid
            if (lovelace > 0)
            {
                txhash = utxo.GetFirstTxHash();
                addr.Txid = string.IsNullOrEmpty(txhash)
                    ? await ConsoleCommand.GetTransactionIdAsync(addr.Address)
                    : txhash;


                if (addr.Foundinslot == null || addr.Foundinslot == 0)
                    addr.Foundinslot = await ConsoleCommand.GetSlotAsync();


                if (string.IsNullOrEmpty(addr.Txid))
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"TxHash is null - {addr.Address} Lovelace: {lovelace} - Price: {addr.Price} - Expires: {addr.Expires} - State: {addr.State}",
                        JsonConvert.SerializeObject(utxo), serverid);
                    return;
                }


                lovelace = utxo.GetLovelace(txhash);
                addr.Lovelace = lovelace;
                addr.Senderaddress = await ConsoleCommand.GetSenderAsync(addr.Txid);
                await db.SaveChangesAsync(cancellationToken);

                if (string.IsNullOrEmpty(addr.Senderaddress))
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"GetSender is null - {addr.Address} - TxId: {addr.Txid} Lovelace: {lovelace} - Price: {addr.Price} - Expires: {addr.Expires} - State: {addr.State}",
                        JsonConvert.SerializeObject(utxo), serverid);
                    return;
                }
            }
            else
            {
                if (addr.State == "payment_received")
                {
                    addr.State = "active";
                    await db.SaveChangesAsync(cancellationToken);
                }
                return;
            }


            await StaticBackgroundServerClass.LogAsync(db,
                $"Processing Payment Address - Lovelace: {lovelace} - Price: {addr.Price} - {addr.Address} - Expires: {addr.Expires} - Project:{addr.Nftproject.Projectname} {addr.NftprojectId} - Customer: {addr.Nftproject.Customer.Firstname} {addr.Nftproject.Customer.Lastname} Company: {addr.Nftproject.Customer.Company}" ??
                "", JsonConvert.SerializeObject(utxo), serverid);


            // Check if there were some tokens sended with the amount and reject it
            if (utxo.TxIn.First().Tokens != null && utxo.TxIn.First().Tokens.Any())
            {
                if (!await CheckForTokensAsync(db, cancellationToken, mainnet, serverid, redis, addr, txhash,
                        utxo, lovelace))
                {
                    return;
                }
                if (!await CheckForCorrectTokensAsync(db, cancellationToken, mainnet, serverid, redis, utxo, addr,
                        txhash, lovelace))
                {
                    return;
                }
            }
            else
            {
                if (!await CheckIfTokensArenecessaryAsync(db, cancellationToken, mainnet, serverid, redis, utxo,
                        addr, txhash, lovelace))
                {
                    return;
                }
            }

            if (!await CheckForCorrectAmountOfTokensAsync(db, cancellationToken, mainnet, serverid, redis, utxo,
                    addr, txhash, lovelace))
            {
                return;
            }

            if (!await CheckForCorrectAmountOfLovelaceAsync(db, cancellationToken, mainnet, serverid, redis,
                    lovelace, addr, txhash, utxo))
            {
                return;
            }

          /*  if (!await CheckIfAlreadyExpired(db, cancellationToken, mainnet, serverid, redis, utxo,
                    addr, txhash, lovelace))
            {
                return;
            }*/

            await StaticBackgroundServerClass.LogAsync(db,
                $"Processing Payment Address (2) - Lovelace: {lovelace} - Price: {addr.Price} - {addr.Address} - Expires: {addr.Expires} - Project:{addr.Nftproject.Projectname} {addr.NftprojectId} - Customer: {addr.Nftproject.Customer.Firstname} {addr.Nftproject.Customer.Lastname} Company: {addr.Nftproject.Customer.Company}" ??
                "", JsonConvert.SerializeObject(utxo), serverid);


            // Check for Saleconditions. If not met, reject it
            var cond = await CheckSalesConditionClass.CheckForSaleConditionsMet(db, redis, addr.NftprojectId ?? 0,
                addr.Senderaddress,
                Math.Max(1, addr.Nfttonftaddresses.Sum(x => x.Tokencount)) *
                Math.Max(1, addr.Nftproject.Multiplier), serverid, addr.Nftproject.Usefrankenprotection, Blockchain.Cardano);
            if (!await CheckForSaleConditions(db,cancellationToken, mainnet, serverid, redis, cond, addr, lovelace, txhash,
                    utxo))
                return;

            await StaticBackgroundServerClass.LogAsync(db,
                $"Processing Payment Address (3) - Lovelace: {lovelace} - Price: {addr.Price} - {addr.Address} - Expires: {addr.Expires} - Project:{addr.Nftproject.Projectname} {addr.NftprojectId} - Customer: {addr.Nftproject.Customer.Firstname} {addr.Nftproject.Customer.Lastname} Company: {addr.Nftproject.Customer.Company}" ??
                "", JsonConvert.SerializeObject(utxo), serverid);


            // If there are additional Payout wallets - check if the rest amount is enough - if not, reject it

            if (addr.Price > 0)
            {
                long restprice = await StaticBackgroundServerClass.CheckRestPrice(db, addr.Nftproject,
                    addr.Nfttonftaddresses.Count, lovelace, addr.Senderaddress, addr.Reservationtoken, Coin.ADA, addr.Customproperty);
                if (!await CheckRestPriceEnough(db,cancellationToken, mainnet, serverid, redis, restprice, addr, lovelace, txhash,
                        utxo))
                {
                    return;
                }
            }

            if (await CheckForAlreadyMintedOnBlockfrost(db, cancellationToken, mainnet, serverid, redis, addr, txhash, utxo, lovelace)) return;

            await StaticBackgroundServerClass.LogAsync(db,
                $"Processing Payment Address (4) - Lovelace: {lovelace} - Price: {addr.Price} - {addr.Address} - Expires: {addr.Expires} - Project:{addr.Nftproject.Projectname} {addr.NftprojectId} - Customer: {addr.Nftproject.Customer.Firstname} {addr.Nftproject.Customer.Lastname} Company: {addr.Nftproject.Customer.Company}" ??
                "", JsonConvert.SerializeObject(utxo), serverid);

            // Catch Additional Payout Wallets from the Database
            var additionalPayouts = await (from a in db.Nftprojectsadditionalpayouts
                    .Include(a => a.Wallet)
                    .AsSplitQuery()
                                           where a.NftprojectId == addr.NftprojectId && 
                                                 a.Coin==Coin.ADA.ToString() && 
                                                 (a.Custompropertycondition==null || a.Custompropertycondition=="" || a.Custompropertycondition == addr.Customproperty)
                                           select a).AsNoTracking().ToArrayAsync(cancellationToken: cancellationToken);


            Pricelistdiscount? discount = null;
            long stakerewards = 0;
            long tokenrewards = 0;
            try
            {
                discount = await PriceListDiscountClass.GetPricelistDiscount(db, redis, addr.NftprojectId ?? 0,
                    addr.Senderaddress,addr.Refererstring,addr.Customproperty, serverid, Blockchain.Cardano);
                var rewards= await RewardsClass.GetTokenAndStakeRewards(db, redis, addr.Senderaddress);
                stakerewards = rewards.StakeReward;
                tokenrewards = rewards.TokenReward;
            }
            catch (Exception e)
            {
                bt.LogFile +="Exception 88: "+ e.Message + Environment.NewLine;
                await StaticBackgroundServerClass.EventLogException(db, 88, e, serverid);
            }



            PromotionClass promotion = null;
            if (addr.PromotionId != null)
                promotion = await GlobalFunctions.GetPromotionAsync(db, redis, (int)addr.PromotionId,
                    addr.Promotionmultiplier ?? 1);

            await StaticBackgroundServerClass.LogAsync(db,
                $"Processing Payment Address (5) - Lovelace: {lovelace} - Price: {addr.Price} - {addr.Address} - Expires: {addr.Expires} - Project:{addr.Nftproject.Projectname} {addr.NftprojectId} - Customer: {addr.Nftproject.Customer.Firstname} {addr.Nftproject.Customer.Lastname} Company: {addr.Nftproject.Customer.Company}" ??
                "", JsonConvert.SerializeObject(utxo), serverid);

            var usedid = await WhitelistFunctions.SaveUsedAddressesToWhitelistSaleCondition(db,
                addr.NftprojectId ?? 0,
                addr.Senderaddress, cond.SendBackAddress?.OriginatorAddress, cond.SendBackAddress?.StakeAddress,
                "",
                Math.Max(1, addr.Nfttonftaddresses.Sum(x => x.Tokencount)) *
                Math.Max(1, addr.Nftproject.Multiplier));


            // Check for Token Only Transactions

            Adminmintandsendaddress paywallet = null;
            // Minus 1 means, that the user can send everything to the address in lovelace. Its for payments in tokens - we need to use the minting addresses also
            if (addr.Price == -1)
            {
                paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db,serverid,"CheckApiAddresses", addr.Reservationtoken);
                if (paywallet == null)
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"CheckApiAddresses - All Paywallets are blocked - waiting: {addr.Address} - {addr.Nftproject.Projectname}",
                        JsonConvert.SerializeObject(utxo), serverid);
                    return;
                }
            }

            //
            // Make the Transaction
            //

            await StaticBackgroundServerClass.LogAsync(db,
                $"Send NFTs/Tokens for Address {addr.Address} - Project: {addr.Nftproject.Projectname}",
                JsonConvert.SerializeObject(utxo),
                serverid);
            bool b = MintAndSendMultipeTokens(db, addr, !string.IsNullOrEmpty(addr.Optionalreceiveraddress)
                    ? addr.Optionalreceiveraddress
                    : string.IsNullOrEmpty(cond.SendBackAddress?.Address)
                        ? addr.Senderaddress
                        : cond.SendBackAddress.Address, additionalPayouts, discount?.Sendbackdiscount ?? 0f, stakerewards, tokenrewards,
                utxo.TxIn.FirstOrDefault(x => x.TxHashId == txhash), serverid, mainnet, redis, promotion, paywallet, ref bt);

            if (string.IsNullOrEmpty(bt.TxHash))
                b = false;
            else
            {
                await StaticBackgroundServerClass.LogAsync(db, $"Transaction-ID: {bt.TxHash}",
                    "",
                    serverid);

                if (promotion != null)
                {
                    await GlobalFunctions.SetPromotionSoldcountAsync(db, (int)addr.PromotionId,
                        promotion.Tokencount);
                }
            }

            await StaticBackgroundServerClass.LogAsync(db,
                $"Transaction {addr.Address}{(b ? " - successful" : " - failed")}",
                bt.LogFile, serverid);
            addr.Submissionresult = bt.SubmissionResult;


            if (!b)
            {
                // if there was an error with paywithtokensonly - try again
                /*if (addr.Price == -1)
                {
                    return;
                }*/

                await SetAddressToError(db, cancellationToken, mainnet, serverid, redis, addr, bt, txhash, utxo, lovelace, usedid);
            }
            else
            {
                await SetAddressToSuccess(db, cancellationToken, mainnet, serverid, redis, addr, bt, stakerewards, tokenrewards, started, cond, promotion, usedid, paywallet);

                await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(
                    BackgroundTaskEnums.checkpaymentaddresses, mainnet, serverid, redis);
            }

        }

        private async Task<bool> CheckIfAlreadyExpired(EasynftprojectsContext db, CancellationToken cancellationToken, bool mainnet, int serverid, IConnectionMultiplexer redis, TxInAddressesClass utxo, Nftaddress addr, string txhash, long lovelace)
        {
            // Check if the amount was correct. If not reject it
            if (addr.Expires > DateTime.Now)
                return true;

            // Only send back, when it is more than 1.5 ADA
          //  if (lovelace > 1500000)
            {
                await SendTooLessOrTooMuchBackFromNftAddressesNewAsync(db, redis, addr, addr.Senderaddress, txhash,
                    "Reservation already expired", utxo, serverid, mainnet, lovelace);
            }

            string rejectparameter =
                $"Reserved until {addr.Expires} - Actual Date/Time {DateTime.Now}";
            await UpdateNftAddressesWithSqlCommand(db, cancellationToken, addr, rejectparameter, "expired", "rejected");


            await GlobalFunctions.ReleaseNftAsync(db, redis, addr.Id);

            return false;
        }

        private async Task<bool> CheckIfTokensArenecessaryAsync(EasynftprojectsContext db, CancellationToken cancellationToken, bool mainnet, int serverid, IConnectionMultiplexer redis, TxInAddressesClass utxo, Nftaddress addr, string txhash, long lovelace)
        {
                if (addr.Priceintoken == null || string.IsNullOrEmpty(addr.Tokenpolicyid))
                    return true;

                await SendbackAndReleaseAsync(db, cancellationToken, mainnet, serverid, redis, utxo, addr, txhash, lovelace, "Send the required tokens with your transaction");
                return false;
        }

        private async Task SendbackAndReleaseAsync(EasynftprojectsContext db, CancellationToken cancellationToken, bool mainnet,
            int serverid, IConnectionMultiplexer redis, TxInAddressesClass utxo, Nftaddress addr, string txhash, long lovelace, string message)
        {
            await SendTooLessOrTooMuchBackFromNftAddressesNewAsync(db, redis, addr, addr.Senderaddress, txhash,
                message, utxo, serverid, mainnet, lovelace);

            addr.State = "rejected";
            addr.Paydate = DateTime.Now;
            addr.Lastcheckforutxo = DateTime.Now;

            addr.Rejectreason = "amountwaswrong";
            addr.Rejectparameter = message;
            await db.SaveChangesAsync(cancellationToken);

            await StaticBackgroundServerClass.LogAsync(db,
                $"Tokens in Transaction: {addr.Address} - Project-Id:{addr.NftprojectId} - {addr.Senderaddress} - {lovelace}",
                message, serverid);

            await GlobalFunctions.ReleaseNftAsync(db, redis, addr.Id);
        }

        private async Task SetAddressToSuccess(EasynftprojectsContext db, CancellationToken cancellationToken, bool mainnet,
            int serverid, IConnectionMultiplexer redis,  Nftaddress addr, BuildTransactionClass bt,
            long stakerewards, long tokenrewards, DateTime started, CheckConditionsResultClass cond, PromotionClass promotion,
            int?[] usedid, Adminmintandsendaddress paywallet)
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

            // Determinate if we have to block the customer wallet - only when the customer wallet is used for receiving the ada
            bool block = !(addr.Nftproject.CustomerwalletId != null &&
                           addr.Nftproject.Customerwallet.State == "active");

            await MarkAsSoldAsync(db, redis, addr,
                !string.IsNullOrEmpty(addr.Optionalreceiveraddress) ? addr.Optionalreceiveraddress : addr.Senderaddress,
                bt.Command, block, serverid, mainnet);

            if (addr.Nftproject.Maxsupply == 1)
                StaticBackgroundServerClass.SaveMarkAsSoldToRedis(addr.Nfttonftaddresses, mainnet);

            if (addr.Price == -1 && paywallet != null)
            {
                await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, $"update adminmintandsendaddresses set addressblocked=1,blockcounter=0,lasttxhash='{bt.TxHash}', lasttxdate=NOW() where id='{paywallet.Id}'", serverid);
            }


            TimeSpan ts = DateTime.Now - started;
            await StaticBackgroundServerClass.SaveTransactionToDatabase(db, redis, bt,
                addr.Nftproject.CustomerId,
                addr.Id, addr.NftprojectId, nameof(TransactionTypes.paidonftaddress),
                addr.Nftproject.CustomerwalletId, serverid, Coin.ADA, (long) ts.TotalMilliseconds,
                addr, cond, promotion, bt.SignedTransaction, addr.Ipaddress, customproperty: addr.Customproperty,
                discountrefererstring: addr.Refererstring);

            await WhitelistFunctions.UpdateUsedAddressesToWhitelistSaleCondition(db, usedid, bt.TxHash);
        }

        private async Task SetAddressToError(EasynftprojectsContext db, CancellationToken cancellationToken, bool mainnet,
            int serverid, IConnectionMultiplexer redis, Nftaddress addr, BuildTransactionClass bt, string txhash,
            TxInAddressesClass utxo, long lovelace, int?[] usedid)
        {
            await StaticBackgroundServerClass.LogAsync(db,
                $"Marking Transaction as Error because of Sale Problems (Api Addresses) {addr.Address} - Project: {addr.NftprojectId} {addr.Nftproject.Projectname}",
                "", serverid);
            addr.State = "error";
            addr.Submissionresult = bt.LogFile;
            await db.SaveChangesAsync(cancellationToken);

            await StaticBackgroundServerClass.LogAsync(db,
                $"Set PrepardPaymentTransaction To Error (Api Addresses) {addr.Address} - Project: {addr.NftprojectId} {addr.Nftproject.Projectname}",
                "", serverid);
            await SetPreparedPaymenttransactionAsync(db, redis, PaymentTransactionsStates.error,
                addr.Id, "", string.IsNullOrEmpty(addr.Optionalreceiveraddress) ? addr.Optionalreceiveraddress : addr.Senderaddress, addr,0);
           
            await StaticBackgroundServerClass.LogAsync(db,
                $"Marking NFT as Error because of Sale Problems (Api Addresses) {addr.Address} - Project: {addr.NftprojectId} {addr.Nftproject.Projectname}",
                bt.LogFile, serverid);
            await MarkAsErrorAsync(db, addr, bt.LogFile, serverid);

            await SendTooLessOrTooMuchBackFromNftAddressesNewAsync(db, redis, addr, addr.Senderaddress, txhash,
                "Error while minting NFT. Please try again", utxo, serverid, mainnet, lovelace);
            await WhitelistFunctions.DeleteUsedAddressesToWhitelistSaleCondition(db, usedid);
        }

        private async Task<bool> CheckForSaleConditions(EasynftprojectsContext db, CancellationToken cancellationToken, bool mainnet,
            int serverid, IConnectionMultiplexer redis, CheckConditionsResultClass cond, Nftaddress addr, long lovelace,
            string txhash, TxInAddressesClass utxo)
        {
            if (cond.ConditionsMet != false)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Conditions met - {addr.Address} - {lovelace} - Price: {addr.Price}Project-Id:{addr.NftprojectId}",
                    JsonConvert.SerializeObject(cond), serverid);
                return true;
            }
            await StaticBackgroundServerClass.LogAsync(db,
                $"Conditions not met - {addr.Address} - Sending back Adaamount: {lovelace} - Price: {addr.Price} Project-Id:{addr.NftprojectId}",
                JsonConvert.SerializeObject(cond), serverid);
            await RejectDueToConditonsNotMetFromNftAddressesNewAsync(db, cancellationToken, redis, addr, addr.Senderaddress,
                lovelace, cond, "Payment conditions not met. Contact the seller.", txhash, utxo, serverid,
                mainnet);
            return false;

        }

        private async Task<bool> CheckForAlreadyMintedOnBlockfrost(EasynftprojectsContext db,
            CancellationToken cancellationToken, bool mainnet, int serverid, IConnectionMultiplexer redis, Nftaddress addr,
            string txhash, TxInAddressesClass utxo, long lovelace)
        {
            // Last Bastion - check on Blockfrost - if there are some nfts already sold - reject it and set the nfts to sold - to prevent double mints

            bool failed = false;
            string resultjsonString = "";
            // Check on blockfrost if the NFT are really really really free
            foreach (var adrx in addr.Nfttonftaddresses)
            {
                if (StaticBackgroundServerClass.FoundNftInBlockchain(adrx.Nft, addr.Nftproject, mainnet,
                        out var bfq, out var resultjson ))
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
                    addr.Id,"", string.IsNullOrEmpty(addr.Optionalreceiveraddress) ? addr.Optionalreceiveraddress : addr.Senderaddress,addr,0);

                await SendTooLessOrTooMuchBackFromNftAddressesNewAsync(db, redis, addr, addr.Senderaddress, txhash,
                    "Error while preparing NFT. Please try again", utxo, serverid, mainnet, lovelace);
                return true;
            }

            return false;
        }

        private async Task<bool> CheckRestPriceEnough(EasynftprojectsContext db,CancellationToken cancellationToken, bool mainnet, int serverid,
            IConnectionMultiplexer redis, long restprice, Nftaddress addr, long lovelace, string txhash,
            TxInAddressesClass utxo)
        {
            if (restprice >= 1000000 || addr.Lovelace == 2000000 || addr.Lovelace == 0) return true;
            await StaticBackgroundServerClass.LogAsync(db,
                $"Restprice is too low: {restprice} - {addr.Address} - Project-Id:{addr.NftprojectId} - {addr.Senderaddress}",
                "", serverid);
            await RejectDueToConditonsNotMetFromNftAddressesNewAsync(db,cancellationToken, redis, addr, addr.Senderaddress,
                lovelace,
                new CheckConditionsResultClass()
                {
                    ConditionsMet = false,
                    RejectParameter = "Error in sellers project configuration",
                    RejectReason = "amountwaswrong"
                }, "Error in sellers project configuration", txhash, utxo, serverid, mainnet);
            return false;
        }

        private async Task<bool> CheckForCorrectAmountOfLovelaceAsync(EasynftprojectsContext db,
            CancellationToken cancellationToken, bool mainnet, int serverid, IConnectionMultiplexer redis, long lovelace,
            Nftaddress addr, string txhash, TxInAddressesClass utxo)
        {
            // Check if the amount was correct. If not reject it
            if (addr.Price < 0 || lovelace == addr.Price) return true;

            // For Dexhunter, we have to check if the amount is more than the price
            if (lovelace > addr.Price && addr.Lovelaceamountmustbeexact == false)
                return true;


            // Only send back, when it is more than 1.5 ADA
         //   if (lovelace > 1500000)
            {
                await SendTooLessOrTooMuchBackFromNftAddressesNewAsync(db, redis, addr, addr.Senderaddress, txhash,
                    lovelace > addr.Price
                        ? "Amount was too much - rejected"
                        : "Not enough ADA/Tokens sent - rejected", utxo, serverid, mainnet, lovelace);
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

        private async Task<bool> CheckForCorrectAmountOfTokensAsync(EasynftprojectsContext db,
            CancellationToken cancellationToken, bool mainnet, int serverid, IConnectionMultiplexer redis,
            TxInAddressesClass utxo, Nftaddress addr, string txhash, long lovelace)
        {
            if (utxo.TxIn.First().Tokens == null || utxo.TxIn.First().Tokens.Count <= 1) return true;

            await SendbackAndReleaseAsync(db, cancellationToken, mainnet, serverid, redis, utxo, addr, txhash, lovelace,
                "Send only the required tokens with your transaction");
         
            return false;
        }

        private async Task<bool> CheckForCorrectTokensAsync(EasynftprojectsContext db,
            CancellationToken cancellationToken, bool mainnet, int serverid, IConnectionMultiplexer redis,
            TxInAddressesClass utxo, Nftaddress addr, string txhash, long lovelace)
        {
            string tokenassetidNftCip68 = "";
            string tokenassetidFtCip68 = "";
            if (!string.IsNullOrEmpty(addr.Tokenassetid))
            {
                tokenassetidNftCip68 =
                    ConsoleCommand.CreateMintTokenname("", addr.Tokenassetid, ConsoleCommand.Cip68Type.NftUserToken).ToLower();
                tokenassetidFtCip68 =
                    ConsoleCommand.CreateMintTokenname("", addr.Tokenassetid, ConsoleCommand.Cip68Type.FtUserToken).ToLower();
            }

            if (utxo.TxIn.First().Tokens.First().PolicyId == addr.Tokenpolicyid &&
                utxo.TxIn.First().Tokens.First().Quantity == (addr.Priceintoken * addr.Tokenmultiplier) &&
                (utxo.TxIn.First().Tokens.First().TokennameHex.ToLower() == addr.Tokenassetid.ToHex() ||
                 utxo.TxIn.First().Tokens.First().TokennameHex.ToLower() == tokenassetidNftCip68 ||
                 utxo.TxIn.First().Tokens.First().TokennameHex.ToLower() == tokenassetidFtCip68 ||
                 string.IsNullOrEmpty(addr.Tokenassetid))) return true;

            await SendTooLessOrTooMuchBackFromNftAddressesNewAsync(db, redis, addr, addr.Senderaddress, txhash,
                "Wrong amount or wrong policy/assetid of token - rejected", utxo, serverid, mainnet, lovelace);


            // Change to sql command to avoid concurrent access
            string rejectparameter = "Tokens in transaction";
            await UpdateNftAddressesWithSqlCommand(db, cancellationToken, addr,rejectparameter,"amountwaswrong","rejected");

            string log = utxo.TxIn.First().Tokens.First().PolicyId + " - " + addr.Tokenpolicyid +
                         Environment.NewLine +
                         "Quantity: "+ utxo.TxIn.First().Tokens.First().Quantity + " - Needed: " +
                         (addr.Priceintoken*addr.Tokenmultiplier).ToString() + Environment.NewLine +
                         utxo.TxIn.First().Tokens.First().TokennameHex + " - " +
                         "tokenassetidNftCip68:" + tokenassetidNftCip68  + Environment.NewLine+
                         "tokenassetidFtCip68:" + tokenassetidFtCip68 +  Environment.NewLine+
                         "multiplier: " + addr.Tokenmultiplier+  Environment.NewLine +
                         addr.Tokenassetid + " - " +
                         GlobalFunctions.ToHexString(addr.Tokenassetid);
            await StaticBackgroundServerClass.LogAsync(db,
                $"Wrong Tokens in Transaction: {addr.Address} - Project-Id:{addr.NftprojectId} - {addr.Senderaddress} - {lovelace}",
                log, serverid);

            await GlobalFunctions.ReleaseNftAsync(db, redis, addr.Id);
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

        private async Task<bool> CheckForTokensAsync(EasynftprojectsContext db,
            CancellationToken cancellationToken, bool mainnet, int serverid, IConnectionMultiplexer redis, Nftaddress addr,
            string txhash, TxInAddressesClass utxo, long lovelace)
        {
            if (addr.Priceintoken != null && addr.Priceintoken != 0) return true;

            await SendbackAndReleaseAsync(db, cancellationToken, mainnet, serverid, redis, utxo, addr, txhash, lovelace, "Transaction contained tokens - rejected");
            return false;

        }

        private async Task MarkAsSoldAsync(EasynftprojectsContext db, IConnectionMultiplexer redis, Nftaddress addr, string senderaddress,
          string buildtransaction, bool blockcustomeraddress, int serverid, bool mainnet)
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
                    await NftReservationClass.MarkAllNftsAsSold(db, ad.Reservationtoken, addr.Nftproject.Cip68, senderaddress);
                }
                catch (Exception e)
                {
                    await StaticBackgroundServerClass.EventLogException(db, 441, e, serverid);
                    await Task.Delay(1000);
                    GlobalFunctions.ResetContextState(db);

                    // Sometime this does not work
                    await using EasynftprojectsContext db2 =
                        new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
                    await GlobalFunctions.LogExceptionAsync(db, "Try to lock again: " + ad.Reservationtoken, e.Message,
                        serverid);
                    await NftReservationClass.MarkAllNftsAsSold(db2, ad.Reservationtoken, addr.Nftproject.Cip68, senderaddress);
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
                        nftx2.Buildtransaction = buildtransaction;
                        nftx2.Soldby = "normal";
                        var id = nftx2.InstockpremintedaddressId;
                        nftx2.InstockpremintedaddressId = null;
                        nftx2.Instockpremintedaddress = null;
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



                ad.Nftproject.Customer.Addressblocked = blockcustomeraddress;

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
                await StaticBackgroundServerClass.EventLogException(db, 13, e, serverid);
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
                prepared.Buyeraddresses= buyeraddress;
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
                    referenced.Paymentgatewaystate= "submitted";
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



        private async Task RejectDueToConditonsNotMetFromNftAddressesNewAsync(EasynftprojectsContext db, CancellationToken cancellationToken,
            IConnectionMultiplexer redis, Nftaddress addr,
            string senderaddress, long lovelace, CheckConditionsResultClass cond, string sendbackmessage, string txhash,
            TxInAddressesClass utxo, int serverid, bool mainnet)
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

          //  if (lovelace > 1500000 || addr.Price==-1)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Sending back all ADA and Tokens from address {addr.Address} to address {addr.Senderaddress}", "",
                    serverid);
                BuildTransactionClass buildtransaction = new BuildTransactionClass();

                string password = addr.Nftproject?.Password;
                if (!string.IsNullOrEmpty(addr.Salt))
                {
                    password = addr.Salt + GeneralConfigurationClass.Masterpassword;
                }

                // When pay with tokens
                Adminmintandsendaddress paywallet = null;
                // Minus 1 means, that the user can send everything to the address in lovelace. Its for payments in tokens - we need to use the minting addresses also
               // if (addr.Price == -1)
                {
                    paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db,0,"CheckApiAddresses2", addr.Reservationtoken);
                    if (paywallet == null)
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"CheckApiAddresses - All Paywallets are blocked - waiting: {addr.Address} - {addr.Nftproject.Projectname}",
                            JsonConvert.SerializeObject(utxo), serverid);
                        return;
                    }
                }
                var s = CardanoSharpFunctions.SendAllAdaAndTokens(db, redis, addr.Address, addr.Privateskey,
                    addr.Privatevkey, password, string.IsNullOrEmpty(addr.Refundreceiveraddress) ? senderaddress : addr.Refundreceiveraddress, mainnet,
                    ref buildtransaction, txhash, 0, 0, sendbackmessage, utxo, paywallet);
                try
                {
                    // Change to sql update because of concurrent db access errors
                    await UpdateNftAddressesWithSqlCommand(db, cancellationToken, addr, cond.RejectParameter, cond.RejectReason, "rejected");


                    await db.SaveChangesAsync(cancellationToken);
                    await GlobalFunctions.SaveRefundLogAsync(db, addr.Address, senderaddress, addr.Txid,
                        s == "OK", buildtransaction.TxHash,
                        cond.RejectReason, (int)addr.NftprojectId,
                        buildtransaction.LogFile, lovelace, buildtransaction.Fees, buildtransaction.NmkrCosts, Coin.ADA);
                    await UpdateAdminMintAndSendAddresses(db, serverid, paywallet, s, buildtransaction);
                }
                catch (Exception e)
                {
                    await GlobalFunctions.LogExceptionAsync(db, e.Message, e.InnerException?.Message, serverid);
                }

            }
           

            await StaticBackgroundServerClass.LogAsync(db, $"Releasing all reserved Nft to Free - {addr.Address}", "",
                serverid);
            await GlobalFunctions.ReleaseNftAsync(db, redis, addr.Id);
        }

        private static async Task UpdateAdminMintAndSendAddresses(EasynftprojectsContext db, int serverid,
            Adminmintandsendaddress paywallet, string s, BuildTransactionClass buildtransaction)
        {
            if (paywallet != null)
            {
                if (s == "OK")
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
            string txhash, string sendbackmessage, TxInAddressesClass utxo, int serverid, bool mainnet, long lovelace)
        {
            await StaticBackgroundServerClass.LogAsync(db,
                $"Sending back all ADA and Tokens from address {addr.Address} to address {senderaddress}", "", serverid);
            BuildTransactionClass buildtransaction = new BuildTransactionClass();


            // When pay with tokens
            Adminmintandsendaddress paywallet = null;
            // Minus 1 means, that the user can send everything to the address in lovelace. Its for payments in tokens - we need to use the minting addresses also
           // if (addr.Price == -1)
            {
                paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db,0,"CheckApiAddresses3", addr.Reservationtoken);
                if (paywallet == null)
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"CheckApiAddresses - All Paywallets are blocked - waiting: {addr.Address} - {addr.Nftproject.Projectname}",
                        JsonConvert.SerializeObject(utxo), serverid);
                    return;
                }
            }


            string password = addr.Nftproject?.Password;
            if (!string.IsNullOrEmpty(addr.Salt))
            {
                password = addr.Salt + GeneralConfigurationClass.Masterpassword;
            }

            var s = CardanoSharpFunctions.SendAllAdaAndTokens(db, redis, addr.Address, addr.Privateskey, addr.Privatevkey, password, 
                string.IsNullOrEmpty(addr.Refundreceiveraddress) ? senderaddress : addr.Refundreceiveraddress, mainnet, ref buildtransaction, txhash, 0, 0, 
                sendbackmessage, utxo, paywallet);
            try
            {
                await db.SaveChangesAsync();
                await GlobalFunctions.SaveRefundLogAsync(db, addr.Address, senderaddress, txhash,
                    s == "OK", buildtransaction.TxHash,
                    sendbackmessage, (int)addr.NftprojectId,
                    buildtransaction.LogFile, lovelace, buildtransaction.Fees, buildtransaction.NmkrCosts, Coin.ADA);
                await UpdateAdminMintAndSendAddresses(db, serverid, paywallet, s, buildtransaction);
            }
            catch
            {
            }
        }

        private bool MintAndSendMultipeTokens(EasynftprojectsContext db, Nftaddress addr, string receiveraddress, Nftprojectsadditionalpayout[] additionalpayoutWallets, float discount, long stakerewards, long tokenrewards, TxInClass txin, int serverid, bool mainnet, IConnectionMultiplexer redis, PromotionClass promotion, Adminmintandsendaddress paywallet, ref BuildTransactionClass buildtransaction)
        {
            try
            {
                List<MultipleTokensClass> nfts = new List<MultipleTokensClass>();
                foreach (var n in addr.Nfttonftaddresses)
                {
                    nfts.Add(new MultipleTokensClass { nft = n.Nft, tokencount = n.Tokencount, Multiplier = n.Nft.Multiplier });
                }

                if (!nfts.Any())
                {
                    buildtransaction.LogFile += "No NFTS found" + Environment.NewLine;
                    return false;
                }
                    


                // Normally always the customer wallet address and not the internal one - because then it goes into mint coupons
                var restofadaaddress = addr.Nftproject.Customer.Adaaddress;
                if (addr.Nftproject.CustomerwalletId != null)
                {
                    if (addr.Nftproject.Customerwallet.State == "active")
                    {
                        restofadaaddress = addr.Nftproject.Customerwallet.Walletaddress;
                    }
                }

                if (addr.Nftproject.Cip68)
                {
                    buildtransaction.LogFile += "Minting Cip68" + Environment.NewLine;
                    buildtransaction.MetadataStandard = "cip68";
                    var res= ConsoleCommand.MintAndSendMultipleTokensFromApiCip68(redis, nfts.ToArray(), addr, receiveraddress,
                                                                          restofadaaddress, additionalpayoutWallets, discount, stakerewards, tokenrewards, mainnet, txin, promotion, addr.Nftproject, paywallet, ref buildtransaction);
                    if (res == "OK")
                    {
                        buildtransaction.LogFile += "Minting Cip68 OK" + Environment.NewLine;
                        return true;
                    }
                    buildtransaction.LogFile += "Minting Cip68 failed " + res + Environment.NewLine;
                }
                else
                {
                    buildtransaction.MetadataStandard = "cip25";
                    buildtransaction.LogFile += "Minting Cip25" + Environment.NewLine;
                    var res= ConsoleCommand.MintAndSendMultipleTokensFromApi(db, redis, nfts.ToArray(), addr, receiveraddress,
                                                   restofadaaddress, additionalpayoutWallets, discount, stakerewards, tokenrewards, mainnet, txin, promotion, paywallet, ref buildtransaction);
                    if (res == "OK")
                    {
                        buildtransaction.LogFile += "Minting Cip25 OK" + Environment.NewLine;
                        return true;
                    }

                    buildtransaction.LogFile += "Minting Cip25 failed " + res + Environment.NewLine;
                }
            }
            catch (Exception e)
            {
                buildtransaction.LogFile += e.Message + "\n" + e.StackTrace + "\n";
                buildtransaction.LogFile += e.InnerException?.Message + Environment.NewLine;
                GlobalFunctions.LogException(db, "Exception while minting - " + e.Message, e.StackTrace ?? "",
                   serverid);
                GlobalFunctions.ResetContextState(db);
            }

            return false;
        }

    }
}
