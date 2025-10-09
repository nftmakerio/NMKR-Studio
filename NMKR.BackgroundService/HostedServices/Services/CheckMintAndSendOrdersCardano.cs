using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckMintAndSendOrdersCardano : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.mintandsend;
            if (server.Checkmintandsend == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);

            int runcounter = 0;
            do
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                runcounter++;

                if (runcounter > 10)
                    break;
                List<Mintandsend> mas = new();
                try
                {
                    mas = await (from a in db.Mintandsends
                            .Include(a => a.Nftproject)
                            .ThenInclude(a => a.Settings)
                            .AsSplitQuery()
                            .Include(a => a.Nftproject)
                            .ThenInclude(a => a.Cip68smartcontract)
                            .AsSplitQuery()
                            .Include(a => a.Customer)
                            .AsSplitQuery()
                                 where a.State == "execute" &&
                                       /*a.Usecustomerwallet == true && */ a.Remintandburn == false && a.Coin == Coin.ADA.ToString()
                                 select a).Take(50).ToListAsync(cancellationToken);// 50 is enough, because we only add max. 15 to 20 in one transaction
                }
                catch (Exception e)
                {
                    GlobalFunctions.ResetContextState(db);
                    await StaticBackgroundServerClass.EventLogException(db, 515, e, serverid);
                }

                var mas1 = mas.GroupBy(x => x.Nftproject).ToList();

                if (mas1.Count == 0)
                    return;

                List<NewMintAndSendClass> nmascl = new();
                foreach (var mas2 in mas1)
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Adding Mint and Send: {mas2.Key.Projectname} - {mas2.Key.Id}", "", serverid);
                    NewMintAndSendClass nmasc = new() { NftProject = mas2.Key, CountNfts = 0 };

                    int MaxCountMintAndSend = nmasc.NftProject.Maxcountmintandsend;

                    if (mas2.Key.Customer.Newpurchasedmints < MaxCountMintAndSend || mas2.Key.Customer.Newpurchasedmints < 1)
                    {
                        MaxCountMintAndSend = Math.Max(1, Convert.ToInt32( mas2.Key.Customer.Newpurchasedmints));
                    }

                    nmasc.NftReservations = new();
                    nmasc.MintAndSends = new();
                    long metadatasize = 0;
                    foreach (var mintandsend in mas2)
                    {
                        if (nmasc.CountNfts > (MaxCountMintAndSend == 0
                                ? 1
                                : MaxCountMintAndSend))
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Adding Mint and Send - too many NFTS (1): {mas2.Key.Projectname} - {mas2.Key.Id} - {mintandsend.Receiveraddress} - {mintandsend.Reservationtoken}",
                                "", serverid);
                            break;
                        }

                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Adding Mint and Send: {mas2.Key.Projectname} - {mas2.Key.Id} - {mintandsend.Receiveraddress} - {mintandsend.Reservationtoken}",
                            "", serverid);
                        var nftreservations = await (from a in db.Nftreservations
                                .Include(a => a.Nft)
                                .ThenInclude(a => a.InverseMainnft)
                                .ThenInclude(a => a.Metadata)
                                .AsSplitQuery()
                                .Include(a => a.Nft)
                                .ThenInclude(a => a.Metadata)
                                .AsSplitQuery()
                                .Include(a => a.Nft)
                                .ThenInclude(a => a.Instockpremintedaddress)
                                .AsSplitQuery()
                                                     where a.Reservationtoken == mintandsend.Reservationtoken && a.Mintandsendcommand == true
                                                     select a).ToListAsync(cancellationToken);

                        if (nmasc.CountNfts > 0 && (nmasc.CountNfts + nftreservations.Count) >
                            (nmasc.NftProject.Maxcountmintandsend == 0 ? 1 : nmasc.NftProject.Maxcountmintandsend))
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Adding Mint and Send - too many NFTS (2): {mas2.Key.Projectname} - {mas2.Key.Id} - {mintandsend.Receiveraddress} - {mintandsend.Reservationtoken}",
                                "", serverid);
                            break;
                        }




                        // Check Metadata size - not more than 15000 bytes
                        var mdc = new GetMetadataClass((from a in nmasc.NftReservations select new NftIdWithMintingAddressClass(a.NftId, "")).ToArray(), true, db);
                        var md = await mdc.MetadataResultAsync();
                        if (!string.IsNullOrEmpty(md.Metadata))
                        {
                            var mdjson =
                                JsonConvert.SerializeObject(JsonConvert.DeserializeObject(md.Metadata), Formatting.None);
                            metadatasize += mdjson.Length;
                            if (metadatasize >= 15000 && nmasc.CountNfts > 0)
                            {
                                await StaticBackgroundServerClass.LogAsync(db,
                                    $"Adding Mint and Send - Metadata too large : {mas2.Key.Projectname} - {mas2.Key.Id} - {mintandsend.Receiveraddress} - {mintandsend.Reservationtoken} - {metadatasize} - Count: {nmasc.CountNfts}",
                                    mdjson, serverid);
                                break;
                            }

                        }



                        nmasc.CountNfts += nftreservations.Count;
                        foreach (var nftreservation in nftreservations)
                        {
                            nmasc.NftReservations.Add(nftreservation);
                        }

                        nmasc.MintAndSends.Add(mintandsend);

                    }

                    nmascl.Add(nmasc);
                }


                foreach (var newMintAndSendClass in nmascl)
                {
                    bool failed = false;

                    var paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db, serverid,"CheckMintAndSendOrdersCardano",null);
                    if (paywallet == null)
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Mint and Send - All Paywallets are blocked - waiting: {newMintAndSendClass.NftProject.Projectname} - {newMintAndSendClass.NftProject.Id}",
                            "", serverid);
                        continue;
                    }



                    // Check for Preminted Tokens
                    var premintedTokens=await(from a in db.Nftprojectsendpremintedtokens
                            .Include(a => a.Blockchain)
                                              where a.State == "active" && a.NftprojectId == newMintAndSendClass.NftProject.Id && a.Blockchain.Name==Blockchain.Cardano.ToString()
                                              select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);
                    Premintedpromotokenaddress premintedTokenWallet = null;
                    if (premintedTokens != null)
                    {
                        premintedTokenWallet = await GlobalFunctions.GetPremintedTokenWalletAndBlockAsync(db, serverid,premintedTokens, newMintAndSendClass.NftReservations.Count(), "CheckMintAndSendOrdersCardanoPremintedTokens", null);
                        if (premintedTokenWallet == null)
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Mint and Send - All PremintedTokenWallets are blocked or empty - waiting: {newMintAndSendClass.NftProject.Projectname} - {newMintAndSendClass.NftProject.Id}",
                                "", serverid);
                            await GlobalFunctions.UnlockPaywalletAsync(db, paywallet);
                            continue;
                        }
                    }



                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Mint and send - Account is ok - start minting: {newMintAndSendClass.NftProject.Projectname} - {newMintAndSendClass.NftProject.Id} {newMintAndSendClass.NftReservations.Count()}",
                        "", serverid);
                    //   JsonConvert.SerializeObject(newMintAndSendClass,Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), serverid);

                    if (!newMintAndSendClass.NftReservations.Any())
                    {
                        await GlobalFunctions.UnlockPaywalletAsync(db, paywallet);
                        await SetNewMintandSendState(db, redis, newMintAndSendClass, "canceled",
                            new() { LogFile = "" }, serverid, cancellationToken);
                        continue;
                    }

                    string errormessage = "";
                    foreach (var res in newMintAndSendClass.NftReservations)
                    {
                        if (res.Nft == null)
                        {
                            errormessage+= $"FAILED - MintNfts - NFT was not found - NftId: {res.NftId} - {res.Tc} - {newMintAndSendClass.NftProject.Maxsupply}"+Environment.NewLine;
                            failed = true;
                            break;
                        }

                        // Last Bastion - check if the asset is known on blockfrost
                        if (!StaticBackgroundServerClass.FoundNftInBlockchain(res.Nft, newMintAndSendClass.NftProject,
                                mainnet, out var bfq, out var resultjson))
                            continue;

                        // Check if the Asset has already minted with mint&send
                        bool found = false;
                        if (newMintAndSendClass.NftProject.Maxsupply == 1)
                        {
                            found = !string.IsNullOrEmpty(GlobalFunctions.GetStringFromRedis(redis, $"MintAndSend_{res.NftId}"));
                            GlobalFunctions.SaveStringToRedis(redis, $"MintAndSend_{res.NftId}",
                                $"MintAndSend_{res.NftId}", 360);
                            if (found)
                            {
                                errormessage+= $"FAILED - MintNfts - NFT was already sold - found in Redis - NftId: {res.NftId} - {res.Tc} - {bfq} - {newMintAndSendClass.NftProject.Maxsupply}" + Environment.NewLine;
                                await StaticBackgroundServerClass.LogAsync(db,errormessage, ".", serverid);
                                
                                failed = true;
                                break;
                            }
                        }


                        if (!(bfq + res.Tc > newMintAndSendClass.NftProject.Maxsupply))
                        {
                            continue;
                        }
                        errormessage += $"FAILED - MintNfts - NFT was already sold - found on Blockfrost for - NftId: {res.NftId} - {res.Tc} - {bfq} - {newMintAndSendClass.NftProject.Maxsupply}" + Environment.NewLine;
                        await StaticBackgroundServerClass.LogAsync(db,
                             errormessage,  
                            ".", serverid);
                        res.Nft.Checkpolicyid = true;
                        if (newMintAndSendClass.NftProject.Maxsupply == 1)
                        {
                            res.Nft.Soldcount = 1;
                            res.Nft.State = "sold";
                            res.Nft.Reservedcount = 0;
                        }

                        await db.SaveChangesAsync(cancellationToken);
                        failed = true;
                        break;
                    }

                    if (failed)
                    {
                        // Release 
                        await SetNewMintandSendState(db, redis, newMintAndSendClass, "error",
                            new() { LogFile = "Mint and send failed, because the NFT is already sold"+Environment.NewLine+errormessage }, serverid, cancellationToken);

                        await db.SaveChangesAsync(cancellationToken);
                        await GlobalFunctions.UnlockPaywalletAsync(db,paywallet);
                        await GlobalFunctions.UnlockPremintedTokenWalletAsync(db, premintedTokenWallet);
                        continue;
                    }

                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Mint and send - Start Minting now: {newMintAndSendClass.NftProject.Projectname} - {newMintAndSendClass.NftProject.Id} {newMintAndSendClass.NftReservations.Count()}",
                        "", serverid);
                    //     JsonConvert.SerializeObject(newMintAndSendClass,new JsonSerializerSettings(){ReferenceLoopHandling = ReferenceLoopHandling.Ignore}), serverid);


                    // Create the Sign Keys for Signind and to calculate the witnesses


                    string payskey = Encryption.DecryptString(paywallet.Privateskey,
                        GeneralConfigurationClass.Masterpassword + paywallet.Salt);

                    string log = "";
                    // Set State to progress
                    foreach (var masx in newMintAndSendClass.MintAndSends)
                    {
                        masx.State = "inprogress";
                        await db.SaveChangesAsync(cancellationToken);
                        log = masx.Buildtransaction ?? "";
                    }


                    BuildTransactionClass buildtransaction = new BuildTransactionClass
                    {
                        LogFile = log+ Environment.NewLine + "Server: " + serverid + Environment.NewLine + "Payaddress: "+ paywallet.Address + Environment.NewLine
                    };

                    buildtransaction.MetadataStandard = newMintAndSendClass.NftProject.Cip68 ? "cip68" : "cip25";

                    string s = newMintAndSendClass.NftProject.Cip68 ?
                        ConsoleCommand.MintAndSendCip68( redis, newMintAndSendClass,
                        paywallet.Address, payskey,
                        mainnet, true, false,
                        newMintAndSendClass.NftProject.Mintandsendminutxo,  ref buildtransaction) :
                        ConsoleCommand.MintAndSendBuildRaw( redis, newMintAndSendClass,
                        paywallet.Address, payskey, premintedTokenWallet, premintedTokens,
                        mainnet, true, false,
                        newMintAndSendClass.NftProject.Mintandsendminutxo,  ref buildtransaction);

                    if (s == "OK")
                    {
                        buildtransaction.LogFile += Environment.NewLine + "TxHash: " + buildtransaction.TxHash;
                      

                        await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, $"update adminmintandsendaddresses set addressblocked=1,blockcounter=0,lasttxhash='{buildtransaction.TxHash}', lasttxdate=NOW() where id='{paywallet.Id}'", serverid);
                        if (premintedTokenWallet != null)
                            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                                $"update premintedpromotokenaddresses set state='blocked',lasttxhash='{buildtransaction.TxHash}' where id='{premintedTokenWallet.Id}'",
                                serverid);



                        long amount = 0;
                        if (buildtransaction.MintingcostsTxOut != null)
                        {
                            amount += buildtransaction.MintingcostsTxOut.Amount;
                        }

                        amount += buildtransaction.BuyerTxOut.Amount;
                        amount += buildtransaction.Fees;

                        int c1 = newMintAndSendClass.NftProject.Settings.Mintandsendcoupons;
                        if (newMintAndSendClass.MintAndSends.First().Remintandburn)
                            c1 = 1;

                        if (newMintAndSendClass.MintAndSends.First().Usecustomerwallet == null ||
                            newMintAndSendClass.MintAndSends.First().Usecustomerwallet == true)
                            await GlobalFunctions.ReduceMintCouponsAsync(db, newMintAndSendClass.NftProject.CustomerId,
                                newMintAndSendClass.NftReservations.Count * c1);

                        // Save Transaction
                        await StaticBackgroundServerClass.SaveTransactionToDatabase(db, redis, buildtransaction,
                            newMintAndSendClass.NftProject.CustomerId, null, newMintAndSendClass.NftProject.Id,
                            TransactionTypes.mintfromcustomeraddress, null,
                            newMintAndSendClass.NftReservations.ToArray(), serverid, Coin.ADA);

                        await SetNewMintandSendState(db, redis, newMintAndSendClass, "success", buildtransaction,
                            serverid,
                            cancellationToken);
                    }
                    else
                    {
                        // Try 3 times
                        var retry = 0;
                        foreach (var masx in newMintAndSendClass.MintAndSends)
                        {
                            masx.State = "execute";
                            masx.Retry++;
                            await db.SaveChangesAsync(cancellationToken);
                            retry = masx.Retry;
                        }
                        
                        if (retry<4)
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Mint and send was error - retrying: {newMintAndSendClass.NftProject.Projectname} - {newMintAndSendClass.NftProject.Id} {newMintAndSendClass.NftReservations.Count()} {paywallet.Address} - Retry: {retry}",
                                buildtransaction.LogFile, serverid);
                            continue;
                        }

                        buildtransaction.LogFile ??= "";
                        buildtransaction.LogFile += s;
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Mint and send was error - releasing the Tokens: {newMintAndSendClass.NftProject.Projectname} - {newMintAndSendClass.NftProject.Id} {newMintAndSendClass.NftReservations.Count()} {paywallet.Address}",
                            buildtransaction.LogFile, serverid);
                        // Release 
                        await SetNewMintandSendState(db, redis, newMintAndSendClass, "error", buildtransaction,
                            serverid,
                            cancellationToken);
                        await GlobalFunctions.UnlockPaywalletAsync(db, paywallet);
                        await GlobalFunctions.UnlockPremintedTokenWalletAsync(db, premintedTokenWallet);
                        await db.SaveChangesAsync(cancellationToken);
                    }
                }
            } while (true);

            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private async Task SetNewMintandSendState(EasynftprojectsContext db, IConnectionMultiplexer redis, NewMintAndSendClass newMintAndSendClass,
            string state, BuildTransactionClass buildtransaction, int serverid, CancellationToken cancellationToken)
        {
            foreach (var mas in newMintAndSendClass.MintAndSends)
            {
                try
                {
                    mas.State = state;
                    mas.Executed = DateTime.Now;
                    mas.Transactionid = buildtransaction.TxHash;
                    mas.Buildtransaction = buildtransaction.LogFile;
                    await db.SaveChangesAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    await StaticBackgroundServerClass.EventLogException(db, 52, e, serverid);
                }

                await SetOnlineNotification(db, state, mas);
                await SetNftStates(db, redis, state, cancellationToken, mas, buildtransaction, newMintAndSendClass.NftProject.Cip68);
            }
        }

        private static async Task SetNftStates(EasynftprojectsContext db, IConnectionMultiplexer redis, string state,
            CancellationToken cancellationToken, Mintandsend mas, BuildTransactionClass buildTransaction, bool cip68)
        {
            if (state == "success")
            {
                try
                {
                    await NftReservationClass.MarkAllNftsAsSold(db, mas.Reservationtoken, cip68, mas.Receiveraddress);
                }
                catch
                {
                    await Task.Delay(1000, cancellationToken);
                    GlobalFunctions.ResetContextState(db);

                    // Sometime this does not work
                    await Task.Delay(1000, cancellationToken);
                    await NftReservationClass.MarkAllNftsAsSold(db, mas.Reservationtoken, cip68, mas.Receiveraddress);
                }
            }
            else
            {
                //await NftReservationClass.ReleaseAllNftsAsync(db,redis, mas.Reservationtoken, serverid);
                await NftReservationClass.MarkAllNftsAsError(db, redis, mas.Reservationtoken, buildTransaction.LogFile);
            }
        }

        private static async Task SetOnlineNotification(EasynftprojectsContext db, string state, Mintandsend mas)
        {
            if (!mas.Onlinenotification) return;

            switch (state)
            {
                case "canceled":
                    {
                        Onlinenotification on = new()
                        {
                            Created = DateTime.Now,
                            CustomerId = mas.CustomerId,
                            Notificationmessage =
                                $"There was an error while reminting the nft(s) ",
                            State = "new",
                            Color = "error"
                        };
                        await db.Onlinenotifications.AddAsync(on);
                        await db.SaveChangesAsync();
                        break;
                    }
                case "error":
                    {
                        Onlinenotification on = new()
                        {
                            Created = DateTime.Now,
                            CustomerId = mas.CustomerId,
                            Notificationmessage =
                                $"There was an error while reminting the nft(s) ",
                            State = "new",
                            Color = "error"
                        };
                        await db.Onlinenotifications.AddAsync(on);
                        await db.SaveChangesAsync();
                        break;
                    }
                case "success":
                    {
                        Onlinenotification on = new()
                        {
                            Created = DateTime.Now,
                            CustomerId = mas.CustomerId,
                            Notificationmessage =
                                $"The nft(s), has reminted and burned",
                            State = "new",
                            Color = "success"
                        };
                        await db.Onlinenotifications.AddAsync(on);
                        await db.SaveChangesAsync();
                        break;
                    }
            }
        }

    }
}
