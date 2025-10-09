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
    public class CheckPayinAddresses : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkpayinaddresses;
            if (server.Checkprojectaddresses == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            var nftprojects = await (from a in db.Nftprojects
                where a.State == "active" &&
                      a.Activatepayinaddress 
                                     orderby a.Lastcheckforutxo
                select a).AsNoTracking().ToListAsync(cancellationToken);

            int i = 0;
            foreach (var proj in nftprojects)
            {
                i++;

                if (cancellationToken.IsCancellationRequested)
                    break;

                if (string.IsNullOrEmpty(proj.Policyaddress))
                    continue;

                await GlobalFunctions.UpdateLastActionProjectAsync(db, proj.Id,redis);
                await GlobalFunctions.UpdateLifesignAsync(db, serverid);

                var backgroundcheck = await (from a in db.Backgroundservers
                    where a.State == "active" && a.Actualtask == nameof(BackgroundTaskEnums.checkpayinaddresses) &&
                          a.Actualprojectid == proj.Id
                    select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (backgroundcheck != null)
                {
                    /*  await LogAsync(db,
                          $"{i} of {nftprojects.Count()} - Project is currently checked by server {backgroundcheck.Id} - skipping - Project: {proj.Id} {proj.Projectname}", "", serverid);*/
                    continue;
                }

                var a1 = await (from a in db.Nftprojects
                    where a.Id == proj.Id
                    select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);


                await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet,serverid,redis, proj.Id);

                DateTime lastcheck = DateTime.Now;
                if (a1.Lastcheckforutxo != null)
                    lastcheck = (DateTime) a1.Lastcheckforutxo;
                await db.SaveChangesAsync(cancellationToken);

                await StaticBackgroundServerClass.LogAsync(db,
                    $"{i} of {nftprojects.Count()} - Check Project Address: {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - Project: {proj.Id} {proj.Projectname} - Lastcheck: {lastcheck.ToShortDateString()}-{lastcheck.ToLongTimeString()}",
                    "", serverid);


                 var utxo1 = await ConsoleCommand.GetNewUtxoAsync(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress, Dataproviders.Koios);
            
             
             int counttxin = (utxo1 != null && utxo1.TxIn != null) ? utxo1.TxIn.Length : 0;


                    // Check for unused addresses
                    if (proj.Lastinputonaddress == null || proj.Lastinputonaddress < DateTime.Now.AddDays(-7) && proj.Donotdisablepayinaddressautomatically==false)
                    {
                        await DeactivatePayinAddress(db, proj.Id, serverid);
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"No TXIN since 1 week. Deactivating the address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - Project-Id: {proj.Id} - {proj.Projectname}",
                            "",
                            serverid);
                    }




                    int i2 = 0;
                for (int i1 = 0; i1 < counttxin; i1++)
                {
                    BuildTransactionClass buildTransaction = new();
                    buildTransaction.LogFile += $"Check Payin Address{Environment.NewLine}";
                    buildTransaction.LogFile +=
                        (string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress) +
                        Environment.NewLine;

                    await UpdateLastCheckForUtxo(db, a1.Id, serverid);
                    await GlobalFunctions.UpdateLastInputOnProjectAsync(db, proj.Id);

                    string txhashx = utxo1.GetTxHashId(i1);
                    if (string.IsNullOrEmpty(txhashx))
                    {
                        await UpdateLastCheckForUtxo(db, a1.Id, serverid);
                        break;
                    }
                   

                    var utxonew = utxo1.TxIn.FirstOrDefault(x => x.TxHashId == txhashx);
                    long adaamount = utxo1.GetLovelace(txhashx);

                    if (adaamount == 0 || utxonew == null)
                    {
                        await UpdateLastCheckForUtxo(db, a1.Id, serverid);
                        continue;
                    }

                    string tokens = "";

                    if (utxonew.Tokens!=null && utxonew.Tokens.Any()) 
                    {
                        // This is for ADA Handles 
                        var adahandles = await (from a in db.Adahandles
                            where a.Policyid == utxonew.Tokens.First().PolicyId
                            select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);
                        if (adahandles!=null)
                            continue;
                    }


                    var checktxhash = await (from a in db.Projectaddressestxhashes
                        where a.Txhash == txhashx
                        select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
                    if (checktxhash != null)
                    {
                        if (checktxhash.Created < DateTime.Now.AddHours(-1))
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Found TxHash already in database - but older than 1 hour trying again - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                                txhashx, serverid);
                        }
                        else
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Found TxHash already in database - continue with next - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                                "", serverid);
                            continue;
                        }
                    }


                    if (i2 > 60)
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"There are more than 60 txin found ({counttxin}) - continue with next address - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                            "", serverid);
                        break;
                    }

                    i2++;

                    long token1 = 0;
                    string assetid1 = "";
                    string policyid1 = "";
                    if (utxonew.Tokens != null && utxonew.Tokens.Any())
                    {
                        policyid1 = utxonew.Tokens.First().PolicyId;
                        assetid1= utxonew.Tokens.First().TokennameHex;
                       // token1= utxonew.Tokens.First().Quantity;
                    }

                    int countpolicyids = utxonew.Tokens!=null ? utxonew.Tokens.GroupBy(x => x.PolicyId).Count() :0;

                    if (utxonew.Tokens != null && utxonew.Tokens.Any())
                    {
                        foreach (var txInTokensClass in utxonew.Tokens)
                        {
                            if (txInTokensClass.PolicyId == policyid1)
                                token1 += txInTokensClass.Quantity;
                        }
                    }

                    var price = await (from a in db.Pricelists
                        where a.NftprojectId == proj.Id && a.State == "active" && 
                              a.Priceinlovelace == adaamount &&
                              a.Currency == Coin.ADA.ToString() &&
                              (a.Priceintoken == null ||
                               (a.Priceintoken == token1 && (a.Tokenassetid == assetid1.FromHex() || a.Tokenassetid == assetid1 || a.Tokenassetid == null ||
                                                             a.Tokenassetid == "" || a.Assetnamehex == assetid1) &&
                                a.Tokenpolicyid == policyid1)) &&
                              (a.Validfrom == null || DateTime.Now > a.Validfrom) &&
                              (a.Validto == null || a.Validto > DateTime.Now)
                        select a).FirstOrDefaultAsync(cancellationToken);

                    // Hack for free NFTS - always 2 ADA
                    if (price == null && adaamount == 2000000)
                    {
                        price = await (from a in db.Pricelists
                                       where a.NftprojectId == proj.Id && a.State == "active" && a.Priceinlovelace == 0 &&
                                  a.Currency == Coin.ADA.ToString() &&
                                  (a.Priceintoken == null ||
                                   (a.Priceintoken == token1 && (a.Tokenassetid == assetid1.FromHex() || a.Tokenassetid == assetid1 || a.Tokenassetid == null ||
                                                                 a.Tokenassetid == "" || a.Assetnamehex == assetid1) &&
                                    a.Tokenpolicyid == policyid1)) &&
                                  (a.Validfrom == null || DateTime.Now > a.Validfrom) &&
                                  (a.Validto == null || a.Validto > DateTime.Now)
                            select a).FirstOrDefaultAsync(cancellationToken);
                    }

                    string senderaddress = await ConsoleCommand.GetSenderAsync(txhashx);
                    if (string.IsNullOrEmpty(senderaddress))
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Could not determine sender - contine - next time{(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                            "", serverid);
                        continue;
                    }
                    else
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Senderadresse determinated for txhash {txhashx} - Sender {senderaddress}", "", serverid);
                    }


                    if (checktxhash != null)
                    {
                        // Save TX to a database - this is necessary because the round robin is faster than the cardano blockchain - so wie save this tx, which we already made
                        await db.Database.ExecuteSqlRawAsync(
                            $"delete from projectaddressestxhashes where txhash='{txhashx}'", cancellationToken: cancellationToken);
                    }

                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Deleting and creating new txhash {txhashx} from projectaddressestxhash ", "", serverid);
                    await db.Projectaddressestxhashes.AddAsync(new()
                    {
                        Created = DateTime.Now, Txhash = txhashx,
                        Address = (string.IsNullOrEmpty(proj.Alternativeaddress)
                            ? proj.Policyaddress
                            : proj.Alternativeaddress),
                        Lovelace = adaamount, Tokens = tokens
                    }, cancellationToken);
                    try
                    {
                        await db.SaveChangesAsync(cancellationToken);
                    }
                    catch
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Could not insert the txinhash - it already exists - skipping {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                            "", serverid);
                        GlobalFunctions.ResetContextState(db);
                        continue;
                    }

                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Saving TXIN to Database - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                        "", serverid);

                    if (await CheckForPriceWithTokens(db, mainnet, serverid, redis, utxonew, adaamount, price, proj, txhashx, senderaddress, utxo1)) continue;
                    if (await CheckForSecondToken(db, mainnet, serverid, redis, countpolicyids, adaamount, proj, txhashx, senderaddress, utxo1)) continue;
                    if (await CheckForPolicyExpiration(db, mainnet, serverid, redis, proj, txhashx, senderaddress, adaamount, utxo1)) continue;
                    if (await CheckForTooLessAdaAmount(db, serverid, adaamount, proj, txhashx)) continue;
                    if (await CheckIfPriceWasFound(db, cancellationToken, mainnet, serverid, redis, price, proj, adaamount, txhashx, senderaddress, utxo1)) continue;

                    long countnft = price.Countnftortoken;
                    if (await CheckIfStillEnoughTokensAvailable(db, mainnet, serverid, redis, proj, countnft, adaamount, txhashx, senderaddress, utxo1)) continue;


                    if (adaamount > 1500000 && !string.IsNullOrEmpty(txhashx) && !string.IsNullOrEmpty(senderaddress))
                    {

                        // Check for Conditions - if not met, send back
                        var cond = await CheckSalesConditionClass.CheckForSaleConditionsMet(db,redis, proj.Id, senderaddress,
                            countnft, serverid, proj.Usefrankenprotection, Blockchain.Cardano);
                        if (cond.ConditionsMet == false)
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Conditions not met: Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - Project-Id:{proj.Id} - {senderaddress}",
                                "", serverid);
                            await SendBackFromProjectAddress(db, proj, txhashx,
                                (string.IsNullOrEmpty(proj.Alternativeaddress)
                                    ? proj.Policyaddress
                                    : proj.Alternativeaddress), senderaddress,
                                "Sale condition not met. Please contact seller.", serverid, mainnet, adaamount, redis, utxo1);
                            continue;
                        }




                        await StaticBackgroundServerClass.LogAsync(db,
                            $"{i1} - Reserve {countnft} NFT for - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                            "", serverid);

                        string guid = GlobalFunctions.GetGuid();

                        List<Nftreservation> selectedreservations = new();

                        // Reserve the tokens
                        selectedreservations = await NftReservationClass.ReserveRandomNft(db,redis, guid, proj.Id,
                            countnft, proj.Expiretime,
                            false, true, Coin.ADA);

                        if (selectedreservations.Sum(x => x.Tc) < countnft)
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Found Amount {adaamount} but not enough nft available(3) - Sending back - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                                "",
                                serverid);
                            await NftReservationClass.ReleaseAllNftsAsync(db,redis, guid, serverid);
                            await SendBackFromProjectAddress(db, proj, txhashx,
                                (string.IsNullOrEmpty(proj.Alternativeaddress)
                                    ? proj.Policyaddress
                                    : proj.Alternativeaddress), senderaddress, "No more NFT available. Sorry.", serverid, mainnet, adaamount, redis, utxo1);
                            continue;
                        }

                        if (selectedreservations.Sum(x => x.Tc) > countnft)
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"More NFT reserved than requested - Try again later - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                                "",
                                serverid);
                            await NftReservationClass.ReleaseAllNftsAsync(db,redis, guid, serverid);
                            await db.Database.ExecuteSqlRawAsync(
                                $"delete from projectaddressestxhashes where txhash='{txhashx}'", cancellationToken: cancellationToken);
                            continue;
                        }


                        if (proj.Maxsupply == 1)
                        {
                            if (await CheckFOrRestprice(db, mainnet, serverid, redis, proj, countnft, adaamount, senderaddress, guid, txhashx, utxo1)) continue;
                        }




                        await StaticBackgroundServerClass.LogAsync(db,
                            $"{i1} - We have enough NFT available - Preparing Token - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}  - {proj.Projectname}",
                            "", serverid);
                        List<Nft> listnft = new();
                        bool failed = false;
                        foreach (var sn in selectedreservations)
                        {
                            var snx = await (from a in db.Nfts
                                    .Include(a => a.Nftproject)
                                    .ThenInclude(a => a.Customer)
                                    .AsSplitQuery()
                                    .Include(a => a.Nftproject)
                                    .ThenInclude(a => a.Settings)
                                    .AsSplitQuery()
                                    .Include(a => a.Nftproject)
                                    .ThenInclude(a => a.Customerwallet)
                                    .AsSplitQuery()
                                    .Include(a => a.InverseMainnft)
                                    .ThenInclude(a => a.Metadata)
                                    .AsSplitQuery()
                                    .Include(a => a.Metadata)
                                    .AsSplitQuery()
                                where a.Id == sn.NftId
                                select a).FirstOrDefaultAsync(cancellationToken);

                            listnft.Add(snx);

                            await StaticBackgroundServerClass.LogAsync(db,
                                $"NFT Added to reservation for txhash {txhashx} - NftId: {sn.NftId} - {sn.Tc}  - ProjId:{proj.Id} - ProjName: {proj.Projectname}",
                                "", serverid);


                            var found = StaticBackgroundServerClass.FoundNftInBlockchain(snx, proj, mainnet,
                                out var bfq, out var resultjson);
                            if (!found)
                                continue;
                            if (!(bfq + sn.Tc > snx.Nftproject.Maxsupply)) continue;

                            await StaticBackgroundServerClass.LogAsync(db,
                                $"FAILED - NFT was already sold - found on Blockfrost for txhash {txhashx} - NftId: {sn.NftId} - {sn.Tc}",
                                "", serverid);
                            snx.Checkpolicyid = true;
                            if (snx.Nftproject.Maxsupply == 1)
                            {
                                snx.Soldcount = 1;
                                snx.State = "sold";
                                snx.Reservedcount = 0;
                            }

                            await db.SaveChangesAsync(cancellationToken);
                            failed = true;
                        }


                        if (failed)
                        {
                            txhashx ??= "";

                            await StaticBackgroundServerClass.LogAsync(db,
                                $"FAILED - Releasing TX because of already sold for txhash {txhashx}", "", serverid);
                            await NftReservationClass.ReleaseAllNftsAsync(db,redis, guid, serverid);

                            await db.Database.ExecuteSqlRawAsync(
                                $"delete from projectaddressestxhashes where txhash='{txhashx}'", cancellationToken: cancellationToken);
                            continue;
                        }


                        var project = listnft.First().Nftproject;

                        // Catch Additional Payout Wallets from the Database
                        var additionalPayouts = await (from a in db.Nftprojectsadditionalpayouts
                                .Include(a => a.Wallet)
                                .AsSplitQuery()
                            where a.NftprojectId == project.Id &&
                                  a.Coin == Coin.ADA.ToString() &&
                                  (a.Custompropertycondition == null || a.Custompropertycondition == "")
                            select a).AsNoTracking().ToArrayAsync(cancellationToken: cancellationToken);

                        var discount =
                            await PriceListDiscountClass.GetPricelistDiscount(db,redis, project.Id, senderaddress,null,null, serverid, Blockchain.Solana);

                        var rewards = await RewardsClass.GetTokenAndStakeRewards(db,redis, senderaddress);

                        string s = "";

                        List<MultipleTokensClass> mtc = new();

                        foreach (var sn in selectedreservations)
                        {
                            var nft1 = await (from a in db.Nfts
                                    .Include(a => a.Nftproject)
                                    .ThenInclude(a => a.Customer)
                                    .AsSplitQuery()
                                    .Include(a => a.Nftproject)
                                    .ThenInclude(a => a.Settings)
                                    .AsSplitQuery()
                                    .Include(a => a.InverseMainnft)
                                    .ThenInclude(a => a.Metadata)
                                    .AsSplitQuery()
                                    .Include(a => a.Metadata)
                                    .AsSplitQuery()
                                where a.Id == sn.NftId
                                select a).FirstOrDefaultAsync(cancellationToken);
                            mtc.Add(new() {nft = nft1, tokencount = sn.Tc, Multiplier = nft1.Multiplier});
                        }

                        if (txhashx == null)
                            txhashx = "";
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Mint and send multiple Token for txhash {txhashx} - Senderaddress: {senderaddress} - Proj.Id: {proj.Id} - {proj.Projectname}",
                            "", serverid);


                        PromotionClass promotion = null;
                        if (price.PromotionId != null)
                            promotion = await GlobalFunctions.GetPromotionAsync(db,redis, (int)price.PromotionId, price.Promotionmultiplier ?? 1);



                        //
                        // Mint the Tokens
                        //
                        if (proj.Cip68)
                        {
                            s = ConsoleCommand.MintAndSendMultipleTokensFromProjectAddressCip68(redis, utxonew,
                                mtc.ToArray(),
                                string.IsNullOrEmpty(cond.SendBackAddress.Address)
                                    ? senderaddress
                                    : cond.SendBackAddress.Address,
                                project,
                                ConsoleCommand.GetNodeVersion(), mainnet, adaamount, additionalPayouts, discount?.Sendbackdiscount ?? 0f,
                                rewards.StakeReward, rewards.TokenReward, promotion, ref buildTransaction, true, false,
                                1, price.Changeaddresswhenpaywithtokens == "buyer" ? senderaddress : null);
                        }
                        else
                        {
                            s = ConsoleCommand.MintAndSendMultipleTokensFromProjectAddress(db, redis, utxonew,
                                mtc.ToArray(),
                                string.IsNullOrEmpty(cond.SendBackAddress.Address)
                                    ? senderaddress
                                    : cond.SendBackAddress.Address,
                                project,
                                ConsoleCommand.GetNodeVersion(), mainnet, adaamount, additionalPayouts, discount?.Sendbackdiscount ?? 0f,
                                rewards.StakeReward, rewards.TokenReward, promotion, ref buildTransaction, true, false,
                                1, price.Changeaddresswhenpaywithtokens == "buyer" ? senderaddress : null);
                        }

                        if (s != "OK")
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Mint was not successful - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - Proj.Id: {proj.Id} - TxHash {txhashx} - Marking the NFT as Error",
                                $"{s} {buildTransaction.LogFile}", serverid);
                            await NftReservationClass.SetLogfileToNfts(db, guid, buildTransaction.LogFile);
                            await NftReservationClass.MarkAllNftsAsError(db,redis, guid, buildTransaction.LogFile);
                            await db.Database.ExecuteSqlRawAsync(
                                $"delete from projectaddressestxhashes where txhash='{txhashx}'",
                                cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Mint was successful - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - Proj.Id: {proj.Id} - TxHash {txhashx}  - {proj.Projectname}",
                                $"{s} {buildTransaction.LogFile}", serverid);
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Save Transaction (Project Pay-In Address) - Senderaddress:{senderaddress} - TxHash {txhashx} TxHash: {buildTransaction.TxHash}",
                                $"{s} {buildTransaction.LogFile}",
                                serverid);

                            if (promotion != null)
                            {
                                await GlobalFunctions.SetPromotionSoldcountAsync(db, (int)price.PromotionId,
                                    promotion.Tokencount);
                            }


                            // Save TX only when the Nfts are not free
                            if (price.Priceinlovelace > 0)
                            {
                                var receiveraddress = buildTransaction.BuyerTxOut == null
                                    ? string.IsNullOrEmpty(cond.SendBackAddress.Address)
                                        ? senderaddress
                                        : cond.SendBackAddress.Address
                                    : buildTransaction.BuyerTxOut.ReceiverAddress;


                                await StaticBackgroundServerClass.SaveTransactionToDatabase(db,redis, buildTransaction,
                                    proj.CustomerId, null,
                                    project.Id, nameof(TransactionTypes.paidonprojectaddress),
                                    proj.CustomerwalletId, serverid,Coin.ADA, 0,
                                    null, cond, promotion, buildTransaction.SignedTransaction, null,selectedreservations);
                            }

                            try
                            {
                                await NftReservationClass.MarkAllNftsAsSold(db, guid, proj.Cip68, buildTransaction.BuyerTxOut.ReceiverAddress);
                            }
                            catch 
                            {
                                await Task.Delay(1000, cancellationToken);
                                GlobalFunctions.ResetContextState(db);

                                // Sometime this does not work
                                await Task.Delay(1000, cancellationToken);
                                await NftReservationClass.MarkAllNftsAsSold(db, guid, proj.Cip68, buildTransaction.BuyerTxOut.ReceiverAddress);
                            }

                            foreach (var a in listnft)
                            {
                                if (project.Maxsupply == 1)
                                {
                                    var id = a.InstockpremintedaddressId;
                                    a.InstockpremintedaddressId = null;
                                    a.State = "sold";
                                    a.Soldcount = 1;
                                    a.Reservedcount = 0;
                                    a.Errorcount = 0;
                                    StaticBackgroundServerClass.SaveToRedis(
                                        $"{(mainnet ? "mainnet_" : "testnet_")}nft_{a.Id}", "sold",
                                        new(4, 0, 0));
                                    await GlobalFunctions.ClearInstockPremintedAddressAsync(db, id);
                                }
                            }

                            await db.SaveChangesAsync(cancellationToken);

                            await WhitelistFunctions.SaveUsedAddressesToWhitelistSaleCondition(db, project.Id,
                                buildTransaction.BuyerTxOut == null
                                    ? senderaddress
                                    : buildTransaction.BuyerTxOut.ReceiverAddress,
                                cond.SendBackAddress?.OriginatorAddress, cond.SendBackAddress?.StakeAddress, buildTransaction.TxHash, selectedreservations.Count);
                        }
                    }
                    else
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Canceled because of to small adaamount or not txhash or sender found {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx} - {adaamount} - Sender:{senderaddress}",
                            "", serverid);
                    }

                    await UpdateLastCheckForUtxo(db, a1.Id, serverid);

                }
            }



            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private async Task<bool> CheckFOrRestprice(EasynftprojectsContext db, bool mainnet, int serverid,
            IConnectionMultiplexer redis, Nftproject proj, long countnft, long adaamount, string senderaddress, string guid,
            string txhashx, TxInAddressesClass utxo1)
        {
            long restprice = await StaticBackgroundServerClass.CheckRestPrice(db, proj, countnft,
                adaamount, senderaddress, guid, Coin.ADA, String.Empty);
            if (restprice < 1000000)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Restprice is too small: Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - Project-Id:{proj.Id} - {senderaddress} {restprice}",
                    "", serverid);
                await SendBackFromProjectAddress(db, proj, txhashx,
                    (string.IsNullOrEmpty(proj.Alternativeaddress)
                        ? proj.Policyaddress
                        : proj.Alternativeaddress), senderaddress,
                    "The seller has an error in his payout configuration.", serverid, mainnet, adaamount, redis, utxo1);
                await NftReservationClass.ReleaseAllNftsAsync(db, redis, guid, serverid);
                return true;
            }

            return false;
        }

        private async Task<bool> CheckForPriceWithTokens(EasynftprojectsContext db, bool mainnet, int serverid,
            IConnectionMultiplexer redis, TxInClass utxonew, long adaamount, Pricelist price, Nftproject proj, string txhashx,
            string senderaddress, TxInAddressesClass utxo1)
        {
            // When Token 1 exists, we will check if there is a price available for this
            if (utxonew.Tokens != null && utxonew.Tokens.Any() && adaamount > 2000000)
            {
                if (price == null)
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Found Tokens in Project Address - Sending back - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                        "", serverid);
                    // Price not found - send back
                    string sendbackmessage = await CheckIfPricelistHasTokens(db, proj)
                        ? "Please send the correct amount of tokens"
                        : "Please send only ADA, no Tokens. Thank you.";
                    await SendBackFromProjectAddress(db, proj, txhashx,
                        (string.IsNullOrEmpty(proj.Alternativeaddress)
                            ? proj.Policyaddress
                            : proj.Alternativeaddress), senderaddress,
                        sendbackmessage, serverid, mainnet, adaamount, redis, utxo1);
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> CheckIfStillEnoughTokensAvailable(EasynftprojectsContext db, bool mainnet, int serverid,
            IConnectionMultiplexer redis, Nftproject proj, long countnft, long adaamount, string txhashx, string senderaddress,
            TxInAddressesClass utxo1)
        {
            // Check if there are enough nfts / Tokens available
            if (proj.Totaltokens1 - proj.Tokensreserved1 - proj.Tokenssold1 < countnft)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Found Amount {adaamount} but not enough nft available(1) - {proj.Totaltokens1} - {proj.Tokensreserved1} - {proj.Tokenssold1} - Sending back - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                    "",
                    serverid);
                await SendBackFromProjectAddress(db, proj, txhashx,
                    (string.IsNullOrEmpty(proj.Alternativeaddress)
                        ? proj.Policyaddress
                        : proj.Alternativeaddress), senderaddress, "No more NFT available. Sorry.", serverid, mainnet,
                    adaamount, redis, utxo1);
                return true;
            }

            return false;
        }

        private async Task<bool> CheckIfPriceWasFound(EasynftprojectsContext db, CancellationToken cancellationToken, bool mainnet,
            int serverid, IConnectionMultiplexer redis, Pricelist price, Nftproject proj, long adaamount, string txhashx,
            string senderaddress, TxInAddressesClass utxo1)
        {
            if (price != null) return false;
            var pricelist = await (from a in db.Pricelists
                where a.NftprojectId == proj.Id
                select a).AsNoTracking().ToArrayAsync(cancellationToken: cancellationToken);

            await StaticBackgroundServerClass.LogAsync(db,
                $"Found Amount {adaamount} but no price found - Sending back - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                JsonConvert.SerializeObject(pricelist, Formatting.Indented,
                    new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}), serverid);

            // Price not found - send back
            string stxhash = await SendBackFromProjectAddress(db, proj, txhashx,
                (string.IsNullOrEmpty(proj.Alternativeaddress)
                    ? proj.Policyaddress
                    : proj.Alternativeaddress), senderaddress, "Wrong amount sent.", serverid, mainnet, adaamount, redis,
                utxo1);


            await StaticBackgroundServerClass.LogAsync(db,
                $"Found Amount {adaamount} but no price - Payin Addr. {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx} - SendBack {stxhash}",
                JsonConvert.SerializeObject(pricelist, Formatting.Indented,
                    new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}), serverid);

            return true;

        }

        private static async Task<bool> CheckForTooLessAdaAmount(EasynftprojectsContext db, int serverid, long adaamount,
            Nftproject proj, string txhashx)
        {
            if (adaamount < 1500000)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Found ADA in Project Address - but too less - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx} - Lovelace: {adaamount}",
                    "", serverid);
                return true;
            }

            return false;
        }

        private async Task<bool> CheckForSecondToken(EasynftprojectsContext db, bool mainnet, int serverid,
            IConnectionMultiplexer redis, int countpolicyids, long adaamount, Nftproject proj, string txhashx,
            string senderaddress, TxInAddressesClass utxo1)
        {
            // If a second Token exists, this is not valid, because currently we only support 1 Token as additional payment 
            if (countpolicyids > 1 && adaamount > 2000000)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Found Tokens in Project Address - Sending back - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                    "", serverid);
                // Price not found - send back
                await SendBackFromProjectAddress(db, proj, txhashx,
                    (string.IsNullOrEmpty(proj.Alternativeaddress)
                        ? proj.Policyaddress
                        : proj.Alternativeaddress), senderaddress,
                    "Please send only the correct Tokens. Thank you.", serverid, mainnet, adaamount, redis, utxo1);
                return true;
            }

            return false;
        }

        private async Task<bool> CheckForPolicyExpiration(EasynftprojectsContext db, bool mainnet, int serverid,
            IConnectionMultiplexer redis, Nftproject proj, string txhashx, string senderaddress, long adaamount,
            TxInAddressesClass utxo1)
        {
            if (proj.Policyexpire != null && DateTime.Now > proj.Policyexpire)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Policy is expired - Sending back - Project Address {(string.IsNullOrEmpty(proj.Alternativeaddress) ? proj.Policyaddress : proj.Alternativeaddress)} - TxHash {txhashx}",
                    "", serverid);
                // Price not found - send back
                await SendBackFromProjectAddress(db, proj, txhashx,
                    (string.IsNullOrEmpty(proj.Alternativeaddress)
                        ? proj.Policyaddress
                        : proj.Alternativeaddress), senderaddress,
                    "The policy of the project is locked. No more NFT can be minted.", serverid, mainnet, adaamount, redis,
                    utxo1);
                return true;
            }

            return false;
        }

        private async Task<bool> CheckIfPricelistHasTokens(EasynftprojectsContext db, Nftproject proj)
        {
           var pricelist=await (from a in db.Pricelists
                                where a.NftprojectId == proj.Id
                                    && a.State=="active" && a.Priceintoken!=null && a.Priceintoken!= 0 && (a.Validfrom == null || DateTime.Now > a.Validfrom) &&
                                    (a.Validto == null || a.Validto > DateTime.Now)
                                select a).AsNoTracking().FirstOrDefaultAsync();

           return pricelist != null;
        }


        private async Task DeactivatePayinAddress(EasynftprojectsContext db, int projectid, int serverid)
        {
            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, $"update nftprojects set activatepayinaddress = 0 where id={projectid}", serverid);
        }


        private async Task UpdateLastCheckForUtxo(EasynftprojectsContext db, int projectid, int serverid)
        {
            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, $"update nftprojects set lastcheckforutxo=now() where id={projectid}", serverid);
        }

        private async Task<string> SendBackFromProjectAddress(EasynftprojectsContext db, Nftproject project, string txhash, string senderaddress, string receiveraddress, string sendbackmessage, int serverid, bool mainnet, long lovelace, IConnectionMultiplexer redis, TxInAddressesClass givenutxo)
        {
            if (receiveraddress == senderaddress)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"SendBackFromProjectAddress Receiver and Sender is the same {(string.IsNullOrEmpty(project.Alternativeaddress) ? project.Policyaddress : project.Alternativeaddress)} {receiveraddress}", "", serverid);
                return "";
            }

            await StaticBackgroundServerClass.LogAsync(db,
                $"SendBackFromProjectAddress start {(string.IsNullOrEmpty(project.Alternativeaddress) ? project.Policyaddress : project.Alternativeaddress)} {receiveraddress}", "", serverid);

            BuildTransactionClass buildtransaction = new();

            //       string s = ConsoleCommand.SendAllAdaAndTokens(db,redis, (string.IsNullOrEmpty(project.Alternativeaddress) ? project.Policyaddress : project.Alternativeaddress), (string.IsNullOrEmpty(project.Alternativeaddress) ? project.Policyskey : project.Alternativepayskey), project.Password, receiveraddress,
            //         mainnet, ref buildtransaction, txhash, 0, 0, sendbackmessage, givenutxo);


            string s = CardanoSharpFunctions.SendAllAdaAndTokens(db, redis, 
                (string.IsNullOrEmpty(project.Alternativeaddress) ? project.Policyaddress : project.Alternativeaddress), 
                (string.IsNullOrEmpty(project.Alternativeaddress) ? project.Policyskey : project.Alternativepayskey),
                (string.IsNullOrEmpty(project.Alternativeaddress) ? project.Policyvkey : project.Alternativepayvkey),
                project.Password, receiveraddress,
                mainnet, 
                ref buildtransaction, txhash, 0, 0, 
                sendbackmessage, givenutxo);



            //  string s = ConsoleCommand.SendAllAdaAndTokensFromProjectAddresses(pv, receiveraddress, mainnet, ref buildtransaction, txhash, 0, sendbackmessage, givenutxo);
            if (s == "OK")
                await StaticBackgroundServerClass.LogAsync(db,
                    $"SendBackFromProjectAddress successful {(string.IsNullOrEmpty(project.Alternativeaddress) ? project.Policyaddress : project.Alternativeaddress)} {receiveraddress}", "", serverid);
            else
                await StaticBackgroundServerClass.LogAsync(db,
                    $"SendBackFromProjectAddress failed {(string.IsNullOrEmpty(project.Alternativeaddress) ? project.Policyaddress : project.Alternativeaddress)} {receiveraddress}", s, serverid);

            try
            {
                await db.SaveChangesAsync();
                await GlobalFunctions.SaveRefundLogAsync(db, senderaddress, receiveraddress, txhash, s == "OK",
                    buildtransaction.TxHash,
                    sendbackmessage, project.Id, buildtransaction.LogFile, lovelace, buildtransaction.Fees, buildtransaction.NmkrCosts, Coin.ADA);
            }
            catch (Exception e)
            {
               await GlobalFunctions.LogExceptionAsync(db, e.Message, e.StackTrace, serverid);
            }
            await StaticBackgroundServerClass.LogAsync(db,
                $"SendBackFromProjectAddress end{(string.IsNullOrEmpty(project.Alternativeaddress) ? project.Policyaddress : project.Alternativeaddress)} {receiveraddress}", buildtransaction.LogFile, serverid);

            return buildtransaction.TxHash;
        }


    }
}
