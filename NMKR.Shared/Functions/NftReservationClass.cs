using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Shared.Functions
{
    public static class NftReservationClass
    {
        public static async Task<List<Nftreservation>> ReserveRandomNft(EasynftprojectsContext db, IConnectionMultiplexer redis, string tokenUid,
            int projectId, long nftOrTokenCount, int reservationTime, bool isMintAndSend, bool doNotCheckForServer, Coin coin)
        {
            int? selectedServer = null;

            if (!doNotCheckForServer)
            {
                selectedServer = await GetNewFreeServer(db, isMintAndSend, coin);
                if (selectedServer == null)
                {
                    return new();
                }
            }

            var nftproject = await (from a in db.Nftprojects
                where a.Id == projectId
                select a).AsNoTracking().FirstOrDefaultAsync();
            if (nftproject == null)
                return new();


            long nftcount = nftproject.Maxsupply == 1 ? nftOrTokenCount : 1;
            long tokenCount = nftproject.Maxsupply == 1 ? 1 : nftOrTokenCount;


            // Check for blocked NFTS
            if (nftproject.Nftsblocked != null)
            {
                if (nftproject.Maxsupply == 1)
                {
                    if (nftproject.Free1 <= (long) nftproject.Nftsblocked)
                    {
                        return new();
                    }
                }
                else
                {
                    if (nftproject.Totaltokens1 - (nftproject.Tokenssold1 + nftproject.Tokensreserved1) <=
                        (long) nftproject.Nftsblocked)
                    {
                        return new();
                    }
                }
            }
            

            List<Nftreservation> resultList = new();
            //  do
            // {
            var nftid = (nftproject.Maxsupply == 1
                ? await ReserveSingleRandomNft(db, nftproject, reservationTime, nftcount, tokenUid)
                : await ReserveSingleRandomFt(db, nftproject, reservationTime, tokenCount, nftcount));

            if (nftid.Any())
            {
                foreach (var nft in nftid)
                {
                    // Check, if we have double reservations
                    if (nftproject.Maxsupply == 1)
                    {
                        // First look into the Redis DB
                        if (!string.IsNullOrEmpty(GlobalFunctions.GetStringFromRedis(redis, "nft_" + nft))) continue;

                        // And second look in the Mysql DB
                        var check2 = await (from a in db.Nftreservations
                            where a.NftId == nft 
                            select a).AsNoTracking().FirstOrDefaultAsync();

                        if (check2!=null)
                            continue;
                    }


                    var res1 = new Nftreservation()
                    {
                        Mintandsendcommand = isMintAndSend,
                        Multiplier = nftproject.Multiplier,
                        NftId = nft,
                        Reservationdate = DateTime.Now,
                        Reservationtime = reservationTime,
                        Serverid = selectedServer,
                        Reservationtoken = tokenUid,
                        Tc = tokenCount,
                    };
                    await db.Nftreservations.AddAsync(res1);
                    await db.SaveChangesAsync();
                    resultList.Add(res1);

                    if (nftproject.Maxsupply == 1)
                        GlobalFunctions.SaveStringToRedis(redis, "nft_" + nft, tokenUid, reservationTime * 60);
                }
            }

            // If the Count is equal to the nftcount, we are done
            if (resultList.Count == nftcount)
            {
                return resultList;
            }

            // Otherwise, we have to release all NFTS and try again
            await ReleaseAllNftsAsync(db,redis, tokenUid);

            return new();
        }
     
        private static async Task<List<int>> ReserveSingleRandomNft(EasynftprojectsContext db, Nftproject nftproject,
            int reservationTime, long nftcount, string restoken)
        {
            List<int> ntfx = new();
            string sql =
                $"update nfts set reservationtoken='{restoken}', state='reserved', reserveduntil = DATE_ADD(NOW(), INTERVAL {reservationTime} MINUTE), reservedcount = 1 where state='free' and nftproject_id={nftproject.Id} and mainnft_id is null and isroyaltytoken=0 ORDER BY rand() LIMIT {nftcount}";

            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql);

            ntfx = await (from a in db.Nfts
                where a.NftprojectId == nftproject.Id && a.Reservationtoken == restoken
                select a.Id).ToListAsync();

                if (ntfx.Count == nftcount || ntfx.Count <= 0) return ntfx;


                await GlobalFunctions.LogMessageAsync(db, "API-Call: NFTS reserved",
                    JsonConvert.SerializeObject(ntfx,
                        new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}) +
                    Environment.NewLine + sql);


                await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                    $"update nfts set reservationtoken=null,state='free',reservedcount=0 where nftproject_id={nftproject.Id} and reservationtoken='{restoken}' and state='reserved'");
                ntfx.Clear();

                return ntfx;
        }
        private static async Task<List<int>> ReserveSingleRandomFt(EasynftprojectsContext db, Nftproject nftproject,
        int reservationTime, long tokencount, long nftcount)
        {
            List<int> ntfx = new();

            var strategy = db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using (var dbContextTransaction = await db.Database.BeginTransactionAsync())
                {
                    // MUST BE RAND !!! NOT UID - because of FT Projects
                    var sqlselect= $"SELECT id, (soldcount+ reservedcount + errorcount) as dummyid FROM nfts where (soldcount + reservedcount + errorcount) <= ({nftproject.Maxsupply} - ({tokencount}*multiplier)) and state='free' and nftproject_id={nftproject.Id} and mainnft_id is null and isroyaltytoken=0 ORDER BY rand() LIMIT {nftcount}";

                    // We are using the updateprojectsid structure, not the table itself. But we only need the id, so this is ok.
                    var nftid = await db.Updateprojectsids.FromSqlRaw(sqlselect).ToListAsync();

                    int affected = 0;
                    if (nftid.Any())
                    {
                        foreach (var nft in nftid)
                        {
                            string sql = "";

                            sql = $"update nfts set reservedcount=reservedcount+({tokencount}*multiplier), reserveduntil = DATE_ADD(NOW(), INTERVAL {reservationTime} MINUTE) where id={nft.Id}";

                            var affectedRows = await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql);
                            affected += affectedRows;
                        }

                        if (nftproject.Maxsupply > 1 && affected == nftcount)
                        {
                            await dbContextTransaction.CommitAsync();
                        }
                        else
                        {
                            await dbContextTransaction.RollbackAsync();
                            nftid.Clear();
                        }
                    }

                    ntfx = (from a in nftid
                        select a.Id).ToList();
                }
            });
            return ntfx;
        }




        private static async Task<int?> GetNewFreeServer(EasynftprojectsContext db, bool isMintAndSend, Coin coin)
        {
            List<Backgroundserver> selectedServers=new ();
            if (isMintAndSend)
            {
                selectedServers = await (from a in db.Backgroundservers
                    where a.State == "active" && ((a.Checkmintandsend && coin == Coin.ADA) ||
                                                  (a.Checkmintandsendsolana && coin == Coin.SOL) ||
                                                  (a.Checkmintandsendcoin.Contains(coin.ToString())))
                    select a).AsNoTracking().ToListAsync();
            }
            else
            {
                selectedServers = await (from a in db.Backgroundservers
                    where a.State == "active" && ((a.Checkpaymentaddresses && coin == Coin.ADA) ||
                                                  (a.Checkpaymentaddressessolana && coin == Coin.SOL) ||
                                                  (a.Checkpaymentaddressescoin.Contains(coin.ToString())))
                    select a).AsNoTracking().ToListAsync();
            }

            if (selectedServers.Count == 0)
            {
                return null;
            }
            var selectedserver = selectedServers.MinBy(x => Guid.NewGuid());
            if (selectedserver != null)
                return selectedserver.Id;

            return null;
        }

        public static async Task<List<Nftreservation>> ReserveSpecificNft(EasynftprojectsContext db, IConnectionMultiplexer redis, string tokenUid,
            int projectId, ReserveNftsClass[] reserveNfts, int reservationTime, bool isMintAndSend, bool isDecentral, Coin coin)
        {
            int? selectedServer = null;

            if (!isDecentral)
            {
                selectedServer = await GetNewFreeServer(db, isMintAndSend, coin);
                if (selectedServer == null)
                {
                    return new();
                }
            }

            List<Nftreservation> resultList = new();

            var nftproject = await (from a in db.Nftprojects
                where a.Id == projectId
                select a).AsNoTracking().FirstOrDefaultAsync();
            if (nftproject == null)
                return resultList;



            // Check for blocked NFTS
            if (nftproject.Nftsblocked != null && !isMintAndSend)
            {
                if (nftproject.Maxsupply == 1)
                {
                    if (nftproject.Free1 <= (long)nftproject.Nftsblocked)
                    {
                        return new();
                    }
                }
                else
                {
                    if (nftproject.Totaltokens1 - (nftproject.Tokenssold1 + nftproject.Tokensreserved1) <=
                        (long)nftproject.Nftsblocked)
                    {
                        return new();
                    }
                }
            }



            foreach (var reserveNftsClass in reserveNfts)
            {
                    var nftid = await ReserveSingleSpecificNft(db, nftproject, reservationTime, reserveNftsClass, tokenUid);
                    if (nftid != null)
                    {
                        var res1 = new Nftreservation()
                        {
                            Mintandsendcommand = isMintAndSend,
                            NftId = (int) nftid,
                            Reservationdate = DateTime.Now,
                            Reservationtime = reservationTime,
                            Serverid = selectedServer,
                            Reservationtoken = tokenUid,
                            Tc = reserveNftsClass.Tokencount,
                            Multiplier = reserveNftsClass.Multiplier
                        };
                        await db.Nftreservations.AddAsync(res1);
                        resultList.Add(res1);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        return new();
                    }
            }

            if (resultList.Count == reserveNfts.Length)
            {
                return resultList;

            }

            await ReleaseAllNftsAsync(db, redis, tokenUid);

            return new();
        }

        private static async Task<int?> ReserveSingleSpecificNft(EasynftprojectsContext db, Nftproject nftproject, int reservationTime, ReserveNftsClass reserveNftsClass, string restoken)
        {
            int? nftx = null;

            var strategy = db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using (var dbContextTransaction = await db.Database.BeginTransactionAsync())
                {
                    string sql = "";
                    sql = nftproject.Maxsupply == 1
                        ? $"update nfts set reservationtoken='{restoken}',state='reserved', reserveduntil = DATE_ADD(NOW(), INTERVAL {reservationTime} MINUTE), reservedcount = 1 where id={reserveNftsClass.NftId} and nftproject_id={nftproject.Id} and mainnft_id is null and isroyaltytoken=0 and (state='free' or (state='burned' and ((soldcount-burncount) + reservedcount + errorcount) < {nftproject.Maxsupply}))"
                        : $"update nfts set reservedcount=reservedcount+({reserveNftsClass.Tokencount}*multiplier), reserveduntil = DATE_ADD(NOW(), INTERVAL {reservationTime} MINUTE) where id={reserveNftsClass.NftId} and nftproject_id={nftproject.Id} and state='free' and (reservedcount+errorcount+soldcount)<= ({nftproject.Maxsupply}*multiplier - ({reserveNftsClass.Tokencount}*multiplier))";

                    var affectedRows = await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql);
                    await dbContextTransaction.CommitAsync();

                    if (affectedRows == 1)
                    {
                        nftx= reserveNftsClass.NftId;
                    }
                }
            });

            return nftx;
        }


        public static async Task SetLogfileToNfts(EasynftprojectsContext db, string guid, string logFile)
        {
            var tokens = await (from a in db.Nftreservations
                    .Include(a => a.Nft)
                    .AsSplitQuery()
                where a.Reservationtoken == guid
                select a).ToListAsync();
            foreach (var nftreservation in tokens)
            {
                nftreservation.Nft.Buildtransaction = logFile;
            }

            await db.SaveChangesAsync();
        }

        public  static async Task MarkAllNftsAsError(EasynftprojectsContext db, IConnectionMultiplexer redis, string tokenUid, string errormessage)
        {
            var nftreservation = await (from a in db.Nftreservations
                    .Include(a => a.Nft).AsSplitQuery()
                                        where a.Reservationtoken == tokenUid
                select a).AsNoTracking().ToListAsync();

            if (nftreservation.Any())
            {
                var project = await (from a in db.Nftprojects
                    where a.Id == nftreservation.First().Nft.NftprojectId
                    select a).AsNoTracking().FirstOrDefaultAsync();

                if (project != null)
                {
                    var strategy = db.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        await using var dbContextTransaction = await db.Database.BeginTransactionAsync();
                        foreach (var reservation in nftreservation)
                        {
                            

                            string sql = project.Maxsupply == 1
                                ? $"UPDATE nfts set state='error', markedaserror=NOW(), reserveduntil=null, reservedcount=0, soldcount=0, errorcount=1  where id = {reservation.NftId}"
                                : $"UPDATE nfts set reservedcount=GREATEST(reservedcount - ({reservation.Tc}*multiplier), 0), errorcount=errorcount+({reservation.Tc}*multiplier) where id = {reservation.NftId}";

                            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql);

                        }

                        await dbContextTransaction.CommitAsync();
                    });
                }
            }
            await DeleteTokenAsync(db, tokenUid);
        }
        public  static async Task MarkAllNftsAsSold(EasynftprojectsContext db, string tokenUid, bool cip68, string receiveraddress, Blockchain blockchain=Blockchain.Cardano, string verifiedcollectionsolana = null, BuildTransactionClass buildtransaction = null)
        {
            var nftreservation = await (from a in db.Nftreservations
                    .Include(a => a.Nft).AsSplitQuery()
                                        where a.Reservationtoken == tokenUid
                select a).AsNoTracking().ToListAsync();

            if (nftreservation.Any())
            {
                var project = await (from a in db.Nftprojects
                    where a.Id == nftreservation.First().Nft.NftprojectId
                    select a).AsNoTracking().FirstOrDefaultAsync();

                string cipversion = cip68 ? "cip68" : "cip25";
                
                receiveraddress ??= "";


                if (project != null)
                {
                    var strategy = db.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        await using var dbContextTransaction = await db.Database.BeginTransactionAsync();
                        foreach (var reservation in nftreservation)
                        {

                            string coll = "";
                            if (!string.IsNullOrEmpty(verifiedcollectionsolana) && blockchain == Blockchain.Solana)
                                coll += $",verifiedcollectionsolana = '{verifiedcollectionsolana}'";

                            // Check if we have a solana asset address - saved in the buildtransaction
                            if (buildtransaction != null && buildtransaction.MintAssetAddress.Any())
                            {
                                var hash=buildtransaction.MintAssetAddress.FirstOrDefault(a => a.NftId == reservation.NftId)?.MintAddress;
                                if (!string.IsNullOrEmpty(hash) && blockchain == Blockchain.Solana)
                                    coll += $",solanatokenhash = '{hash}'";
                            }


                            string sql = project.Maxsupply == 1
                                ? $" UPDATE nfts set cipversion='{cipversion}', state='sold', reserveduntil=null, selldate=NOW(), instockpremintedaddress_id=null, " +
                                  $"reservedcount=0, soldcount=1, errorcount=0, receiveraddress='{receiveraddress}', mintedonblockchain='{blockchain}' {coll} where id = {reservation.NftId}"
                                : $" UPDATE nfts set cipversion='{cipversion}', checkpolicyid=1, lastpolicycheck=null, reservedcount=GREATEST(reservedcount - ({reservation.Tc}*multiplier), 0), soldcount=soldcount+({reservation.Tc}*multiplier) where id = {reservation.NftId}";

                            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql);

                            if (project.Maxsupply > 1)
                            {
                                await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                                    $"UPDATE nfts set cipversion='{cipversion}', state='sold', selldate=NOW() where id={reservation.NftId} and soldcount>={project.Maxsupply}");
                            }
                        }

                        await dbContextTransaction.CommitAsync();
                    });
                }
            }

            await DeleteTokenAsync(db, tokenUid);
        }
    
        public static async Task DeleteTokenAsync(EasynftprojectsContext db, string tokenUid)
        {
            string sqldelete = $"delete from nftreservations where reservationtoken='{tokenUid}'";
            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sqldelete);
        }


        public static async Task ReleaseAllNftsAsync(EasynftprojectsContext db, IConnectionMultiplexer redis, string tokenUid, int serverid=0, bool decreseRestime=false)
        {
            var nftreservation = await (from a in db.Nftreservations
                    .Include(a=>a.Nft).AsSplitQuery()
                                        where a.Reservationtoken == tokenUid
                select a).AsNoTracking().ToListAsync();

            if (nftreservation.Any())
            {
                var project = await (from a in db.Nftprojects
                    where a.Id == nftreservation.First().Nft.NftprojectId
                    select a).AsNoTracking().FirstOrDefaultAsync();

                if (project != null)
                {
                    var strategy = db.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        await using var dbContextTransaction = await db.Database.BeginTransactionAsync();
                        foreach (var reservation in nftreservation)
                        {
                                if (project.Maxsupply == 1)
                                {
                                    var res = GlobalFunctions.GetStringFromRedis(redis, "nft_" + reservation.NftId);
                                    if (string.IsNullOrEmpty(res) || res.Contains(tokenUid))
                                    {
                                        if (decreseRestime)
                                        {
                                            string sql =
                                                $"update nftreservations set reservationdate=DATE_SUB(NOW(), INTERVAL 69 MINUTE) where reservationtoken='{tokenUid}'";
                                            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql,
                                                serverid);
                                        }
                                        else
                                        {
                                            string sql =
                                                $"update nfts set state='free', reservationtoken='', reserveduntil=null, reservedcount=0 where id={reservation.NftId} and state='reserved'";
                                            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql,
                                                serverid);
                                            GlobalFunctions.DeleteStringFromRedis(redis, "nft_" + reservation.NftId);
                                        }
                                    }
                                    else
                                    {
                                        await GlobalFunctions.LogMessageAsync(db, "Not found for release", tokenUid + "- "+reservation.NftId);
                                    }
                                }
                                else
                                {
                                    string sql= $"update nfts set state='free', reservationtoken='', reservedcount=GREATEST(reservedcount - ({reservation.Tc}*multiplier), 0) where id = {reservation.NftId} and state='free'";
                                    await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql,  serverid);
                                }
                            
                        }

                        await dbContextTransaction.CommitAsync();
                    });

                    if (!decreseRestime)
                        await DeleteTokenAsync(db, tokenUid);
                }
            }

            
        }
    }
}
