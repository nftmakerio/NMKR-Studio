using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckRemintAndBurnOrders : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.mintandsend;
            if (server.Checkmintandsend == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);


            List<Mintandsend> mas = new();
            try
            {
                mas = await (from a in db.Mintandsends
                        .Include(a => a.Nftproject)
                        .ThenInclude(a => a.Settings)
                        .AsSplitQuery()
                        .Include(a => a.Customer)
                        .AsSplitQuery()
                             where a.State == "execute" && a.Usecustomerwallet == true && a.Remintandburn == true && a.Coin==Coin.ADA.ToString()
                             select a).ToListAsync(cancellationToken);
            }
            catch (Exception e)
            {
                GlobalFunctions.ResetContextState(db);
                await StaticBackgroundServerClass.EventLogException(db, 51, e, serverid);
            }

            var mas1 = mas.GroupBy(x => x.Nftproject).ToList();

            if (mas1.Count == 0)
                return;

            List<NewMintAndSendClass> nmascl = new();
            foreach (var mas2 in mas1)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Adding ReMint and burn: {mas2.Key.Projectname} - {mas2.Key.Id}", "", serverid);
                NewMintAndSendClass nmasc = new() { NftProject = mas2.Key, CountNfts = 0 };
                nmasc.NftReservations = new();
                nmasc.MintAndSends = new();
                foreach (var mintandsend in mas2)
                {
                    if (nmasc.CountNfts > (nmasc.NftProject.Maxcountmintandsend == 0
                            ? 1
                            : nmasc.NftProject.Maxcountmintandsend))
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Adding ReMint and Burn - too many NFTS (1): {mas2.Key.Projectname} - {mas2.Key.Id} - {mintandsend.Receiveraddress} - {mintandsend.Reservationtoken}",
                            "", serverid);
                        continue;
                    }

                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Adding ReMint and Burn: {mas2.Key.Projectname} - {mas2.Key.Id} - {mintandsend.Receiveraddress} - {mintandsend.Reservationtoken}",
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
                            $"Adding ReMint and Burn - too many NFTS (2): {mas2.Key.Projectname} - {mas2.Key.Id} - {mintandsend.Receiveraddress} - {mintandsend.Reservationtoken}",
                            "", serverid);
                        continue;
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
                var paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db, serverid,"CheckRemintAndBurnOrders", null);
                if (paywallet == null)
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Mint and Send - All Paywallets are blocked - waiting: {newMintAndSendClass.NftProject.Projectname} - {newMintAndSendClass.NftProject.Id}",
                        "", serverid);
                    continue;
                }


                if (!newMintAndSendClass.NftReservations.Any())
                {
                    await GlobalFunctions.UnlockPaywalletAsync(db, paywallet);
                    await SetNewMintandSendState(db, redis, newMintAndSendClass, "canceled",
                        new() { LogFile = "" }, serverid, cancellationToken);
                    continue;
                }


                await StaticBackgroundServerClass.LogAsync(db,
                    $"ReMint and burn - Start Minting now: {newMintAndSendClass.NftProject.Projectname} - {newMintAndSendClass.NftProject.Id} {newMintAndSendClass.NftReservations.Count()}",
                    "", serverid);


                string payskey = Encryption.DecryptString(paywallet.Privateskey,
                    GeneralConfigurationClass.Masterpassword + paywallet.Salt);


                /*       string s = ConsoleCommand.MintAndSend(db, redis, newMintAndSendClass,
                           paywallet.Address, payskey, ConsoleCommand.GetNodeVersion(),
                           mainnet, true,true,
                           newMintAndSendClass.NftProject.Mintandsendminutxo, true, out var buildtransaction);*/

                BuildTransactionClass buildtransaction = new BuildTransactionClass();
                string s = newMintAndSendClass.NftProject.Cip68 ?
                    ConsoleCommand.MintAndSendCip68(redis, newMintAndSendClass,
                        paywallet.Address, payskey, 
                        mainnet, true, true,
                        newMintAndSendClass.NftProject.Mintandsendminutxo,  ref buildtransaction) :
                    ConsoleCommand.MintAndSendBuildRaw( redis, newMintAndSendClass,
                        paywallet.Address, payskey, null,null,
                        mainnet, true, true,
                        newMintAndSendClass.NftProject.Mintandsendminutxo,  ref buildtransaction);

                if (s == "OK")
                {
                    await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, $"update adminmintandsendaddresses set addressblocked=1,blockcounter=0,lasttxhash='{buildtransaction.TxHash}', lasttxdate=NOW() where id='{paywallet.Id}'", serverid);

                    // Save Transaction
                    await StaticBackgroundServerClass.SaveTransactionToDatabase(db,redis, buildtransaction,
                        newMintAndSendClass.NftProject.CustomerId, null, newMintAndSendClass.NftProject.Id,
                        TransactionTypes.mintfromcustomeraddress, null,
                        newMintAndSendClass.NftReservations.ToArray(), serverid, Coin.ADA);

                    await SetNewMintandSendState(db, redis, newMintAndSendClass, "success", buildtransaction, serverid,
                        cancellationToken);

                    await GlobalFunctions.ReduceMintCouponsAsync(db, newMintAndSendClass.NftProject.CustomerId, newMintAndSendClass.NftReservations.Count/2f);

                }
                else
                {
                    await GlobalFunctions.UnlockPaywalletAsync(db, paywallet);
                    buildtransaction.LogFile += Environment.NewLine + s;
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"ReMint and burn was error: {newMintAndSendClass.NftProject.Projectname} - {newMintAndSendClass.NftProject.Id} {newMintAndSendClass.NftReservations.Count()}",
                        buildtransaction.LogFile, serverid);


                    // Release 
                    await SetNewMintandSendState(db, redis, newMintAndSendClass, "error", buildtransaction, serverid,
                        cancellationToken);
                }
            }


            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private async Task SetNewMintandSendState(EasynftprojectsContext db, IConnectionMultiplexer redis, NewMintAndSendClass newMintAndSendClass,
            string state, BuildTransactionClass buildtransaction, int serverid, CancellationToken cancellationToken)
        {
            foreach (var mas in newMintAndSendClass.MintAndSends)
            {
                await NftReservationClass.DeleteTokenAsync(db, mas.Reservationtoken);

                mas.State = state;
                mas.Executed = DateTime.Now;
                mas.Transactionid = buildtransaction.TxHash;
                mas.Buildtransaction = buildtransaction.LogFile;
                await db.SaveChangesAsync(cancellationToken);

                await SetOnlineNotification(db, state, mas);
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
