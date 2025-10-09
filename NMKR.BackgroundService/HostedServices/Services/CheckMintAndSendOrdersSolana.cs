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
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckMintAndSendOrdersSolana : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.mintandsend;
            if (server.Checkmintandsendsolana == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);

            var mas = await (from a in db.Mintandsends
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Settings)
                    .AsSplitQuery()
                    .Include(a => a.Customer)
                    .AsSplitQuery()
                where a.State == "execute" &&
                      /* a.Usecustomerwallet == true && */a.Remintandburn == false && a.Coin == Coin.SOL.ToString()
                select a).Take(20).ToListAsync(cancellationToken);

            foreach (var mas2 in mas)
            {
                var nftreservations = await (from a in db.Nftreservations
                        .Include(a => a.Nft)
                        .ThenInclude(a => a.InverseMainnft)
                        .ThenInclude(a => a.Metadata)
                        .AsSplitQuery()
                        .Include(a => a.Nft)
                        .ThenInclude(a => a.Metadata)
                        .AsSplitQuery()
                        .Include(a => a.Nft)
                        .ThenInclude(a=>a.Nftproject)
                        .ThenInclude(a=>a.Solanacustomerwallet)
                    where a.Reservationtoken == mas2.Reservationtoken && a.Mintandsendcommand == true
                    select a).ToListAsync(cancellationToken);

                foreach (var res in nftreservations)
                {

                    // Last Bastion - check if the asset is known on blockfrost or the solana blockchain
                    if (StaticBackgroundServerClass.FoundNftInBlockchain(res.Nft, mas2.Nftproject,
                            mainnet, out var bfq, out var resultjson))
                    {
                        BuildTransactionClass bt = new BuildTransactionClass("SOLANA - Mint & Send - Found NFT in Blockchain - skipped");
                        await SetNewMintandSendState(db, redis, mas2, "error", bt,
                            cancellationToken);
                        await SetNftStates(db, redis, "error", cancellationToken, mas2, "",bt);
                        continue;
                    }

                    if (res.Nft.Nftproject.Aptoscollectiontransaction == "<PENDING>")
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Mint and Send - Collection is not yet created: {mas2.Nftproject.Projectname} - {mas2.Nftproject.Id} - {res.Nft.Nftproject.Aptoscollectiontransaction}",
                            "", serverid);
                        continue;
                    }



                    // Then get paywallet
                    var paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db, serverid, "CheckMintAndSendOrdersSolana",res.Reservationtoken,Coin.SOL);
                    if (paywallet == null)
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Mint and Send - All Paywallets are blocked - waiting: {mas2.Nftproject.Projectname} - {mas2.Nftproject.Id}",
                            "", serverid);
                        continue;
                    }

                    // Block the nft in redis 
                    bool found = false;
                    if (mas2.Nftproject.Maxsupply == 1)
                    {
                        found = !string.IsNullOrEmpty(
                            GlobalFunctions.GetStringFromRedis(redis, $"MintAndSend_{res.NftId}"));
                        GlobalFunctions.SaveStringToRedis(redis, $"MintAndSend_{res.NftId}",
                            $"MintAndSend_{res.NftId}", 360);
                        if (found)
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"FAILED - MintNfts - NFT was already sold - found in Redis - NftId: {res.NftId} - {res.Tc} - {bfq} - {mas2.Nftproject.Maxsupply}",
                                ".", serverid);
                            await SetNewMintandSendState(db, redis, mas2, "error", new BuildTransactionClass("SOLANA - Mint & Send - Found NFT in Redis as already minted - skipped"),
                                cancellationToken);
                            await GlobalFunctions.UnlockPaywalletAsync(db,paywallet);
                            continue;
                        }
                    }

                

                    // Mint
                    BuildTransactionClass buildtransaction = new BuildTransactionClass();
                    buildtransaction.MetadataStandard = "solana";
                    buildtransaction = await SolanaFunctions.MintAndSendCoreAsync(res.Nft, res.Nft.Nftproject, mas2.Receiveraddress,
                        SolanaFunctions.GetWallet(paywallet), buildtransaction);

                    // Check for successful transactions
                    if (buildtransaction.MintAssetAddress.Any())
                    {
                        // Save Transaction
                        await StaticBackgroundServerClass.SaveTransactionToDatabase(db, redis, buildtransaction,
                            mas2.Nftproject.CustomerId, null, mas2.Nftproject.Id,
                            TransactionTypes.mintfromcustomeraddress, null,
                            new[] {res}, serverid, Coin.SOL);

                        await SetNewMintandSendState(db, redis, mas2, "success", buildtransaction, cancellationToken);

                        if (mas2.Usecustomerwallet == null ||
                            mas2.Usecustomerwallet == true)
                            await GlobalFunctions.ReduceMintCouponsAsync(db, mas2.Nftproject.CustomerId, 1);

                        paywallet.Addressblocked = false;
                        await db.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Mint and send was error - releasing the Tokens: {mas2.Nftproject.Projectname} - {mas2.Nftproject.Id}  {paywallet.Address}",
                            buildtransaction.LogFile, serverid);
                        // Release 
                        await SetNewMintandSendState(db, redis, mas2, "error", buildtransaction,
                            cancellationToken);

                        paywallet.Addressblocked = false;
                        await db.SaveChangesAsync(cancellationToken);
                    }


                }


                // Reset the Display for the Admintool
                await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet,
                    serverid,
                    redis);
            }
        }

        private async Task SetNewMintandSendState(EasynftprojectsContext db, IConnectionMultiplexer redis,
            Mintandsend mas,
            string state, BuildTransactionClass buildtransaction, CancellationToken cancellationToken)
        {
            mas.State = state;
            mas.Executed = DateTime.Now;
            mas.Transactionid = buildtransaction.TxHash;
            mas.Buildtransaction = buildtransaction.LogFile;
            await db.SaveChangesAsync(cancellationToken);

            await SetOnlineNotification(db, state, mas);
            await SetNftStates(db, redis, state, cancellationToken, mas, !string.IsNullOrEmpty(mas.Nftproject.Solanacollectiontransaction) ? "mustbeadded" : "nocollection", buildtransaction);
        }

        private static async Task SetNftStates(EasynftprojectsContext db, IConnectionMultiplexer redis, string state,
            CancellationToken cancellationToken, Mintandsend mas, string verifiedcollectionsolana, BuildTransactionClass buildTransaction)
        {
            if (state == "success")
            {
                try
                {
                    await NftReservationClass.MarkAllNftsAsSold(db, mas.Reservationtoken, false, mas.Receiveraddress, Blockchain.Solana, verifiedcollectionsolana, buildTransaction);
                }
                catch
                {
                    await Task.Delay(1000, cancellationToken);
                    GlobalFunctions.ResetContextState(db);

                    // Sometime this does not work
                    await Task.Delay(1000, cancellationToken);
                    await NftReservationClass.MarkAllNftsAsSold(db, mas.Reservationtoken, false, mas.Receiveraddress, Blockchain.Solana, verifiedcollectionsolana, buildTransaction);
                }
            }
            else
            {
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
