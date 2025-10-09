using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.BITCOIN;
using NMKR.Shared.Blockchains.Solana;
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
    public class CheckMintAndSendOrders : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter,
            Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.mintandsend;

            if (string.IsNullOrEmpty(server.Checkmintandsendcoin))
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);

            foreach (var coin in server.Checkmintandsendcoin.Trim()
                         .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).OrEmptyIfNull())
            {
                await CheckMintAndSendsAsync(db, cancellationToken, mainnet, serverid, redis, coin.ToEnum<Coin>());
            }

            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet,
                serverid,
                redis);
        }

        private async Task CheckMintAndSendsAsync(EasynftprojectsContext db, CancellationToken cancellationToken, bool mainnet, int serverid, IConnectionMultiplexer redis, Coin coin)
        {
        var mas = await (from a in db.Mintandsends
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Settings)
                    .AsSplitQuery()
                    .Include(a => a.Customer)
                    .AsSplitQuery()
                             where a.State == "execute" &&
                                   a.Remintandburn == false && a.Coin == coin.ToString()
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
                        .ThenInclude(a => a.Nftproject)
                        .ThenInclude(a => a.Aptoscustomerwallet)
                                             where a.Reservationtoken == mas2.Reservationtoken && a.Mintandsendcommand == true
                                             select a).ToListAsync(cancellationToken);

                foreach (var res in nftreservations)
                {

                    // Last Bastion - check if the asset is known on blockfrost or the aptos blockchain
                    if (StaticBackgroundServerClass.FoundNftInBlockchain(res.Nft, mas2.Nftproject,
                            mainnet, out var bfq, out var resultjson))
                    {
                        BuildTransactionClass bt = new BuildTransactionClass($"{coin.ToString()} - Mint & Send - Found NFT in Blockchain - skipped");
                        await SetNewMintandSendState(db, redis, mas2, "error", bt,coin,
                            cancellationToken);
                        await SetNftStates(db, redis, "error", cancellationToken, mas2, bt, coin);
                        continue;
                    }


                    if (coin==Coin.APT) {
                        if (res.Nft.Nftproject.Aptoscollectiontransaction == "<PENDING>" ||
                            res.Nft.Nftproject.Aptoscollectiontransaction == null)
                        {
                            await StaticBackgroundServerClass.LogAsync(db,
                                $"Mint and Send - Collection is not yet created: {mas2.Nftproject.Projectname} - {mas2.Nftproject.Id} - {res.Nft.Nftproject.Aptoscollectiontransaction}",
                                "", serverid);
                            continue;
                        }
                    }

                    // Then get paywallet
                    var paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db, serverid, $"CheckMintAndSendOrders{coin.ToString()}", res.Reservationtoken, coin);
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
                            await SetNewMintandSendState(db, redis, mas2, "error", new BuildTransactionClass($"{coin.ToString()} - Mint & Send - Found NFT in Redis as already minted - skipped"),coin,
                                cancellationToken);
                            await GlobalFunctions.UnlockPaywalletAsync(db, paywallet);
                            continue;
                        }
                    }



                    // Mint
                    BuildTransactionClass buildtransaction = new BuildTransactionClass();

                    IBlockchainFunctions blockchainFunctions = null;
                    switch (coin)
                    {
                        case Coin.SOL:
                            blockchainFunctions = new SolanaBlockchainFunctions();
                            buildtransaction.MetadataStandard = "solana";
                            break;
                        case Coin.APT:
                            blockchainFunctions = new AptosBlockchainFunctions();
                            buildtransaction.MetadataStandard = "aptos";
                            break;
                        case Coin.BTC:
                            blockchainFunctions = new BitcoinBlockchainFunctions();
                            buildtransaction.MetadataStandard = "bitcoin";
                            break;
                    }

                    if (blockchainFunctions == null)
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Mint and Send - Coin {coin} is not supported: {mas2.Nftproject.Projectname} - {mas2.Nftproject.Id}",
                            "", serverid);
                        continue;
                    }



                    buildtransaction = await blockchainFunctions.MintAndSend(res.Nft, res.Nft.Nftproject, mas2.Receiveraddress,
                        blockchainFunctions.ConvertToBlockchainKeysClass(paywallet), buildtransaction);


                    bool ok = coin == Coin.APT && buildtransaction.MintAssetAddress.Any();
                    if (coin == Coin.BTC && !string.IsNullOrEmpty(buildtransaction.TxHash) &&
                        string.IsNullOrEmpty(buildtransaction.ErrorMessage))
                        ok = true;


                    // Check for successful transactions
                    if (ok)
                    {
                        // Save Transaction
                        await StaticBackgroundServerClass.SaveTransactionToDatabase(db, redis, buildtransaction,
                            mas2.Nftproject.CustomerId, null, mas2.Nftproject.Id,
                            TransactionTypes.mintfromcustomeraddress, null,
                            new[] { res }, serverid, coin);

                        await SetNewMintandSendState(db, redis, mas2, "success", buildtransaction,coin, cancellationToken);

                        if (mas2.Usecustomerwallet == null ||
                            mas2.Usecustomerwallet == true)
                            await GlobalFunctions.ReduceMintCouponsAsync(db, mas2.Nftproject.CustomerId, mas2.Requiredcoupons);


                        // Solana and Aptos are very fast - so we can release the paywallet immediately
                        // But Bitcoin needs some time to confirm the transaction
                        if (coin == Coin.APT || coin == Coin.SOL)
                        {
                            paywallet.Addressblocked = false;
                            await db.SaveChangesAsync(cancellationToken);
                        }
                    }
                    else
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Mint and send was error - releasing the Tokens: {mas2.Nftproject.Projectname} - {mas2.Nftproject.Id}  {paywallet.Address}",
                            buildtransaction.LogFile, serverid);
                        // Release 
                        await SetNewMintandSendState(db, redis, mas2, "error", buildtransaction,coin,
                            cancellationToken);

                        paywallet.Addressblocked = false;
                        await db.SaveChangesAsync(cancellationToken);
                    }


                }


             
            }
        }

        private async Task SetNewMintandSendState(EasynftprojectsContext db, IConnectionMultiplexer redis,
            Mintandsend mas,
            string state, BuildTransactionClass buildtransaction,Coin coin, CancellationToken cancellationToken)
        {
            mas.State = state;
            mas.Executed = DateTime.Now;
            mas.Transactionid = buildtransaction.TxHash;
            mas.Buildtransaction = buildtransaction.LogFile;
            await db.SaveChangesAsync(cancellationToken);

            await SetOnlineNotification(db, state, mas);
            await SetNftStates(db, redis, state, cancellationToken, mas, buildtransaction, coin);
        }

        private static async Task SetNftStates(EasynftprojectsContext db, IConnectionMultiplexer redis, string state,
            CancellationToken cancellationToken, Mintandsend mas,  BuildTransactionClass buildTransaction, Coin coin)
        {
            if (state == "success")
            {
                try
                {
                    await NftReservationClass.MarkAllNftsAsSold(db, mas.Reservationtoken, false, mas.Receiveraddress,GlobalFunctions.ConvertToBlockchain(coin), null,buildTransaction);
                }
                catch
                {
                    await Task.Delay(1000, cancellationToken);
                    GlobalFunctions.ResetContextState(db);

                    // Sometime this does not work
                    await Task.Delay(1000, cancellationToken);
                    await NftReservationClass.MarkAllNftsAsSold(db, mas.Reservationtoken, false, mas.Receiveraddress, GlobalFunctions.ConvertToBlockchain(coin), null, buildTransaction);
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
