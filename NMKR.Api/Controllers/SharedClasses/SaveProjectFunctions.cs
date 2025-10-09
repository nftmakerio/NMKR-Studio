using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Functions.Extensions;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Api.Controllers.SharedClasses
{
    public class SaveProjectFunctions
    {
        public ApiErrorResultClass CheckPricelists(EasynftprojectsContext db, PricelistClassV2[] pricelist, Customer customer, ApiErrorResultClass result)
        {
            if (pricelist == null)
                return result;

            List<long> countnft = new List<long>();
            foreach (var pricelistClassV2 in pricelist)
            {
                long mincosts = GlobalFunctions.CalculateMinutxoNew(customer, pricelistClassV2.CountNft);

                if (pricelistClassV2.PriceInLovelace < mincosts)
                {
                    result.ErrorCode = 128;
                    result.ErrorMessage = "Pricelist: The minimum price for " + pricelistClassV2.CountNft + " NFT is " +
                                          mincosts + " lovelace";
                    result.ResultState = ResultStates.Error;
                    return result;
                }

                if (pricelistClassV2.ValidFrom != null && pricelistClassV2.ValidTo != null &&
                    pricelistClassV2.ValidFrom > pricelistClassV2.ValidTo)
                {
                    result.ErrorCode = 129;
                    result.ErrorMessage = "Pricelist: 'Validfrom' date can not after 'Validto'  date";
                    result.ResultState = ResultStates.Error;
                    return result;
                }

                if (countnft.Exists(x=>x==pricelistClassV2.CountNft))
                {
                    result.ErrorCode = 141;
                    result.ErrorMessage = $"Pricelist: the same amount of CountNft already exists (Amount: {pricelistClassV2.CountNft})";
                    result.ResultState = ResultStates.Error;
                    return result;
                }
                countnft.Add(pricelistClassV2.CountNft);
            }

            return result;
        }

        public async Task SavePricelists(EasynftprojectsContext db, PricelistClassV2[] pricelist, long maxNftSupply, int proId, int? defaultpromotionid)
        {
            if (pricelist == null)
                return;


            foreach (var pricelistClassV2 in pricelist)
            {
                int multiplier = 1;
                if (maxNftSupply == 1)
                    multiplier = (int)pricelistClassV2.CountNft;

                var p = GetPrice(pricelistClassV2);
                Pricelist pl = new()
                {
                    Countnftortoken = pricelistClassV2.CountNft,
                    NftprojectId = proId,
                    Priceinlovelace = p.Price,
                    Currency = p.Currency,
                    State = pricelistClassV2.IsActive ? "active" : "notactive",
                    Validfrom = pricelistClassV2.ValidFrom,
                    Validto = pricelistClassV2.ValidTo,
                    PromotionId = defaultpromotionid,
                    Promotionmultiplier = multiplier
                };
               
                await db.Pricelists.AddAsync(pl);
                await db.SaveChangesAsync();
            }
        }

        private GetPriceClass GetPrice(PricelistClassV2 pricelistClassV2)
        {
            GetPriceClass p = new() {Currency = Coin.ADA.ToString(), Price = 0};
            if (pricelistClassV2.Price != null)
            {
                if (pricelistClassV2.Currency == Coin.USD || pricelistClassV2.Currency==Coin.EUR || pricelistClassV2.Currency==Coin.JPY)
                {
                    p.Price = (long) (pricelistClassV2.Price * 100f);
                }
                if (pricelistClassV2.Currency == Coin.ADA)
                {
                    p.Price = (long)(pricelistClassV2.Price * 1000000f);
                }
                if (pricelistClassV2.Currency == Coin.SOL)
                {
                    p.Price = (long)(pricelistClassV2.Price * 1000000000f);
                }
                if (pricelistClassV2.Currency == Coin.APT)
                {
                    p.Price = (long)(pricelistClassV2.Price * 100000000f);
                }

                p.Currency = pricelistClassV2.Currency.ToString();
                return p;
            }

            if (pricelistClassV2.PriceInLovelace != null)
                p.Price = (long)pricelistClassV2.PriceInLovelace;

            return p;
        }

        public ApiErrorResultClass CheckSaleConditions(EasynftprojectsContext db, SaleconditionsClassV2[] saleconditions, ApiErrorResultClass result)
        {
            if (saleconditions == null)
                return result;

            foreach (var saleconditionsClassV2 in saleconditions)
            {
                if (saleconditionsClassV2.Condition == SaleConditionsTypes.whitlistedaddresses)
                {
                    result.ErrorCode = 135;
                    result.ErrorMessage = "Salecondition: Whitelist ist deprecated. Use CountedWhitelist instead";
                    result.ResultState = ResultStates.Error;
                    return result;
                }


                if (saleconditionsClassV2.Condition == SaleConditionsTypes.walletdoescontainmaxpolicyid &&
                    saleconditionsClassV2.MinOrMaxValue == null)
                {
                    result.ErrorCode = 130;
                    result.ErrorMessage = "Salecondition: MinMax Value must be specified on the Condition " + saleconditionsClassV2.Condition;
                    result.ResultState = ResultStates.Error;
                    return result;
                }
                if (saleconditionsClassV2.Condition == SaleConditionsTypes.walletdoescontainmaxpolicyid &&
                    saleconditionsClassV2.MinOrMaxValue == null)
                {
                    result.ErrorCode = 131;
                    result.ErrorMessage = "Salecondition: MinMax Value must be specified on the Condition " + saleconditionsClassV2.Condition;
                    result.ResultState = ResultStates.Error;
                    return result;
                }

                if (saleconditionsClassV2.Condition != SaleConditionsTypes.whitlistedaddresses && saleconditionsClassV2.Condition != SaleConditionsTypes.blacklistedaddresses && saleconditionsClassV2.Condition!=SaleConditionsTypes.countedwhitelistedaddresses)
                {
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId1, 1, out var checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId2,2, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId3, 3, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId4, 4, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId5, 5, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId6, 6, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId7, 7, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId8, 8, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId9, 9, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId10, 10, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId11, 11, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId12, 12, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId13, 13, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId14, 14, out checkSaleConditions)) return checkSaleConditions;
                    if (CheckPolicyId(result, saleconditionsClassV2, saleconditionsClassV2.PolicyId15, 15, out checkSaleConditions)) return checkSaleConditions;

                }

            }

            return result;
        }

        private static bool CheckPolicyId(ApiErrorResultClass result, SaleconditionsClassV2 saleconditionsClassV2, string policyid, int no,
            out ApiErrorResultClass checkSaleConditions)
        {
            if (!string.IsNullOrEmpty(policyid) &&
                policyid.Length != 56)
            {
                result.ErrorCode = 133;
                result.ErrorMessage = "Salecondition:Policy Id {no} is not correct on Condition " +
                                      saleconditionsClassV2.Condition;
                result.ResultState = ResultStates.Error;
                checkSaleConditions = result;
                return true;
            }
            checkSaleConditions = result;
            return false;
        }

        public ApiErrorResultClass CheckDiscounts(EasynftprojectsContext db, PriceDiscountClassV2[] discounts, ApiErrorResultClass result)
        {
            if (discounts == null)
                return result;

            foreach (var discountsClassV2 in discounts)
            {

                if (discountsClassV2.Condition == PricelistDiscountTypes.walletcontainsminofpolicyid)
                {
                    if (discountsClassV2.Minvalue == 0 || discountsClassV2.Minvalue == null)
                    {
                        result.ErrorCode = 136;
                        result.ErrorMessage = "Discounts: MinValue must be specified on the Discount " + discountsClassV2.Condition;
                        result.ResultState = ResultStates.Error;
                        return result;
                    }

                    if (discountsClassV2.Operator != "AND" && discountsClassV2.Operator != "OR")
                    {
                        result.ErrorCode = 159;
                        result.ErrorMessage = "Discounts: Operator must be AND or OR on Discount " + discountsClassV2.Condition;
                        result.ResultState = ResultStates.Error;
                        return result;
                    }


                    if (string.IsNullOrEmpty(discountsClassV2.PolicyIdOrStakeAddress1))
                    {
                        result.ErrorCode = 132;
                        result.ErrorMessage = "Discounts:Policy Id not specified on Discount " +
                                              discountsClassV2.Condition;
                        result.ResultState = ResultStates.Error;
                        return result;
                    }

                    if (discountsClassV2.PolicyIdOrStakeAddress1.Length != 56)
                    {
                        result.ErrorCode = 133;
                        result.ErrorMessage = "Discounts:Policy Id 1 is not correct on Discount " +
                                              discountsClassV2.Condition;
                        result.ResultState = ResultStates.Error;
                        return result;
                    }

                    if (!string.IsNullOrEmpty(discountsClassV2.PolicyIdOrStakeAddress2) &&
                        discountsClassV2.PolicyIdOrStakeAddress2.Length != 56)
                    {
                        result.ErrorCode = 133;
                        result.ErrorMessage = "Discounts:Policy Id 2 is not correct on Discount " +
                                              discountsClassV2.Condition;
                        result.ResultState = ResultStates.Error;
                        return result;
                    }

                    if (!string.IsNullOrEmpty(discountsClassV2.PolicyIdOrStakeAddress3) &&
                        discountsClassV2.PolicyIdOrStakeAddress3.Length != 56)
                    {
                        result.ErrorCode = 133;
                        result.ErrorMessage = "Discounts:Policy Id 3 is not correct on Discount " +
                                              discountsClassV2.Condition;
                        result.ResultState = ResultStates.Error;
                        return result;
                    }

                    if (!string.IsNullOrEmpty(discountsClassV2.PolicyIdOrStakeAddress4) &&
                        discountsClassV2.PolicyIdOrStakeAddress4.Length != 56)
                    {
                        result.ErrorCode = 133;
                        result.ErrorMessage = "Discounts:Policy Id 4 is not correct on Discount " +
                                              discountsClassV2.Condition;
                        result.ResultState = ResultStates.Error;
                        return result;
                    }

                    if (!string.IsNullOrEmpty(discountsClassV2.PolicyIdOrStakeAddress5) &&
                        discountsClassV2.PolicyIdOrStakeAddress5.Length != 56)
                    {
                        result.ErrorCode = 133;
                        result.ErrorMessage = "Discounts:Policy Id 5 is not correct on Discount " +
                                              discountsClassV2.Condition;
                        result.ResultState = ResultStates.Error;
                        return result;
                    }
                }

                if (discountsClassV2.Condition == PricelistDiscountTypes.couponcode)
                {
                    if (string.IsNullOrEmpty(discountsClassV2.Couponcode))
                    {
                        result.ErrorCode = 136;
                        result.ErrorMessage = "Discounts: Couponcode must be specified on the Discount " + discountsClassV2.Condition;
                        result.ResultState = ResultStates.Error;
                        return result;
                    }
                }
            }

            return result;
        }

        internal async Task SaveSaleConditions(EasynftprojectsContext db, SaleconditionsClassV2[] saleconditions, int proId)
        {
            if (saleconditions == null)
                return;
            foreach (var saleconditionsClassV2 in saleconditions)
            {
                Nftprojectsalecondition sc = new()
                {
                    Description = saleconditionsClassV2.Description,
                    Policyid = saleconditionsClassV2.PolicyId1 ?? "",
                    Policyid2 = saleconditionsClassV2.PolicyId2,
                    Policyid3 = saleconditionsClassV2.PolicyId3,
                    Policyid4 = saleconditionsClassV2.PolicyId4,
                    Policyid5 = saleconditionsClassV2.PolicyId5,
                    Policyid6 = saleconditionsClassV2.PolicyId6,
                    Policyid7 = saleconditionsClassV2.PolicyId7,
                    Policyid8 = saleconditionsClassV2.PolicyId8,
                    Policyid9 = saleconditionsClassV2.PolicyId9,
                    Policyid10 = saleconditionsClassV2.PolicyId10,
                    Policyid11 = saleconditionsClassV2.PolicyId11,
                    Policyid12 = saleconditionsClassV2.PolicyId12,
                    Policyid13 = saleconditionsClassV2.PolicyId13,
                    Policyid14 = saleconditionsClassV2.PolicyId14,
                    Policyid15 = saleconditionsClassV2.PolicyId15,
                    Maxvalue = saleconditionsClassV2.MinOrMaxValue,
                  //  Whitlistaddresses = string.Join(Environment.NewLine, saleconditionsClassV2.WhitelistedAddresses),
                    Blacklistedaddresses = saleconditionsClassV2.BlacklistedAddresses ==null ? null : string.Join(Environment.NewLine, saleconditionsClassV2.BlacklistedAddresses),
                    NftprojectId = proId,
                    State = saleconditionsClassV2.IsActive ? "active" : "notactive",
                    Policyprojectname = saleconditionsClassV2.PolicyProjectname,
                 //   Onlyonesaleperwhitlistaddress = saleconditionsClassV2.OnlyOneSalePerWhitelistAddress ?? false,
                 Onlyonesaleperwhitlistaddress = false,
                    Condition = saleconditionsClassV2.Condition.ToString()
                };

                await db.Nftprojectsaleconditions.AddAsync(sc);
                await db.SaveChangesAsync();


                // Save counted whitelist
                if (saleconditionsClassV2.Condition == SaleConditionsTypes.countedwhitelistedaddresses && saleconditionsClassV2.CountedWhitelistAddresses!=null)
                {
                    foreach (var countedWhitelistAddressesClass in saleconditionsClassV2.CountedWhitelistAddresses)
                    {
                        if (ConsoleCommand.CheckIfAddressIsValid(db, countedWhitelistAddressesClass.Address,
                                GlobalFunctions.IsMainnet(), out string adaaddress, out Blockchain blockchain, true, true))
                        {
                            await db.Countedwhitelists.AddAsync(new()
                            {
                                Address = adaaddress,
                                SaleconditionsId = sc.Id,
                                Created = DateTime.Now,
                                Maxcount = countedWhitelistAddressesClass.MaxCount,
                                Stakeaddress = Bech32Engine.GetStakeFromAddress(adaaddress)
                            });
                            await db.SaveChangesAsync();
                        }
                    }
                }

            }
        }

        internal async Task SaveDiscounts(EasynftprojectsContext db, PriceDiscountClassV2[] discounts, int projectid)
        {
            if (discounts == null)
                return;
            foreach (var discountClass in discounts)
            {
                Pricelistdiscount sc = new()
                {
                    Description = discountClass.Description,
                    Policyid = discountClass.PolicyIdOrStakeAddress1 ?? "",
                    Policyid2 = discountClass.PolicyIdOrStakeAddress2,
                    Policyid3 = discountClass.PolicyIdOrStakeAddress3,
                    Policyid4 = discountClass.PolicyIdOrStakeAddress4,
                    Policyid5 = discountClass.PolicyIdOrStakeAddress5,
                    Minvalue = discountClass.Minvalue,
                    Sendbackdiscount=discountClass.SendbackDiscount,
                    Whitlistaddresses = discountClass.WhitelistedAddresses==null? null : string.Join(Environment.NewLine, discountClass.WhitelistedAddresses),
                    NftprojectId = projectid,
                    State = discountClass.IsActive ? "active" : "notactive",
                    Policyprojectname = discountClass.PolicyProjectname,
                    Condition = discountClass.Condition.ToString(),
                    Operator = discountClass.Operator,
                    Minvalue2 = discountClass.Minvalue2,
                    Minvalue3 = discountClass.Minvalue3,
                    Minvalue4 = discountClass.Minvalue4,
                    Minvalue5 = discountClass.Minvalue5,  
                    Couponcode = discountClass.Couponcode,
                };

                await db.Pricelistdiscounts.AddAsync(sc);
                await db.SaveChangesAsync();
            }
        }

        internal ApiErrorResultClass CheckNotifications(EasynftprojectsContext db, NotificationsClassV2[] notifications, ApiErrorResultClass result)
        {
            if (notifications == null)
                return result;

            foreach (var noti in notifications)
            {

                if (noti.NotificationType == PaymentTransactionNotificationTypes.email &&
                   !GlobalFunctions.IsValidEmail(noti.Address))
                {
                    result.ErrorCode = 436;
                    result.ErrorMessage = "Notifications: Address is not a valid email address";
                    result.ResultState = ResultStates.Error;
                    return result;
                }


                if (noti.NotificationType == PaymentTransactionNotificationTypes.webhook &&
                    !noti.Address.IsValidUrl())
                {
                        result.ErrorCode = 432;
                        result.ErrorMessage = "Notifications: Address is not a valid url";
                        result.ResultState = ResultStates.Error;
                        return result;
                }
            }

            return result;
        }

        internal async Task SaveNotifications(EasynftprojectsContext db, NotificationsClassV2[] notifications, int projectId)
        {
            if (notifications == null)
                return;

            foreach (var noti in notifications)
            {
                Notification noti1 = new Notification
                {
                    State = noti.IsActive ? "active" : "notactive",
                    Secret = GlobalFunctions.GetGuid(),
                    Address = noti.Address,
                    NftprojectId = projectId,
                    Notificationtype = noti.NotificationType.ToString()
                };

                await db.Notifications.AddAsync(noti1);
                await db.SaveChangesAsync();
            }
        }

        internal async Task<GetNotificationsClass[]> GetNotifications(EasynftprojectsContext db, string projectUid)
        {
            var project = await (from a in db.Nftprojects
                        .Include(a=>a.Notifications)
                    where a.Uid == projectUid
                    select a
                ).AsNoTracking().FirstOrDefaultAsync();

            if (project == null)
                return null;

            if (project.Notifications == null || !project.Notifications.Any())
                return null;

            var res = (from a in project.Notifications
                select new GetNotificationsClass
                {
                    Address = a.Address, IsActive = a.State == "active",
                    NotificationType = a.Notificationtype.ToEnum<PaymentTransactionNotificationTypes>(),
                    secret = a.Secret
                }).ToArray();
            return res;
        }
    }
}
