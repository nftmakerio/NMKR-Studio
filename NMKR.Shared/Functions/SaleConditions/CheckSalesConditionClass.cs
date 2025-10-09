using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.BITCOIN;
using NMKR.Shared.Blockchains.Cardano;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.SaleConditions
{
    public static class CheckSalesConditionClass
    {
        public static async Task<CheckConditionsResultClass> CheckForSaleConditionsMet(EasynftprojectsContext db, IConnectionMultiplexer redis, int addrNftprojectId, string receiveraddress, long countnft, int serverid, bool usefrankenprotection, Blockchain blockchain)
        {
            countnft = Math.Max(1, countnft);

            var res = new CheckConditionsResultClass() { ConditionsMet = true, SendBackAddress = new() { Address = receiveraddress, OriginatorAddress = receiveraddress, StakeAddress = "" } };

            res.StakeAddress = receiveraddress.ToLower().StartsWith("addr") && blockchain==Blockchain.Cardano ? Bech32Engine.GetStakeFromAddress(receiveraddress) : "";


            var conditions = await (from a in db.Nftprojectsaleconditions
                    .Include(a => a.Nftproject)
                    .AsSplitQuery()
                    .Include(a => a.Usedaddressesonsaleconditions)
                    .AsSplitQuery()
                                    where a.NftprojectId == addrNftprojectId && a.State == "active"
                                    select a).AsNoTracking().ToListAsync();

            if (!conditions.Any())
            {
                // If there are no conditions, just use always the originator address
                res.SendBackAddress.Address = receiveraddress;
                return res;
            }

            if (usefrankenprotection && receiveraddress.ToLower().StartsWith("addr") && blockchain==Blockchain.Cardano)
            {
                res.SendBackAddress =
                    await ConsoleCommand.GetFrankenAddressProtectionAddress(db, redis, receiveraddress);
            }
            else
            {
                res.SendBackAddress.Address = receiveraddress;
            }

            IBlockchainFunctions blockchainFunctions = null;
            switch (blockchain)
            {
                case Blockchain.Aptos:
                    blockchainFunctions = new AptosBlockchainFunctions();
                    break;
                case Blockchain.Solana:
                    blockchainFunctions = new SolanaBlockchainFunctions();
                    break;
                case Blockchain.Cardano:
                    blockchainFunctions = new CardanoBlockchainFunctions();
                    break;
                case Blockchain.Bitcoin:
                    blockchainFunctions = new BitcoinBlockchainFunctions();
                    break;
            }
            if (blockchainFunctions== null)
            {
                await GlobalFunctions.LogExceptionAsync(db,
                    $"Blockchain not found {blockchain}", "", serverid);
                return res;
            }


            res.AssetsAssociatedWithAccount = await blockchainFunctions.GetAllAssetsInWalletAsync(redis, receiveraddress);
            

            foreach (var condition in conditions)
            {
                try
                {
                    IEnumerable<Type> commands = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                        .Where(t => t.GetInterfaces().Contains(typeof(ISaleConditions)));
                    foreach (Type type in commands)
                    {
                        try
                        {
                            ISaleConditions command = (ISaleConditions) Activator.CreateInstance(type);
                            if (command == null)
                                continue;

                            res.ConditionsMet = command.CheckCondition(db, redis, condition,
                                receiveraddress,
                                res.StakeAddress,
                                addrNftprojectId,
                                countnft, res.AssetsAssociatedWithAccount, ref res);

                            if (res.ConditionsMet == false)
                            {
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            await GlobalFunctions.LogExceptionAsync(db,
                                $"Exception in CheckSalesConditionClass {e.Message} {condition.Id} {type.Name} {condition.Nftproject.Projectname} {condition.NftprojectId}",
                                e.StackTrace, serverid);
                        }

                    }

                    if (res.ConditionsMet == false)
                        break;
                }
                catch (Exception e)
                {
                    await GlobalFunctions.LogExceptionAsync(db,
                                               $"Exception in CheckSalesConditionClass {e.Message} {condition.Id} {condition.Nftproject.Projectname} {condition.NftprojectId}",
                                                                      e.StackTrace, serverid);
                }
            }

            await GlobalFunctions.LogMessageAsync(db,
                $"Conditions result - {res.ConditionsMet} {receiveraddress}", JsonConvert.SerializeObject(res.AssetsAssociatedWithAccount), serverid);

            return res;
        }
    }

}
