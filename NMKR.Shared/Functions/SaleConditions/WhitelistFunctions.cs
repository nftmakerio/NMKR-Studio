using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Shared.Functions.SaleConditions
{
    public static class WhitelistFunctions
    {
        /// <summary>
        /// When a whitelist is used as a salecondition, we will check, if we can use them just one time. If yes, we will add the whitelisted address to a second list, so that we will not use this address again
        /// </summary>
        /// <param name="db"></param>
        /// <param name="project"></param>
        /// <param name="receiveraddress"></param>
        /// <returns></returns>
        public static async Task<int?[]> SaveUsedAddressesToWhitelistSaleCondition(EasynftprojectsContext db, int projectid,
            string receiveraddress, string originatoraddress, string stakeaddress, string txid, long countnft)
        {
            List<int?> usedid = new();
            // Old Whitelist
            var salecondition = await (from a in db.Nftprojectsaleconditions
                                       where a.NftprojectId == projectid &&
                                             a.Condition == nameof(SaleConditionsTypes.whitlistedaddresses) &&
                                             a.State == "active"
                                       select a).AsNoTracking().ToListAsync();

            if (salecondition.Any())
            {
                foreach (var nftprojectsalecondition in salecondition)
                {
                    int saleconditionid = nftprojectsalecondition.Id;

                    await SaveUsedAddress(db, receiveraddress, saleconditionid);
                    await SaveUsedAddress(db, stakeaddress, saleconditionid);
                    await SaveUsedAddress(db, originatoraddress, saleconditionid);
                }
            }


            // New counted whitelist

            var salecondition2 = await (from a in db.Nftprojectsaleconditions
                                        where a.NftprojectId == projectid &&
                                              a.Condition == nameof(SaleConditionsTypes.countedwhitelistedaddresses) &&
                                              a.State == "active"
                                        select a).AsNoTracking().ToListAsync();

            if (salecondition2.Any())
            {
                foreach (var countedWhitelists in salecondition2.Select(nftprojectsalecondition => nftprojectsalecondition.Id).Select(saleconditionid => (from a in db.Countedwhitelists
                             where a.SaleconditionsId == saleconditionid
                                   && (a.Address == receiveraddress || a.Address == originatoraddress || (a.Stakeaddress == stakeaddress && !string.IsNullOrEmpty(stakeaddress)))
                             select a).AsNoTracking().FirstOrDefault()))
                {
                    if (countedWhitelists != null)
                        usedid.Add(await SaveUsedAddress2(db, originatoraddress ?? receiveraddress, receiveraddress, countedWhitelists.Id, txid,
                            countnft));
                    else
                    {
                        await GlobalFunctions.LogExceptionAsync(db, "Could not save used address - Address not found in Whitelist",
                            originatoraddress + Environment.NewLine + receiveraddress + Environment.NewLine + txid +
                            Environment.NewLine + stakeaddress + Environment.NewLine + countnft + Environment.NewLine);
                    }
                }
            }

            return usedid.ToArray();
        }

        private static async Task<int?> SaveUsedAddress2(EasynftprojectsContext db, string senderaddress,
            string receiveraddress, int cwlid, string txid, long countnft)
        {
            try
            {
                if (!string.IsNullOrEmpty(receiveraddress))
                {
                    senderaddress ??= receiveraddress;


                    Countedwhitelistusedaddress cwl = new()
                    {
                        CountedwhitelistId = cwlid,
                        Countnft = countnft,
                        Transactionid = txid,
                        Originatoraddress = senderaddress,
                        Usedaddress = receiveraddress,
                        Created = DateTime.Now
                    };

                    await db.AddAsync(cwl);
                    await db.SaveChangesAsync();
                    return cwl.Id;
                }
            }
            catch (Exception e)
            {

                await GlobalFunctions.LogExceptionAsync(db, "Could not save used address - Whitelist",
                    senderaddress + Environment.NewLine + receiveraddress + Environment.NewLine + txid +
                    Environment.NewLine + cwlid + Environment.NewLine + countnft + Environment.NewLine + e.Message);
            }

            return null;
        }


        private static async Task SaveUsedAddress(EasynftprojectsContext db, string receiveraddress, int saleconditionid)
        {
            if (!string.IsNullOrEmpty(receiveraddress))
            {
                var t = await (from a in db.Usedaddressesonsaleconditions
                               where a.SalecondtionsId == saleconditionid && a.Address == receiveraddress
                               select a).AsNoTracking().FirstOrDefaultAsync();

                if (t != null)
                    return;

                try
                {
                    Usedaddressesonsalecondition uasc = new()
                    { Address = receiveraddress, SalecondtionsId = saleconditionid };
                    await db.Usedaddressesonsaleconditions.AddAsync(uasc);
                    await db.SaveChangesAsync();
                }
                catch
                {
                    // This can happen, if a address is double - we prevent it from mysql
                }
            }
        }

        public static async Task DeleteUsedAddressesToWhitelistSaleCondition(EasynftprojectsContext db, int?[] usedid)
        {
            foreach (var id in usedid)
            {
                if (id != null && id != 0)
                {
                    await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                        "delete from countedwhitelistusedaddresses where id=" + id);
                }
            }
        }

        public static async Task UpdateUsedAddressesToWhitelistSaleCondition(EasynftprojectsContext db, int?[] usedid, string btTxId)
        {
            foreach (var id in usedid)
            {
                if (id != null && id != 0)
                {
                    await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                        $"update countedwhitelistusedaddresses set transactionid='{btTxId}' where id=" + id);
                }
            }
        }
    }
}
