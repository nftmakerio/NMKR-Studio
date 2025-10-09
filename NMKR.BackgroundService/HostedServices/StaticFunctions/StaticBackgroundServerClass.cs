using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.Cardano;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.StaticFunctions
{
    public static class StaticBackgroundServerClass
    {
        internal static IConnectionMultiplexer _redis;
        internal static ILogger _iLogger;
        internal static IBus _bus;

        internal static async Task UpdateActualRunnningTaskAsync(BackgroundTaskEnums task, bool mainnet, int serverid, IConnectionMultiplexer redis, int? projectid = null)
        {
            BackgroundServerTasksClass bc = new()
            {
                ActualProjectId = projectid,
                Task = task,
                LastlifeSign = DateTime.Now
            };
            await SaveToRedisAsync($"BackgroundServer{(mainnet ? "_Mainnet_" : "_Testnet_")}{serverid}", JsonConvert.SerializeObject(bc), new(0, 1, 0, 0));
        }
        internal static void SaveToRedis(string key, string parameter, TimeSpan expire)
        {
            IDatabase dbr = _redis.GetDatabase();
            dbr.StringSet(key, parameter, expiry: expire);
        }

        private static async Task SaveToRedisAsync(string key, string parameter, TimeSpan expire)
        {
            IDatabase dbr = _redis.GetDatabase();
            await dbr.StringSetAsync(key, parameter, expiry: expire);
        }
        internal static async Task<bool> CheckStopServerAsync(EasynftprojectsContext db, int serverid, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return true;

            var bg = await (from a in db.Backgroundservers
                where a.Id == serverid
                select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (bg == null)
                return false;
            return bg.Stopserver || bg.Pauseserver;
        }
        internal static async Task LogAsync(EasynftprojectsContext db, string message, string data = "", int serverid = 0)
        {
            await GlobalFunctions.LogMessageAsync(db, message, data, serverid);
            
            _iLogger.LogInformation(DateTime.Now.ToShortDateString() + " " +
                                    DateTime.Now.ToLongTimeString() +
                                    " " + message);
        }
        internal static async Task EventLogException(EasynftprojectsContext db, int no, Exception e, int serverid)
        {
            string stacktrace = "";
            if (e.StackTrace != null)
                stacktrace = e.StackTrace;
            stacktrace += " - ";
            if (e.InnerException != null && e.InnerException.StackTrace != null)
                stacktrace += e.InnerException.StackTrace;

            _iLogger.LogCritical(e, e.Message, new {StackTrace = stacktrace});

            await LogAsync(db, $"Exception {no}: {e.Message}", (e.InnerException != null ? e.InnerException.Message + stacktrace : stacktrace), serverid);
            await GlobalFunctions.LogExceptionAsync(db, $"Exception{no}: {e.Message}", (e.InnerException != null ? e.InnerException.Message + stacktrace : stacktrace), serverid);
        }
        internal static async Task UpdateCustomerLastCheckForUtxo(EasynftprojectsContext db, int customerid, int serverid, Coin coin)
        {
            switch (coin)
            {
                case Coin.SOL:
                    await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, $"update customers set sollastcheckforutxo=now() where id={customerid}", serverid);
                    break;
                case Coin.ADA:
                    await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, $"update customers set lastcheckforutxo=now() where id={customerid}", serverid);
                    break;
                case Coin.APT:
                    await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, $"update customers set aptlastcheckforutxo=now() where id={customerid}", serverid);
                    break;
            }
        }

        internal static async Task<long> CheckRestPrice(EasynftprojectsContext db, Nftproject project, long countnft, long adaamount, string receiveradddress, string reservationtoken, Coin coin, string customerproperty)
        {

            // Catch Additional Payout Wallets from the Database
            var additionalPayouts = await (from a in db.Nftprojectsadditionalpayouts
                    .Include(a => a.Wallet)
                    .AsSplitQuery()
                where a.NftprojectId == project.Id
                      && a.Coin == coin.ToString()
                      && (a.Custompropertycondition == null || a.Custompropertycondition == "" || a.Custompropertycondition==customerproperty)
                select a).AsNoTracking().ToArrayAsync();



            var mintingcosts = GlobalFunctions.GetMintingcosts2( project.Id, countnft, adaamount);
            var minutxo = 2000000;
            // Calculate the Minuxto - all 6 NFT we will Add 2 ADA - i know, it is not correct, but it works
            long minutxofinal = mintingcosts.MinUtxo;
            int ux = 0;
            long rest = adaamount;
            rest -= mintingcosts.Costs;

            if (project.Minutxo == nameof(MinUtxoTypes.twoadaeverynft))
            {
                minutxofinal = minutxo * countnft;
            }

            if (project.Minutxo == nameof(MinUtxoTypes.twoadaall5nft))
            {
                for (int i = 0; i < countnft; i++)
                {
                    ux++;
                    if (ux >= 5)
                    {
                        ux = 0;
                        minutxofinal += minutxo;
                    }
                }
            }


            if (project.Minutxo == nameof(MinUtxoTypes.minutxo))
            {
                List<Nftreservation> selectedNfts = await (from a in db.Nftreservations
                        .Include(a => a.Nft)
                        .AsSplitQuery()
                                                           where a.Reservationtoken == reservationtoken
                                                           select a).AsNoTracking().ToListAsync();


                string guid = GlobalFunctions.GetGuid();
                BuildTransactionClass buildtransaction = new();
                string sendtoken = "";
                foreach (var nft in selectedNfts)
                {
                    if (!string.IsNullOrEmpty(sendtoken))
                        sendtoken += " + ";
                    sendtoken += nft.Tc + " " + project.Policyid + "." +
                                 ConsoleCommand.CreateMintTokenname(project.Tokennameprefix,
                                     nft.Nft.Name);
                }
                minutxofinal = ConsoleCommand.CalculateRequiredMinUtxo(_redis,receiveradddress, sendtoken,"", guid, GlobalFunctions.IsMainnet(), ref buildtransaction);
            }


            rest -= minutxofinal;

            // TODO: Discounts


            foreach (var nftprojectsadditionalpayout in additionalPayouts.OrEmptyIfNull())
            {
                long addvalue =
                    ConsoleCommand.GetAdditionalPayoutwalletsValue(nftprojectsadditionalpayout, adaamount, countnft);
                if (addvalue > 0)
                {
                    rest -= addvalue;
                }
            }

            return rest;
        }

        internal static bool FoundNftInBlockchain(Nft nft1, Nftproject project, bool mainnet, out long quantity,
            out object resultjson)
        {
            quantity = 0;
            resultjson = null;

            try
            {

                if (CheckIfRedisKeyExists($"{(mainnet ? "mainnet_" : "testnet_")}nft_{nft1.Id}", out var value))
                {
                    return true;
                }

                if (project.Enabledcoins.Contains(Coin.ADA.ToString()) == true || project.Enabledcoins == null)
                {
                    IBlockchainFunctions blockchain = new CardanoBlockchainFunctions();
                    var as1 = blockchain.GetAsset(project, nft1);
                    if (as1 != null)
                    {
                        resultjson = as1;
                        if (as1.Quantity != null) quantity = (long) as1.Quantity;
                        return true;
                    }
                }

                if (project.Enabledcoins.Contains(Coin.SOL.ToString()) && !string.IsNullOrEmpty(nft1.Solanatokenhash))
                {
                    IBlockchainFunctions blockchain = new SolanaBlockchainFunctions();
                    var as1 = blockchain.GetAsset(project, nft1);
                    if (as1 != null)
                    {
                        resultjson = as1;
                        if (as1.Quantity != null) quantity = (long)as1.Quantity;
                        return true;
                    }
                }

                if (project.Enabledcoins.Contains(Coin.APT.ToString()))
                {
                    IBlockchainFunctions blockchain = new AptosBlockchainFunctions();
                    var as1 = blockchain.GetAsset(project, nft1);
                    if (as1 != null)
                    {
                        resultjson = as1;
                        if (as1.Quantity != null) quantity = (long)as1.Quantity;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                GlobalFunctions.LogException(null, "Error in FoundNftInBlockchain",
                    nft1.Id + Environment.NewLine +project.Policyid+" - "+ (project.Tokennameprefix??"NULL")+" - "+
                    nft1.Name + Environment.NewLine+e.Message);
            }

            return false;
    }
        private static bool CheckIfRedisKeyExists(string key, out string value)
        {
            value = null;
            IDatabase db = _redis.GetDatabase();
            var res = db.StringGet(key);
            // var a=db.ListLeftPop(new RedisKey("ddd"),10, CommandFlags.None);
            if (!res.IsNull)
                value = res.ToString();
            return !res.IsNull;
        }
        internal static void SaveMarkAsSoldToRedis(ICollection<Nfttonftaddress> addrNfttonftaddresses, bool mainnet)
        {
            foreach (var ad in addrNfttonftaddresses)
            {
                SaveToRedis($"{(mainnet ? "mainnet_" : "testnet_")}nft_{ad.NftId}", "sold", new(4, 0, 0));
            }
        }



        internal static async Task SaveTransactionToDatabase(EasynftprojectsContext db, IConnectionMultiplexer redis, BuildTransactionClass buildtransaction, 
            int? customerId, int? nftaddressid, int? nftprojectid, string transactiontype, int? walletId, int serverid, Coin coin, 
            long milliseconds=0, Nftaddress addr = null, CheckConditionsResultClass cond = null, PromotionClass promotion=null, 
            string cbor=null, string ipaddress=null, List<Nftreservation> selectedreservations=null, string customproperty=null, string discountrefererstring=null)
        {
            await LogAsync(db,
                $"Save Transaction {transactiontype} - {buildtransaction.SenderAddress} - {buildtransaction.TxHash}", buildtransaction.LogFile, serverid);
            if (buildtransaction.BuyerTxOut == null)
                return;

            int nftcount = transactiontype == "burning" ? 1 : addr?.Nfttonftaddresses?.Count ?? 0;
            if (selectedreservations!=null)
                nftcount=selectedreservations.Count;

            long? priceintokensmultiplier = null;
            if (buildtransaction.PriceInTokens != null)
            {
                var pt= await KoiosFunctions.GetFtTokensMultiplierAsync(buildtransaction.PriceInTokens?.PolicyId,
                        buildtransaction.PriceInTokens?.TokennameHex);
                    if (pt!=null)
                        priceintokensmultiplier=pt.Multiplier;
            }


            try
            {
                var rates=await GlobalFunctions.GetNewRatesAsync(redis, coin);
                Transaction t = new()
                {
                    Senderaddress = buildtransaction.SenderAddress,
                    Receiveraddress = buildtransaction.BuyerTxOut.ReceiverAddress,
                    Originatoraddress = cond?.SendBackAddress.OriginatorAddress,
                    Stakeaddress = (string.IsNullOrEmpty(cond?.SendBackAddress.StakeAddress) && coin==Coin.ADA) ? Bech32Engine.GetStakeFromAddress(buildtransaction.BuyerTxOut.ReceiverAddress) : cond?.SendBackAddress.StakeAddress,
                    Ada = buildtransaction.BuyerTxOut.Amount,
                    Stakereward = buildtransaction.StakeRewards ?? 0,
                    Tokenreward = buildtransaction.TokenRewards??0,
                    Discount = buildtransaction.Discount ?? 0,
                    Created = DateTime.Now,
                    CustomerId = customerId,
                    NftaddressId = nftaddressid,
                    NftprojectId = nftprojectid,
                    Transactiontype = transactiontype,
                    Transactionid = buildtransaction.TxHash,
                    Fee = buildtransaction.Fees,
                    Projectaddress = buildtransaction.ProjectTxOut?.ReceiverAddress,
                    Projectada = transactiontype == nameof(TransactionTypes.mintfromcustomeraddress) ? 0 : buildtransaction.ProjectTxOut?.Amount,
                    Mintingcostsaddress = buildtransaction.MintingcostsTxOut?.ReceiverAddress,
                    Mintingcostsada = buildtransaction.MintingcostsTxOut?.Amount ?? 0,
                    WalletId = walletId,
                    State = "submitted",
                    Ipaddress = ipaddress,
                    Eurorate = (float)rates.EurRate,
                    Nmkrcosts = buildtransaction.NmkrCosts,
                    Serverid = serverid,
                    Cbor = cbor,
                    Confirmed = coin==Coin.APT ? true: false,
                    Paymentmethod = coin.ToString(), // TODO: Add Payment Method when FIAT etc
                    PreparedpaymenttransactionId = addr?.PreparedpaymenttransactionsId,
                    Nftcount = nftcount,
                    Telemetrytooktime = milliseconds,
                    Priceintokensquantity = buildtransaction.PriceInTokens?.Quantity,
                    Priceintokenspolicyid = buildtransaction.PriceInTokens?.PolicyId,
                    Priceintokenstokennamehex = buildtransaction.PriceInTokens?.TokennameHex,
                    Priceintokensmultiplier = priceintokensmultiplier,
                    Coin = coin.ToString(),
                    Customerproperty = customproperty,
                    Discountcode = discountrefererstring,
                    Projectincomingtxhash = addr?.Txid,
                    Incomingtxblockchaintime = await GetBlockchainTime(addr?.Txid),
                    Metadatastandard=buildtransaction.MetadataStandard,
                    Cip68referencetokenminutxo= buildtransaction.Cip68ReferenceTokenTxOut?.Amount,
                    Cip68referencetokenaddress = buildtransaction.Cip68ReferenceTokenTxOut?.ReceiverAddress,
                };

                // Save Referer
                if (addr is {Referer.State: "active"})
                {
                    t.RefererId = addr.Referer.Id;
                    t.RefererCommission = (long)(t.Projectada / 100 * addr.Referer.Commisionpercent);
                }

                await db.AddAsync(t);

                if (customerId != null)
                {
                    // Check the customer internal address - to prevent too many tx in
                    var customer = await (from a in db.Customers
                                          where a.Id == customerId
                                          select a).FirstOrDefaultAsync();
                    if (customer != null)
                        customer.Checkaddresscount = 100;
                }

                await db.SaveChangesAsync();

                if (addr != null)
                {
                    int i = 0;
                    foreach (var addrNfttonftaddress in addr.Nfttonftaddresses.OrEmptyIfNull())
                    {
                        var txhash = buildtransaction.TxHash;
                        if (buildtransaction.MintAssetAddress.Any())
                        {
                            if (coin==Coin.SOL)
                                txhash=buildtransaction.MintAssetAddress.FirstOrDefault(x=>x.NftId== addrNfttonftaddress.NftId)?.MintAddress;
                            if (coin==Coin.APT)
                                txhash = buildtransaction.MintAssetAddress.FirstOrDefault(x => x.NftId == addrNfttonftaddress.NftId)?.MintTxHash;
                            if (coin== Coin.ADA)
                                txhash = buildtransaction.MintAssetAddress.FirstOrDefault(x => x.NftId == addrNfttonftaddress.NftId)?.TxHash;
                        }

                        await db.TransactionNfts.AddAsync(new()
                        {
                            NftId = addrNfttonftaddress.NftId,
                            TransactionId = t.Id,
                            Mintedontransaction = !addrNfttonftaddress.Nft.Minted,
                            Tokencount = addrNfttonftaddress.Tokencount,
                            Multiplier = addrNfttonftaddress.Nft.Multiplier,
                            Ispromotion = false,
                            Txhash=txhash
                        });
                        i++;
                        await db.SaveChangesAsync();
                    }
                }
                else
                {
                    foreach (var sn in selectedreservations.OrEmptyIfNull())
                    {
                        var txhash = buildtransaction.TxHash;
                        if (buildtransaction.MintAssetAddress.Any())
                        {
                            txhash = buildtransaction.MintAssetAddress.FirstOrDefault(x => x.NftId == sn.NftId)?.MintAddress;
                        }

                        int i = 0;
                        await db.TransactionNfts.AddAsync(new()
                        {
                            NftId = sn.NftId,
                            TransactionId = t.Id,
                            Mintedontransaction = true,
                            Tokencount = sn.Tc,
                            Multiplier = await GlobalFunctions.GetNftMultiplierAsync(db, sn.NftId),
                            Ispromotion = false,
                            Txhash = txhash
                        });
                        i++;
                        await db.SaveChangesAsync();
                    }
                }



                if (promotion != null)
                {
                    var txhash = buildtransaction.TxHash;
                    if (buildtransaction.MintAssetAddress.Any())
                    {
                        txhash = buildtransaction.MintAssetAddress.FirstOrDefault(x => x.NftId == promotion.PromotionNft.Id)?.MintAddress;
                    }
                    await db.TransactionNfts.AddAsync(new()
                    {
                        NftId = promotion.PromotionNft.Id,
                        TransactionId = t.Id,
                        Mintedontransaction = true,
                        Tokencount = promotion.Tokencount,
                        Multiplier = promotion.PromotionNft.Multiplier,
                        Ispromotion = true,
                        Txhash = txhash
                    });
                    await db.SaveChangesAsync();
                }


                // Save Additional Payouts
                foreach (var addp in buildtransaction.AdditionalPayouts.OrEmptyIfNull())
                {
                    await db.TransactionsAdditionalpayouts.AddAsync(new TransactionsAdditionalpayout()
                    {
                        TransactionId = t.Id, Lovelace = addp.Valuetotal ?? 0,
                        Payoutaddress = addp.Wallet.Walletaddress,
                    });
                    await db.SaveChangesAsync();
                }


                // Save Statistics

                if (transactiontype == nameof(TransactionTypes.paidonftaddress))
                {
                    var stat = await (from a in db.Statistics
                                      where a.CustomerId == customerId && a.NftprojectId == nftprojectid
                                                                       && a.Day == t.Created.Day && a.Month == t.Created.Month &&
                                                                       a.Year == t.Created.Year
                                      select a).FirstOrDefaultAsync();


                    long amount = 0;
                    long mintingcosts = 0;
                    if (buildtransaction.ProjectTxOut != null)
                        amount += buildtransaction.ProjectTxOut.Amount;
                    if (buildtransaction.MintingcostsTxOut != null)
                    {
                        amount += buildtransaction.MintingcostsTxOut.Amount;
                        mintingcosts += buildtransaction.MintingcostsTxOut.Amount;
                    }

                    amount += buildtransaction.BuyerTxOut.Amount;
                    amount += buildtransaction.Fees;


                    if (stat == null)
                    {
                        await db.Statistics.AddAsync(new()
                        {
                            CustomerId = (int)customerId,
                            NftprojectId = (int)nftprojectid,
                            Day = t.Created.Day,
                            Month = t.Created.Month,
                            Year = t.Created.Year,
                            Amount = amount,
                            Mintingcosts = mintingcosts,
                            Minutxo = buildtransaction.BuyerTxOut.Amount,
                            Sales = 1,
                            Transactionfees = buildtransaction.Fees
                        });
                    }
                    else
                    {
                        stat.Sales++;
                        stat.Amount += amount;
                        stat.Mintingcosts += mintingcosts;
                        stat.Minutxo += buildtransaction.BuyerTxOut.Amount;
                        stat.Transactionfees += buildtransaction.Fees;
                    }
                    await db.SaveChangesAsync();
                    // End save Statistics

                }
            }
            catch (Exception e)
            {
                GlobalFunctions.ResetContextState(db);
                await EventLogException(db, 6, e, serverid);
            }
            await LogAsync(db,
                $"Transaction saved - {transactiontype} - {buildtransaction.SenderAddress}", "", serverid);
        }

        private static async Task<long?> GetBlockchainTime(string addrTxid)
        {
            if (string.IsNullOrEmpty(addrTxid))
                return null;

            var txinfobf = await ConsoleCommand.GetTransactionAsync(addrTxid);


            if (txinfobf != null)
            {
                DateTime currentTime = DateTime.UtcNow;
                long unixTime = ((DateTimeOffset) currentTime).ToUnixTimeSeconds();
                var txdate = txinfobf.BlockTime == null
                    ? unixTime
                    : txinfobf.BlockTime;

                return txdate;
            }

            return null;
        }


        internal static async Task SaveTransactionToDatabase(EasynftprojectsContext db, IConnectionMultiplexer redis, BuildTransactionClass buildtransaction,
        int? customerId, int? nftaddressid, int? nftprojectid, TransactionTypes transactiontype, int? walletId,
        Nftreservation[] reservationtokens, int serverid, Coin coin)
        {
            var restok = (from a in reservationtokens
                    select a.Reservationtoken
                ).ToList();
            int countnfts=0;
            var res1 = restok.Distinct().ToList();
            foreach (var reservationtoken in res1)
            {
                if (!string.IsNullOrEmpty(reservationtoken))
                {
                    countnfts+= await (from a in db.Nftreservations
                        where a.Reservationtoken == reservationtoken
                        select a).CountAsync();
                }
            }


            Transaction t = await SaveTransaction(db,redis, buildtransaction, customerId, nftaddressid, nftprojectid,
                transactiontype, walletId, serverid,countnfts, coin);
            if (t == null)
                return;

          

            foreach (var reservationtoken in res1)
            {
                if (!string.IsNullOrEmpty(reservationtoken))
                {
                    var nfts = await (from a in db.Nftreservations
                                      where a.Reservationtoken == reservationtoken
                                      select a).ToListAsync();

                    foreach (var nftreservation in nfts)
                    {
                        string txhash = buildtransaction.TxHash;
                        if (buildtransaction != null && buildtransaction.MintAssetAddress.Any())
                        {
                            txhash=buildtransaction.MintAssetAddress.FirstOrDefault(x=>x.NftId==nftreservation.NftId)?.MintAddress;
                        }
                        await db.TransactionNfts.AddAsync(new()
                        {
                            NftId = nftreservation.NftId,
                            TransactionId = t.Id,
                            Mintedontransaction = true,
                            Tokencount = nftreservation.Tc,
                            Multiplier = await GlobalFunctions.GetNftMultiplierAsync(db, nftreservation.NftId),
                            Ispromotion = false, 
                            Txhash = txhash
                        });

                    }
                }
            }


            await db.SaveChangesAsync();


            // Save Statistics

            if (transactiontype == TransactionTypes.paidonftaddress)
            {
                var stat = await (from a in db.Statistics
                                  where a.CustomerId == customerId && a.NftprojectId == nftprojectid
                                                                   && a.Day == t.Created.Day && a.Month == t.Created.Month &&
                                                                   a.Year == t.Created.Year
                                  select a).FirstOrDefaultAsync();


                long amount = 0;
                long mintingcosts = 0;
                if (buildtransaction.ProjectTxOut != null)
                    amount += buildtransaction.ProjectTxOut.Amount;
                if (buildtransaction.MintingcostsTxOut != null)
                {
                    amount += buildtransaction.MintingcostsTxOut.Amount;
                    mintingcosts += buildtransaction.MintingcostsTxOut.Amount;
                }

                amount += buildtransaction.BuyerTxOut.Amount;
                amount += buildtransaction.Fees;


                if (stat == null)
                {
                    await db.Statistics.AddAsync(new()
                    {
                        CustomerId = (int)customerId,
                        NftprojectId = (int)nftprojectid,
                        Day = t.Created.Day,
                        Month = t.Created.Month,
                        Year = t.Created.Year,
                        Amount = amount,
                        Mintingcosts = mintingcosts,
                        Minutxo = buildtransaction.BuyerTxOut.Amount,
                        Sales = 1,
                        Transactionfees = buildtransaction.Fees
                    });
                }
                else
                {
                    stat.Sales++;
                    stat.Amount += amount;
                    stat.Mintingcosts += mintingcosts;
                    stat.Minutxo += buildtransaction.BuyerTxOut.Amount;
                    stat.Transactionfees += buildtransaction.Fees;
                }

                await db.SaveChangesAsync();
                // End save Statistics

            }

            await LogAsync(db,
                $"Transaction saved - {transactiontype} - {buildtransaction.SenderAddress}", "", serverid);
        }


        internal static async Task<Transaction> SaveTransactionToDatabase(EasynftprojectsContext db,IConnectionMultiplexer redis, BuildTransactionClass buildtransaction,
            int? customerId, int? nftprojectid, TransactionTypes transactiontype, int? walletId,
            PreparedpaymenttransactionsNft[] nfts, int serverid, Coin coin)
        {
            Transaction t = await SaveTransaction(db,redis, buildtransaction, customerId, null, nftprojectid,
                transactiontype, walletId, serverid, nfts?.Length ?? 0, coin);
            if (t == null)
                return null;


            foreach (var nftreservation in nfts.OrEmptyIfNull())
            {
                await db.TransactionNfts.AddAsync(new()
                {
                    NftId = nftreservation.NftId,
                    TransactionId = t.Id,
                    Mintedontransaction = true,
                    Tokencount = nftreservation.Count,
                    Multiplier = await GlobalFunctions.GetNftMultiplierAsync(db, (int)nftreservation.NftId),
                    Ispromotion = false
                });
            }

            await db.SaveChangesAsync();

            await LogAsync(db,
                $"Transaction saved - {transactiontype} - {buildtransaction.SenderAddress}", "", serverid);
            return t;
        }

        private static async Task<Transaction> SaveTransaction(EasynftprojectsContext db,IConnectionMultiplexer redis, BuildTransactionClass buildtransaction,
          int? customerId, int? nftaddressid, int? nftprojectid, TransactionTypes transactiontype, int? walletId, int serverid, int nftcount, Coin coin)
        {
            await LogAsync(db,
                $"Save Transaction {transactiontype} - {buildtransaction.SenderAddress} - {buildtransaction.TxHash}",
                buildtransaction.LogFile + Environment.NewLine+ (buildtransaction.LockTxIn!=null ?JsonConvert.SerializeObject(buildtransaction.LockTxIn):""), serverid);
            if (buildtransaction.BuyerTxOut == null)
                return null;

            var rates=GlobalFunctions.GetNewRates(redis, Coin.APT);

            try
            {
                Transaction t = new()
                {
                    Senderaddress = buildtransaction.SenderAddress,
                    Receiveraddress = buildtransaction.BuyerTxOut.ReceiverAddress,
                    Ada = buildtransaction.BuyerTxOut.Amount,
                    Nmkrcosts = transactiontype is TransactionTypes.mintfromcustomeraddress or TransactionTypes.mintfromnftmakeraddress ? (buildtransaction.BuyerTxOut.Amount + buildtransaction.Fees) : 0,
                    Stakereward = buildtransaction.StakeRewards??0,
                    Tokenreward = buildtransaction.TokenRewards??0,
                    Discount = buildtransaction.Discount??0,
                    Created = DateTime.Now,
                    CustomerId = customerId,
                    NftaddressId = nftaddressid,
                    NftprojectId = nftprojectid,
                    Transactiontype = transactiontype.ToString(),
                    Transactionid = buildtransaction.TxHash,
                    Fee = buildtransaction.Fees,
                    Projectaddress = buildtransaction.ProjectTxOut?.ReceiverAddress,
                    Projectada = transactiontype is TransactionTypes.mintfromcustomeraddress or TransactionTypes.mintfromnftmakeraddress ? 0 : buildtransaction.ProjectTxOut?.Amount,
                    Mintingcostsaddress = buildtransaction.MintingcostsTxOut?.ReceiverAddress,
                    Mintingcostsada = transactiontype is TransactionTypes.mintfromnftmakeraddress ? 0 : buildtransaction.MintingcostsTxOut?.Amount??0,
                    WalletId = walletId,
                    State = "submitted",
                    Eurorate = (float)rates.EurRate,
                    Serverid = serverid,
                    Nftcount = nftcount,
                    Stakeaddress = "",
                    Coin = coin.ToString(),
                    Metadatastandard = buildtransaction.MetadataStandard
                };
                await db.AddAsync(t);

                if (customerId != null)
                {
                    // Check the customer internal address - to prevent too many tx in
                    var customer = await (from a in db.Customers
                                          where a.Id == customerId
                                          select a).FirstOrDefaultAsync();
                    if (customer != null)
                        customer.Checkaddresscount = 100;
                }

                await db.SaveChangesAsync();
                return t;
            }
            catch (Exception e)
            {
                GlobalFunctions.ResetContextState(db);
                await EventLogException(db, 6, e, serverid);
            }

            return null;
        }


    }
}
