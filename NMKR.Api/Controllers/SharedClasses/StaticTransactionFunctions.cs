using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CardanoSharp.Wallet.CIPs.CIP2;
using CardanoSharp.Wallet.CIPs.CIP2.ChangeCreationStrategies;
using CardanoSharp.Wallet.CIPs.CIP2.Models;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.TransactionBuilding;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.CustodialWallets;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Blockfrost;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Model;
using NMKR.Shared.SmartContracts;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.SharedClasses
{
   

    public static class StaticTransactionFunctions
    {
        public static PaymentTransactionResultClass GetTransactionState(EasynftprojectsContext db,IConnectionMultiplexer redis,
            string transactionuid, bool sendnotification, bool sendcbor = false)
        {
            var preparedpaymenttransaction = GetPreparedPaymentTransactionFromDatabase(db, transactionuid);

            if (preparedpaymenttransaction == null)
                return null;

            if (preparedpaymenttransaction.ReferencedprepearedtransactionId == null)
                return GetTransactionState(db,redis, preparedpaymenttransaction, transactionuid, sendnotification, null,
                    sendcbor);

            // If there is a referenced transaction (only available in nmkr_pay_random or nmkr_pay_specfic) we will send out the referenced transaction state

            var rpreparedpaymenttransaction =
                GetPreparedPaymentTransactionFromDatabase(db,
                    preparedpaymenttransaction.Referencedprepearedtransaction.Transactionuid);

            return GetTransactionState(db,redis, preparedpaymenttransaction, transactionuid, sendnotification,
                GetTransactionState(db,redis, rpreparedpaymenttransaction, rpreparedpaymenttransaction?.Transactionuid,
                    sendnotification, null, sendcbor), sendcbor);

        }

        private static Preparedpaymenttransaction GetPreparedPaymentTransactionFromDatabase(EasynftprojectsContext db,
            string transactionuid)
        {
            var preparedpaymenttransaction = (from a in db.Preparedpaymenttransactions
                    .Include(a => a.PreparedpaymenttransactionsCustomproperties)
                    .AsSplitQuery()
                    .Include(a => a.PreparedpaymenttransactionsNfts)
                    .AsSplitQuery()
                    .Include(a => a.Nftaddresses)
                    .AsSplitQuery()
                    .Include(a => a.PreparedpaymenttransactionsSmartcontractsjsons)
                    .AsSplitQuery()
                    .Include(a => a.Legacyauctions)
                    .ThenInclude(a => a.Legacyauctionshistories)
                    .AsSplitQuery()
                    .Include(a => a.PreparedpaymenttransactionsNotifications)
                    .AsSplitQuery()
                    .Include(a => a.Nftproject)
                    .AsSplitQuery()
                    .Include(a => a.Mintandsend)
                    .AsSplitQuery()
                    .Include(a => a.PreparedpaymenttransactionsTokenprices)
                    .AsSplitQuery()
                    .Include(a => a.PreparedpaymenttransactionsSmartcontractOutputs)
                    .ThenInclude(a => a.PreparedpaymenttransactionsSmartcontractOutputsAssets)
                    .AsSplitQuery()
                    .Include(a => a.Smartcontracts)
                    .AsSplitQuery()
                    .Include(a => a.Buyoutaddresses)
                    .ThenInclude(a => a.BuyoutsmartcontractaddressesNfts)
                    .AsSplitQuery()
                    .Include(a=>a.Referencedprepearedtransaction)
                    .AsSplitQuery()
                    .Include(a=>a.NftaddressesNavigation).AsSplitQuery()
                where a.Transactionuid == transactionuid
                select a).AsNoTracking().FirstOrDefault();
            return preparedpaymenttransaction;
        }


        public static PaymentTransactionResultClass GetTransactionState(EasynftprojectsContext db, IConnectionMultiplexer redis, Preparedpaymenttransaction preparedpaymenttransaction, string transactionuid, bool sendnotification, PaymentTransactionResultClass referenedTransaction, bool sendcbor = false)
        {
            if (preparedpaymenttransaction == null) return null;
            var res = new PaymentTransactionResultClass()
                {PaymentTransactionUid = transactionuid};

          //  res.OriginalTransactionType = originaltransactiontype ?? preparedpaymenttransaction.Transactiontype.ToEnum<PaymentTransactionTypes>();

          if (referenedTransaction != null)
              res.ReferencedTransaction = referenedTransaction;
         

            res.CustomProperties = AddCustompropertiesToResult(preparedpaymenttransaction);

            if (Enum.TryParse<PaymentTransactionTypes>(preparedpaymenttransaction.Transactiontype, out var pm))
                res.PaymentTransactionType = pm;


            res=res.AddPaymentgatewayResults(preparedpaymenttransaction, pm)
                .AddAllTypesDecentralResults(preparedpaymenttransaction, pm)
                .AddDecentralSaleResults(preparedpaymenttransaction, pm)
                .AddSmartcontractAuctionAndDirectsaleAndOfferResults(preparedpaymenttransaction, pm,
                    out string transtype)
                .AddDirectsaleResults(preparedpaymenttransaction, pm)
                .AddSmartcontractDirectsaleOfferResults(preparedpaymenttransaction, pm)
                .AddTransactionParameters(preparedpaymenttransaction)
                .AddNmkrPayUrl(preparedpaymenttransaction, transtype)
                .AddSmartContractResults(preparedpaymenttransaction, pm)
                .AddSmartContractDirectsaleOfferResults(preparedpaymenttransaction, pm)
                .AddLegacyResults(preparedpaymenttransaction, pm)
                .AddSmartcontractDirectsaleResults(redis, preparedpaymenttransaction, pm)
                .AddCborsWithReadyToSign(preparedpaymenttransaction, sendcbor)
                .AddHistoryOnLegacyAuctions(preparedpaymenttransaction, pm)
                .AddHistoryOnSmartcontractAuctions(db, preparedpaymenttransaction, pm)
                .AddDefaultResults(preparedpaymenttransaction);


            if (sendnotification && preparedpaymenttransaction.PreparedpaymenttransactionsNotifications.Any())
                InsertNotifications(db, preparedpaymenttransaction, res);


            // Check for a smartcontracrt directsale - if is is still available
            res = CheckSmartcontractDirectsale(res, pm);

            return res;

        }

        private static PaymentTransactionResultClass AddDefaultResults(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction)
        {
            res.PaymentTransactionCreated = preparedpaymenttransaction.Created;
            res.Expires = preparedpaymenttransaction.Expires;
            res.ProjectUid = preparedpaymenttransaction.Nftproject.Uid;
            res.TxHash = preparedpaymenttransaction.Txhash;
            res.Customeripaddress = preparedpaymenttransaction.Customeripaddress;
            res.Referer = preparedpaymenttransaction.Referer;
            if (Enum.TryParse<PaymentTransactionsStates>(preparedpaymenttransaction.State, out var state))
                res.State = state;
            return res;
        }



        private static PaymentTransactionResultClass AddHistoryOnSmartcontractAuctions(this PaymentTransactionResultClass res, EasynftprojectsContext db,
            Preparedpaymenttransaction preparedpaymenttransaction, PaymentTransactionTypes pm)
        {
            // Add History of Smartcontract Auctions
            if (pm == PaymentTransactionTypes.smartcontract_auction)
            {
                var history = (from a in preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons
                    select new AuctionHistoryResultClass()
                    {
                        BidAmount = a.Bidamount ?? 0,
                        Created = a.Created ?? DateTime.Now,
                        Address = a.Address,
                        TxHash = a.Txid,
                        SignedAndSubmitted = a.Signedandsubmitted,
                        State = GetHistoryState(a,
                            preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons)
                    }).ToArray();

                var marketplace = GetMarketplacePkhPercentage(db, preparedpaymenttransaction.NftprojectId);
                var royalty = GetRoyaltyPkhPercentage(db, preparedpaymenttransaction.Policyid);

                res.AuctionResults = new()
                {
                    History = history,
                    MinBet = preparedpaymenttransaction.Auctionminprice ?? 0,
                    MarketplaceFeePercent = marketplace.Percentage,
                    RunsUntil = preparedpaymenttransaction.Created.AddSeconds((int) preparedpaymenttransaction
                        .Auctionduration),
                    RoyaltyFeePercent = royalty?.Percentage,

                    JsonHash = preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons.Any()
                        ? preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last().Hash
                        : "",
                    ActualBid = preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons.Any()
                        ? preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last()
                            .Bidamount
                        : 0,
                };
            }

            return res;
        }

        private static PaymentTransactionResultClass AddHistoryOnLegacyAuctions(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction,
            PaymentTransactionTypes pm)
        {
            // Add History of Legacy Auctions
            if (pm == PaymentTransactionTypes.legacy_auction)
            {
                var history = (from a in preparedpaymenttransaction.Legacyauctions.Legacyauctionshistories
                    select new AuctionHistoryResultClass
                    {
                        Address = a.Senderaddress,
                        BidAmount = a.Bidamount,
                        Created = a.Created,
                        ReturnTxHash = a.Returntxhash,
                        TxHash = a.Txhash,
                        SignedAndSubmitted = true,
                        State = GetHistoryState(a.State)
                    }).ToArray();
                res.AuctionResults = new()
                {
                    ActualBid = preparedpaymenttransaction.Legacyauctions.Actualbet,
                    History = history,
                    MinBet = preparedpaymenttransaction.Legacyauctions.Minbet,
                    MarketplaceFeePercent = preparedpaymenttransaction.Legacyauctions.Marketplacefeepercent,
                    RunsUntil = preparedpaymenttransaction.Legacyauctions.Runsuntil,
                    RoyaltyFeePercent = preparedpaymenttransaction.Legacyauctions.Royaltyfeespercent
                };

                res.Fee = preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons
                    .LastOrDefault()?.Fee;
            }

            return res;
        }

        private static PaymentTransactionResultClass AddCborsWithReadyToSign(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction, bool sendcbor)
        {
            // Send CBORS with ready to sign

            if (preparedpaymenttransaction.Smartcontractstate ==
                nameof(PaymentTransactionSubstates.readytosignbybuyer) ||
                preparedpaymenttransaction.Smartcontractstate ==
                nameof(PaymentTransactionSubstates.readytosignbysellercancel) ||
                preparedpaymenttransaction.Smartcontractstate ==
                nameof(PaymentTransactionSubstates.readytosignbybuyercancel) ||
                preparedpaymenttransaction.Smartcontractstate ==
                nameof(PaymentTransactionSubstates.readytosignbyseller) || sendcbor)
            {
                if (preparedpaymenttransaction
                    .PreparedpaymenttransactionsSmartcontractsjsons.Any())
                {
                    res.Cbor = ConsoleCommand.GetCbor(preparedpaymenttransaction
                        .PreparedpaymenttransactionsSmartcontractsjsons.Last().Rawtx);

                    res.SignedCbor = ConsoleCommand.GetCbor(preparedpaymenttransaction
                        .PreparedpaymenttransactionsSmartcontractsjsons.Last().Signedcbr);

                    res.SignGuid = preparedpaymenttransaction
                        .PreparedpaymenttransactionsSmartcontractsjsons.Last().Signinguid;

                    res.Fee = preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons
                        .LastOrDefault()?.Fee;
                }
            }

            return res;
        }

        private static PaymentTransactionResultClass AddSmartcontractDirectsaleResults(this PaymentTransactionResultClass res, IConnectionMultiplexer redis,
            Preparedpaymenttransaction preparedpaymenttransaction, PaymentTransactionTypes pm)
        {
            if (pm == PaymentTransactionTypes.smartcontract_directsale &&
                preparedpaymenttransaction.BuyoutaddressesId != null)
            {
                var rates = GlobalFunctions.GetNewRates(redis, Coin.ADA);

                GetPaymentAddressResultClass pnrc = new()
                {
                    Expires = preparedpaymenttransaction.Buyoutaddresses.Expiredate,
                    PaymentAddress = preparedpaymenttransaction.Buyoutaddresses.Address,
                    Debug = "",

                    PriceInEur =
                        (float) Math.Round(
                            (rates.EurRate * (preparedpaymenttransaction.Buyoutaddresses.Lovelace +
                                               preparedpaymenttransaction.Buyoutaddresses.Additionalamount) / 1000000),
                            2),
                    PriceInUsd =
                        (float) Math.Round(
                            ((rates.UsdRate) * (preparedpaymenttransaction.Buyoutaddresses.Lovelace +
                                                     preparedpaymenttransaction.Buyoutaddresses.Additionalamount) /
                             1000000),
                            2),
                    PriceInJpy =
                        (float) Math.Round(
                            ((rates.JpyRate) * (preparedpaymenttransaction.Buyoutaddresses.Lovelace +
                                                     preparedpaymenttransaction.Buyoutaddresses.Additionalamount) /
                             1000000),
                            2),

                    PriceInLovelace = (long) (preparedpaymenttransaction.Buyoutaddresses.Lovelace +
                                              preparedpaymenttransaction.Buyoutaddresses.Additionalamount),
                    Effectivedate = rates.EffectiveDate,
                    SendbackToUser = preparedpaymenttransaction.Buyoutaddresses.Additionalamount +
                                     preparedpaymenttransaction.Buyoutaddresses.Lockamount,
                    Revervationtype = "specific"
                };
                res.DirectSaleResults.BuyoutSmartcontractAddress = pnrc;
            }

            return res;
        }

        private static PaymentTransactionResultClass AddLegacyResults(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction, PaymentTransactionTypes pm)
            
        {
            if (pm == PaymentTransactionTypes.legacy_directsale ||
                pm == PaymentTransactionTypes.legacy_auction)
            {
                res.PaymentTransactionSubStateResult = new()
                    {PaymentTransactionSubstate = PaymentTransactionSubstates.waitingforlocknft};
                // TODO: Set last tx hash

                if (Enum.TryParse<PaymentTransactionSubstates>(preparedpaymenttransaction.Smartcontractstate,
                        out var smcs))
                    res.PaymentTransactionSubStateResult.PaymentTransactionSubstate = smcs;
            }

            return res;
        }

        private static PaymentTransactionResultClass AddSmartContractDirectsaleOfferResults(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction,
            PaymentTransactionTypes pm)
        {
            if (pm == PaymentTransactionTypes.smartcontract_directsale_offer)
            {
                res.PaymentTransactionSubStateResult = new()
                    {PaymentTransactionSubstate = PaymentTransactionSubstates.waitingforlockada};

                res.PaymentTransactionSubStateResult.LastTxHash = preparedpaymenttransaction
                    .PreparedpaymenttransactionsSmartcontractsjsons.LastOrDefault()?.Txid;

                if (Enum.TryParse<PaymentTransactionSubstates>(preparedpaymenttransaction.Smartcontractstate,
                        out var smcs))
                    res.PaymentTransactionSubStateResult.PaymentTransactionSubstate = smcs;
            }

            return res;
        }

        private static PaymentTransactionResultClass AddSmartContractResults(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction,
            PaymentTransactionTypes pm)
        {
            if (pm == PaymentTransactionTypes.smartcontract_auction ||
                pm == PaymentTransactionTypes.smartcontract_directsale)
            {
                res.PaymentTransactionSubStateResult = new()
                    {PaymentTransactionSubstate = PaymentTransactionSubstates.waitingforlocknft};

                res.PaymentTransactionSubStateResult.LastTxHash = preparedpaymenttransaction
                    .PreparedpaymenttransactionsSmartcontractsjsons.LastOrDefault()?.Txid;

                if (Enum.TryParse<PaymentTransactionSubstates>(preparedpaymenttransaction.Smartcontractstate,
                        out var smcs))
                    res.PaymentTransactionSubStateResult.PaymentTransactionSubstate = smcs;
            }

            return res;
        }

        private static PaymentTransactionResultClass AddNmkrPayUrl(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction,
            string transtype)
        {
            res.NMKRPayUrl = GeneralConfigurationClass.Paywindowlink + transtype +
                             preparedpaymenttransaction.Transactionuid;

            if (preparedpaymenttransaction.Smartcontractstate == nameof(PaymentTransactionSubstates.waitingforsale))
                res.NMKRPayUrl += "&a=buy";

            if (preparedpaymenttransaction.Smartcontractstate == nameof(PaymentTransactionSubstates.waitingforlocknft))
                res.NMKRPayUrl += "&a=list";
            return res;
        }

        private static PaymentTransactionResultClass AddTransactionParameters(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction)
        {
            // Transaction Parameters - at the moment the tokens
            if (preparedpaymenttransaction.PreparedpaymenttransactionsNfts.Any())
            {
                List<TransactionParametersClass> tpc = new();
                foreach (var preparedpaymenttransactionsNft in preparedpaymenttransaction
                             .PreparedpaymenttransactionsNfts.OrEmptyIfNull())
                {
                    tpc.Add(new()
                    {
                        PolicyId = preparedpaymenttransactionsNft.Policyid,
                        TokennameHex = preparedpaymenttransactionsNft.Tokennamehex,
                        Tokencount = preparedpaymenttransactionsNft.Count,
                        Tokenname = preparedpaymenttransactionsNft.Tokennamehex.FromHex(),
                    });
                }

                res.TransactionParameters = tpc.ToArray();
            }

            return res;
        }

        private static PaymentTransactionResultClass AddSmartcontractDirectsaleOfferResults(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction,
            PaymentTransactionTypes pm)
        {
            if (pm == PaymentTransactionTypes.smartcontract_directsale_offer)
            {
                var price = preparedpaymenttransaction.Lovelace ?? 0;

                res.DirectSaleOfferResults = new()
                {
                    OfferPrice = price,
                    LockedInAmount = preparedpaymenttransaction.Lockamount ?? 0,
                    BuyerAddress = preparedpaymenttransaction.Buyeraddress,
                    SellerAddress = preparedpaymenttransaction.Selleraddress,
                    BuyerTxHash = preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons
                        .LastOrDefault(x => x.Templatetype == nameof(DatumTemplateTypes.lockada))?.Txid,
                    BuyerTxDatumHash = preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons
                        .LastOrDefault(x => x.Templatetype == nameof(DatumTemplateTypes.lockada))?.Hash,
                    BuyerTxCreate = preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons
                        .LastOrDefault(x => x.Templatetype == nameof(DatumTemplateTypes.lockada))?.Created,
                };
                res.DirectSaleOfferResults.Receivers = GetReceivers(preparedpaymenttransaction);
            }

            return res;
        }

        private static PaymentTransactionResultClass AddDirectsaleResults(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction,
            PaymentTransactionTypes pm)
        {
            // Send Directsale Parameters
            if (pm == PaymentTransactionTypes.legacy_directsale || pm ==
                PaymentTransactionTypes.smartcontract_directsale)
            {
                var price = preparedpaymenttransaction.Lovelace ?? 0;

                res.DirectSaleResults = new()
                {
                    SellingPrice = price,
                    LockedInAmount = preparedpaymenttransaction.Lockamount ?? 0,
                    SellerAddress = preparedpaymenttransaction.Selleraddress,
                    BuyerAddress = preparedpaymenttransaction.Buyeraddress,
                    SellerTxHash = preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons
                        .LastOrDefault(x => x.Templatetype == nameof(DatumTemplateTypes.locknft))?.Txid,
                    SellerTxDatumHash = preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons
                        .LastOrDefault(x => x.Templatetype == nameof(DatumTemplateTypes.locknft))?.Hash,
                    SellerTxCreate = preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractsjsons
                        .LastOrDefault(x => x.Templatetype == nameof(DatumTemplateTypes.locknft))?.Created,
                };
                res.DirectSaleResults.Receivers = GetReceivers(preparedpaymenttransaction);
            }

            return res;
        }

        private static PaymentTransactionResultClass AddSmartcontractAuctionAndDirectsaleAndOfferResults(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction,
            PaymentTransactionTypes pm, out string transtype)
        {
            // Set paywindow link
            transtype = "mtid=";
            if (pm == PaymentTransactionTypes.smartcontract_auction ||
                pm == PaymentTransactionTypes.smartcontract_directsale ||
                pm == PaymentTransactionTypes.smartcontract_directsale_offer)
            {
                transtype = "adsid=";

                if (preparedpaymenttransaction.Smartcontracts != null)
                {
                    res.SmartContractInformation = new SmartContractInformationResultClass()
                    {
                        SmartcontractAddress = preparedpaymenttransaction.Smartcontracts.Address,
                        SmartcontractName = preparedpaymenttransaction.Smartcontracts.Smartcontractname,
                        SmartcontractType = preparedpaymenttransaction.Smartcontracts.Type
                    };
                }
            }

            return res;
        }

        private static PaymentTransactionResultClass AddDecentralSaleResults(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction,
            PaymentTransactionTypes pm)
        {
            if (pm == PaymentTransactionTypes.decentral_mintandsale_specific ||
                pm == PaymentTransactionTypes.decentral_mintandsale_random ||


                pm == PaymentTransactionTypes.paymentgateway_nft_specific ||
                pm == PaymentTransactionTypes.paymentgateway_nft_random)
            {
                res.DecentralParameters.StakeRewards = preparedpaymenttransaction.Stakerewards;
                res.DecentralParameters.TokenRewards = preparedpaymenttransaction.Tokenrewards;
                res.DecentralParameters.Discount = preparedpaymenttransaction.Discount;
                res.DecentralParameters.Fees = preparedpaymenttransaction.Fee;
                res.DecentralParameters.OptionalReceiverAddress =preparedpaymenttransaction.Optionalreceiveraddress;

                if (preparedpaymenttransaction.State == nameof(PaymentTransactionsStates.rejected))
                {
                    res.DecentralParameters.RejectParameter = preparedpaymenttransaction.Rejectparameter;
                    res.DecentralParameters.RejectReason = preparedpaymenttransaction.Rejectreason;
                }
            }

            return res;
        }

        private static PaymentTransactionResultClass AddAllTypesDecentralResults(this PaymentTransactionResultClass res,Preparedpaymenttransaction preparedpaymenttransaction,
            PaymentTransactionTypes pm)
        {
            if (pm == PaymentTransactionTypes.decentral_mintandsale_specific ||
                pm == PaymentTransactionTypes.decentral_mintandsale_random ||
                pm == PaymentTransactionTypes.decentral_mintandsend_random ||
                pm == PaymentTransactionTypes.decentral_mintandsend_specific ||


                pm == PaymentTransactionTypes.paymentgateway_nft_specific ||
                pm == PaymentTransactionTypes.paymentgateway_nft_random)
            {

                    
                res.DecentralParameters = new()
                {
                    PriceInLovelace = preparedpaymenttransaction.Lovelace,
                    AdditionalPriceInTokens = GetAdditionalPriceInTokens(preparedpaymenttransaction)
                };


                // Fill the nfts
                var rnc2 = (from a in preparedpaymenttransaction.PreparedpaymenttransactionsNfts.OrEmptyIfNull()
                    select new ReservedNftsClassV2()
                    {
                        NftUid = a.Nftuid, Tokencount = a.Count, TokennameHex = a.Tokennamehex,
                        PolicyId = a.Policyid, NftId = a.NftId, Lovelace = a.Lovelace
                    }).ToArray();

                if (rnc2.Length == 0)
                {
                    res.DecentralParameters.MintNfts = new()
                        {CountNfts = preparedpaymenttransaction.Countnft, ReserveNfts = rnc2};
                }
                else
                {
                    res.DecentralParameters.MintNfts = new()
                        {CountNfts = rnc2.Length, ReserveNfts = rnc2};
                }

                if (Enum.TryParse<PaymentTransactionSubstates>(preparedpaymenttransaction.Smartcontractstate,
                        out var smcs))
                    res.PaymentTransactionSubStateResult = new()
                        {PaymentTransactionSubstate = smcs, LastTxHash = preparedpaymenttransaction.Txhash};
            }

            return res;
        }

        private static PaymentTransactionResultClass AddPaymentgatewayResults(this PaymentTransactionResultClass res, Preparedpaymenttransaction preparedpaymenttransaction,
            PaymentTransactionTypes pm)
        {
            if (pm == PaymentTransactionTypes.paymentgateway_nft_specific ||
                pm == PaymentTransactionTypes.paymentgateway_nft_random ||
                pm == PaymentTransactionTypes.paymentgateway_mintandsend_random ||
                pm == PaymentTransactionTypes.paymentgateway_mintandsend_specific ||
                pm == PaymentTransactionTypes.nmkr_pay_random ||
                pm == PaymentTransactionTypes.nmkr_pay_specific)
            {
                res.PaymentgatewayResults = new()
                {
                    PriceInLovelace = preparedpaymenttransaction.Lovelace,
                    Fee = preparedpaymenttransaction.Fee,
                    Discount = preparedpaymenttransaction.Discount,
                    StakeRewards = preparedpaymenttransaction.Stakerewards,
                    TokenRewards = preparedpaymenttransaction.Tokenrewards,
                    AdditionalPriceInTokens = GetAdditionalPriceInTokens(preparedpaymenttransaction),
                    OptionalReceiverAddress = preparedpaymenttransaction.Optionalreceiveraddress
                };
                if (preparedpaymenttransaction.NftaddressesNavigation != null)
                {
                    res.PaymentgatewayResults.ReceiverAddress =
                        preparedpaymenttransaction.NftaddressesNavigation.Senderaddress;
                    res.PaymentgatewayResults.TxHash = preparedpaymenttransaction.NftaddressesNavigation.Outgoingtxhash;
                    res.PaymentgatewayResults.ReceiverStakeAddress =
                        Bech32Engine.GetStakeFromAddress(
                            preparedpaymenttransaction.NftaddressesNavigation.Senderaddress);
                    res.PaymentgatewayResults.SenderAddress = preparedpaymenttransaction.NftaddressesNavigation.Address;
                }

                /*if (pm == PaymentTransactionTypes.nmkr_pay_random ||
                    pm == PaymentTransactionTypes.nmkr_pay_specific)*/
                {
                    res.Expires = preparedpaymenttransaction.Expires;
                    res.PaymentgatewayResults.MintNfts = new()
                        {CountNfts = preparedpaymenttransaction.Countnft};
                    res.TransactionParameters = null;
                    res.PaymentgatewayResults.OptionalReceiverAddress =
                        preparedpaymenttransaction.Optionalreceiveraddress;
                    var rnc2 = (from a in preparedpaymenttransaction.PreparedpaymenttransactionsNfts.OrEmptyIfNull()
                        select new ReservedNftsClassV2()
                        {
                            NftUid = a.Nftuid,
                            Tokencount = a.Count,
                            TokennameHex = a.Tokennamehex,
                            PolicyId = a.Policyid,
                            NftId = a.NftId,
                            Lovelace = a.Lovelace
                        }).ToArray();


                    res.PaymentgatewayResults.MintNfts.ReserveNfts = rnc2;

                    if (preparedpaymenttransaction.Mintandsend != null)
                    {
                        res.MintAndSendResults = new()
                        {
                            Executed = preparedpaymenttransaction.Mintandsend.Executed,
                            ReceiverAddress = preparedpaymenttransaction.Mintandsend.Receiveraddress,
                            TransactionId = preparedpaymenttransaction.Mintandsend.Transactionid,
                            State = GetMintAndSendState(preparedpaymenttransaction.Mintandsend.State)
                        };
                    }

                    res.PaymentGatewayType = res.PaymentTransactionType;
                    if (res.ReferencedTransaction != null)
                    {
                        res.PaymentGatewayType = res.ReferencedTransaction.PaymentTransactionType;
                    }
                }


                if (pm == PaymentTransactionTypes.paymentgateway_nft_random ||
                    pm == PaymentTransactionTypes.paymentgateway_nft_specific)
                {
                    res.Cbor = ConsoleCommand.GetCbor(preparedpaymenttransaction.Cbor);
                    res.SignedCbor = ConsoleCommand.GetCbor(preparedpaymenttransaction.Signedcbor);
                    res.Expires = preparedpaymenttransaction.Expires;
                }
            }

            return res;
        }

        private static Dictionary<string, string> AddCustompropertiesToResult(Preparedpaymenttransaction preparedpaymenttransaction)
        {
            Dictionary<string, string> prop = new();
            if (preparedpaymenttransaction.PreparedpaymenttransactionsCustomproperties != null)
                foreach (var preparedpaymenttransactionPreparedpaymenttransactionsCustomproperty in
                         preparedpaymenttransaction.PreparedpaymenttransactionsCustomproperties.OrEmptyIfNull())
                {
                    try
                    {
                        prop.Add(preparedpaymenttransactionPreparedpaymenttransactionsCustomproperty.Key,
                            preparedpaymenttransactionPreparedpaymenttransactionsCustomproperty.Value);
                    }
                    catch
                    {}
                }

            return prop;
        }

        private static PaymentTransactionResultClass CheckSmartcontractDirectsale(PaymentTransactionResultClass res, PaymentTransactionTypes pm)
        {
            if (pm != PaymentTransactionTypes.smartcontract_directsale)
                return res;

            if (res.State!= PaymentTransactionsStates.active)
                return res;

            if (res.PaymentTransactionSubStateResult.PaymentTransactionSubstate !=
                PaymentTransactionSubstates.waitingforsale)
                return res;

            var bl = BlockfrostFunctions.GetTransactionUtxoFromBlockfrost(res.PaymentTransactionSubStateResult.LastTxHash);
            if (bl == null || !bl.Outputs.Any())
            {
                res.PaymentTransactionSubStateResult.PaymentTransactionSubstate =
                    PaymentTransactionSubstates.sold;
                res.State = PaymentTransactionsStates.finished;
            }

            return res;
        }

        private static SmartcontractDirectsaleReceiverClass[] GetReceivers(Preparedpaymenttransaction preparedpaymenttransaction)
        {
            // TODO: Multiplier, Decimals from Tokenregistry or Database
            var res = (from a in preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractOutputs
                select new SmartcontractDirectsaleReceiverClass()
                {
                    Address = a.Address, AmountInLovelace = a.Lovelace, Pkh = a.Pkh, RecevierType = a.Type, Tokens =
                        (from b in a.PreparedpaymenttransactionsSmartcontractOutputsAssets
                            select new Tokens()
                            {
                                AssetName = b.Tokennameinhex.FromHex(), AssetNameInHex = b.Tokennameinhex, PolicyId = b.Policyid,
                                CountToken = b.Amount, Decimals = 0, Multiplier = 1, TotalCount = b.Amount,  
                            }).ToArray()
                }).ToArray();
            return res;
        }

        internal static SmartContractsPayoutsClass[] GetReceiversToSmartContractsPayoutsClass(Preparedpaymenttransaction preparedpaymenttransaction)
        {
            List<SmartContractsPayoutsClass> res = new List<SmartContractsPayoutsClass>(); 
            // TODO: Multiplier, Decimals from Tokenregistry or Database
            foreach (var output in preparedpaymenttransaction.PreparedpaymenttransactionsSmartcontractOutputs)
            {
                string token = "";
                foreach (var nft in output.PreparedpaymenttransactionsSmartcontractOutputsAssets)
                {
                    if (token != "")
                        token += " + ";
                    token += nft.Amount + " " + nft.Policyid + "." + nft.Tokennameinhex;
                }

                var scpc = new SmartContractsPayoutsClass()
                {
                    address = output.Address,
                    lovelace = output.Lovelace,
                    receivertype = output.Type.ToEnum<ReceiverTypes>(),
                    tokens = token
                };
                res.Add(scpc);
            }
           
            return res.ToArray();
        }
        private static Tokens[] GetAdditionalPriceInTokens(Preparedpaymenttransaction transaction)
        {
            List<Tokens> token = new();

            if (transaction.PreparedpaymenttransactionsTokenprices!=null && transaction.PreparedpaymenttransactionsTokenprices.Any())
            {
                token.AddRange(transaction.PreparedpaymenttransactionsTokenprices.Select(tok => new Tokens()
                {
                    AssetNameInHex = tok.Assetname.ToHex(),
                    AssetName = tok.Assetname,
                    CountToken = (long)tok.Tokencount,
                    PolicyId = tok.Policyid,
                    TotalCount = (long)tok.Totalcount,
                    Multiplier = tok.Multiplier,
                    Decimals = tok.Decimals,
                }));
            }

            return token.ToArray();
        }


        private static void InsertNotifications(EasynftprojectsContext db, Preparedpaymenttransaction preparedpaymenttransaction, PaymentTransactionResultClass res)
        {
            foreach (var notification in preparedpaymenttransaction.PreparedpaymenttransactionsNotifications)
            {
                Notificationqueue not = new()
                {
                    Created = DateTime.Now,
                    Notificationendpoint = notification.Notificationendpoint,
                    Notificationtype = notification.Notificationtype,
                    Payload = JsonConvert.SerializeObject(res),
                    State = "active"
                };
                db.Notificationqueues.Add(not);
                db.SaveChanges();
            }
        }

        private static AuctionHistoryStates GetHistoryState(PreparedpaymenttransactionsSmartcontractsjson a, ICollection<PreparedpaymenttransactionsSmartcontractsjson> preparedpaymenttransactionsSmartcontractsjsons)
        {
            if (a.Templatetype == nameof(DatumTemplateTypes.locknft))
                return AuctionHistoryStates.seller;
            return a == preparedpaymenttransactionsSmartcontractsjsons.Last() ? AuctionHistoryStates.buyer : AuctionHistoryStates.outbid;
        }

     
        private static AuctionHistoryStates GetHistoryState(string aState)
        {
            return aState.ToEnum<AuctionHistoryStates>();
        }
        private static MintAndSendSubstates GetMintAndSendState(string aState)
        {
            return aState.ToEnum<MintAndSendSubstates>();
        }

        public static string FillJsonBuyer(this string json, EasynftprojectsContext db, BuyerClass bet)
        {
            if (json == null)
                return null;

            json = json.Replace("$buyerPkh$", bet.Buyer.Pkh);
            json = json.Replace("$buyeroffer$", bet.BuyerOffer.ToString());

            return json;
        }

       
        public static string FillJsonMarketplace(this string json, EasynftprojectsContext db, Smartcontractsmarketplacesetting settings)
        {
            if (json == null)
                return null;

            json = json.Replace("$marketplacePkh$", settings.Pkh);
            return json;
        }

        private static PkhPercentageClass GetRoyaltyPkhPercentage(EasynftprojectsContext db, string policyid)
        {
            var royalty=KoiosFunctions.GetRoyaltiesFromPolicyId(policyid);
            if (royalty == null)
                return null;

            PkhPercentageClass res = new() {Percentage = royalty.Percentage, PublicKeyHash = royalty.Pkh, Address = royalty.Address};

            return res;
        }

        private static PkhPercentageClass GetRefererPkhPercentage(EasynftprojectsContext db,
            int projectid)
        {
            var nftproject = (from a in db.Nftprojects
                    .Include(a=>a.Customerwallet).AsSplitQuery()
                    .Include(a=>a.Customer).AsSplitQuery()
                where a.Id == projectid
                select a).FirstOrDefault();

            if (nftproject == null || nftproject.Addrefereramounttopaymenttransactions==null || nftproject.Addrefereramounttopaymenttransactions==0)
                return null;

            return new()
            {
                Percentage = (float)(nftproject.Addrefereramounttopaymenttransactions??0),
                PublicKeyHash = GlobalFunctions.GetPkhFromAddress(nftproject.Customerwallet!=null? nftproject.Customerwallet.Walletaddress : nftproject.Customer.Adaaddress),
                Address= nftproject.Customerwallet != null ? nftproject.Customerwallet.Walletaddress : nftproject.Customer.Adaaddress
            };
        }


        private static PkhPercentageClass GetMarketplacePkhPercentage(EasynftprojectsContext db,
            int projectid)
        {
            var nftproject = (from a in db.Nftprojects
                    .Include(a => a.Smartcontractssettings)
                where a.Id == projectid
                select a).FirstOrDefault();

            if (nftproject == null)
                return null;
            if (string.IsNullOrEmpty(nftproject.Smartcontractssettings.Pkh))
            {
                nftproject.Smartcontractssettings.Pkh = ConsoleCommand.GetPkh(Encryption.DecryptString(nftproject.Smartcontractssettings.Vkey, nftproject.Smartcontractssettings.Salt + GeneralConfigurationClass.Masterpassword));
                db.SaveChanges();
            }

            return new()
            {
                EncryptedSKey = nftproject.Smartcontractssettings.Skey,
                Percentage = nftproject.Smartcontractssettings.Percentage,
                PublicKeyHash = nftproject.Smartcontractssettings.Pkh,
                Salt = nftproject.Smartcontractssettings.Salt,
                Address = nftproject.Smartcontractssettings.Address,
            };
        }

        internal static string FillJsonRedeemer(EasynftprojectsContext db, Preparedpaymenttransaction preparedtransaction,
            string templatetype)
        {
            var contract = preparedtransaction.Smartcontracts.Smartcontractsjsontemplates.LastOrDefault(x => x.Templatetype == templatetype);
            if (contract == null)
                return null;
            if (contract.Redeemertemplate == null)
                return null;
            return FillJson(db, preparedtransaction, contract.Redeemertemplate,"");
        }

        internal static string FillJsonTemplate(EasynftprojectsContext db, Preparedpaymenttransaction preparedtransaction, string templatetype)
        {
            var contract = preparedtransaction.Smartcontracts.Smartcontractsjsontemplates.LastOrDefault(x => x.Templatetype == templatetype);
            if (contract == null)
                return "";
            if (contract.Jsontemplate == null)
                return null;
            return FillJson(db, preparedtransaction, contract.Jsontemplate, contract.Recipienttemplate);
        }

        private static string FillJson(EasynftprojectsContext db, Preparedpaymenttransaction preparedtransaction, string template, string recipienttemplate)
        {
            var created = preparedtransaction.Created;
            var posixtimecreated = new DateTimeOffset(created).ToUnixTimeSeconds();
           // var now = nowseconds * 1000;

            string json = template;

            var marketplace = GetMarketplacePkhPercentage(db, preparedtransaction.NftprojectId);
            var royalty = GetRoyaltyPkhPercentage(db, preparedtransaction.Policyid);

            preparedtransaction.Lovelace ??= 0;
            
            string reci = "";
            string mp = AddRecipient(recipienttemplate, marketplace, preparedtransaction.Transactiontype,(long) preparedtransaction.Lovelace, out long marketplaceamount);
            string roy= AddRecipient(recipienttemplate, royalty, preparedtransaction.Transactiontype,(long)preparedtransaction.Lovelace, out long royaltyamount);


            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.smartcontract_auction))
            {
                var posixtimeends = new DateTimeOffset(created.AddSeconds(preparedtransaction.Auctionduration??1200)).ToUnixTimeSeconds();
                json = json.Replace("$timestamp$", posixtimeends.ToString());
                json = json.Replace("$minBid$", preparedtransaction.Auctionminprice.ToString());
                int royaltypercentage = royalty!=null? Convert.ToInt32(royalty.Percentage * 10):0;
                int marketplacepercentage = marketplace!=null? Convert.ToInt32(marketplace.Percentage * 10):0;

                int sellerpercent = 1000 - royaltypercentage - marketplacepercentage;

                reci = AddRecipientSellerPercentage(recipienttemplate, preparedtransaction.Sellerpkh, sellerpercent);
            }

            if (preparedtransaction.Transactiontype ==nameof(PaymentTransactionTypes.smartcontract_directsale) || preparedtransaction.Transactiontype==nameof(PaymentTransactionTypes.smartcontract_directsale_offer))
            {
                long lockAmount = preparedtransaction.Lockamount ?? 0;
                var price = (preparedtransaction.Lovelace);
                // lockamount is going back to the buyer - so we will add this to the price
                long sellerAmount = ((long)price - marketplaceamount - royaltyamount + lockAmount);
                reci =AddRecipientSellerAmount(recipienttemplate, preparedtransaction.Sellerpkh, sellerAmount);

                json = json.Replace("$price$", price.ToString());
            }
           


            if (!string.IsNullOrEmpty(mp))
            {
                reci += "," + mp;
            }

            if (!string.IsNullOrEmpty(roy))
            {
                reci += "," + roy;
            }

            json = json.Replace("$now$", posixtimecreated.ToString());
            json = json.Replace("$sellerPkh$", preparedtransaction.Sellerpkh);
            json = json.Replace("$policyid$", preparedtransaction.Policyid);
            json = json.Replace("$tokenname$", preparedtransaction.Tokenname);
            json = json.Replace("$tokencount$", preparedtransaction.Tokencount.ToString());

            // Ad Recipients
            json = json.Replace("$recipients$", reci);


            return json;
        }

        private static string AddRecipient(string template, PkhPercentageClass receiver,
            string transactiontype, long price, out long amount)
        {
            amount = 0;

            if (receiver == null)
                return "";

            if (transactiontype == nameof(PaymentTransactionTypes.smartcontract_auction))
            {
                int percenttage = Convert.ToInt32(receiver.Percentage * 10);
                template = template.Replace("$PercentageInt$", percenttage.ToString());
                amount = percenttage;
            }

            if (transactiontype == nameof(PaymentTransactionTypes.smartcontract_directsale) || transactiontype==nameof(PaymentTransactionTypes.smartcontract_directsale_offer))
            {
                amount = Convert.ToInt64(price / 100 * receiver.Percentage);
                if (amount != 0)
                    amount = Math.Max(1000000, amount);
                template = template.Replace("$Amount$", amount.ToString());
            }

            template = template.Replace("$Pkh$", receiver.PublicKeyHash);
            if (amount == 0)
                return "";
            return template;
        }

        private static string AddRecipientSellerAmount(string template, string sellerpkh,
            long amount)
        {
            template = template.Replace("$Amount$", amount.ToString());
            template = template.Replace("$Pkh$", sellerpkh ?? "");
            if (amount == 0)
                return "";
            return template;
        }

        private static string AddRecipientSellerPercentage(string template, string sellerpkh,
            long amount)
        {
            template = template.Replace("$PercentageInt$", amount.ToString());
            template = template.Replace("$Pkh$", sellerpkh ?? "");
            if (amount == 0)
                return "";
            return template;
        }

        internal static string GetHash(string ptsjJson)
        {
            string guid = GlobalFunctions.GetGuid();
            string jsonfile = GeneralConfigurationClass.TempFilePath+ guid + ".json";

            if (!string.IsNullOrEmpty(ptsjJson))
            {
                File.WriteAllText(jsonfile, ptsjJson);
            }
            else
                return "";

            var hash = ConsoleCommand.CardanoCli("latest transaction hash-script-data --script-data-file " + jsonfile,
                out var errormessage);


            if (hash.Contains("ERROR"))
                return "";
            hash = hash.Replace("\n", "").Replace("\r", "").Replace(Environment.NewLine, "");

            GlobalFunctions.DeleteFile(jsonfile);
            return hash;
        }

        public static TxInAddressesClass[] GetAllNeededTxin(IConnectionMultiplexer redis, string[] addresses, long minlovelave,
            long tokencount,
            string searchForTokenOrPolicyid, string collateral, out string errormessage)
        {
            var adr = (from a in addresses
                select new AddressTxInClass() {Address = a, Utxo = null}).ToArray();
            return GetAllNeededTxin(redis, adr, minlovelave, tokencount, searchForTokenOrPolicyid, collateral, out errormessage, out AllTxInAddressesClass alltxin);
        }
        
        
        public static TxInAddressesClass[] GetAllNeededTxinWithCardanosharp(IConnectionMultiplexer redis, AddressTxInClass[] inaddresses, CreateManagedWalletTransactionClass transaction,
            Tokens[] tokensToMint, string senderaddress, out AllTxInAddressesClass alltxin)
        {
            alltxin = null;

            if (inaddresses == null || inaddresses.Length == 0)
            {
                return null;
            }

            alltxin = ConsoleCommand.GetNewUtxo(inaddresses.Select(x => x.Address).ToArray());

            var cs = new CoinSelectionService(new LargestFirstStrategy(), new BasicChangeSelectionStrategy());

            // Add all Mint Tokens
            var mints = TokenBundleBuilder.Create;
            foreach (var token in tokensToMint)
            {
                mints.AddToken(token.PolicyId.ToBytes(), token.AssetNameInHex.ToBytes(), token.TotalCount);
            }

            var outputs = transaction.ToTransactionOutput();
            var utxos = alltxin.ToCardanosharpUtxos();
            var csCoinSelection = cs.GetCoinSelection(outputs, utxos, senderaddress, mints, 20, 1000000);

            return ConvertCoinselectionToTxInAddressesClass(csCoinSelection, alltxin);
        }

        private static TxInAddressesClass[] ConvertCoinselectionToTxInAddressesClass(CoinSelection csCoinSelection, AllTxInAddressesClass alltxin)
        {
            throw new NotImplementedException();
        }

        public static TxInAddressesClass[] GetAllNeededTxin(IConnectionMultiplexer redis, AddressTxInClass[] addresses,
            long minlovelave, long searchForTokenOrPolicyidTokencount,
            string searchForTokenOrPolicyid, string collaterals, out string errormessage,
            out AllTxInAddressesClass alltxin)
        {
            string[] collateralArray = null;
            if (!string.IsNullOrEmpty(collaterals))
                collateralArray = collaterals.Split(',', StringSplitOptions.RemoveEmptyEntries);

            alltxin = null;

            errormessage = null;
            if (addresses == null || addresses.Length == 0)
            {
                errormessage = "Submit the addresses";
                return null;
            }

            alltxin = ConsoleCommand.GetNewUtxo(addresses.Select(x => x.Address).ToArray());

            long estimatedfees = 500000;
            long neededLovelace = minlovelave + estimatedfees;
            List<TxInAddressesClass> selectedTxIns = new List<TxInAddressesClass>();

            // First find the searched token
            if (!string.IsNullOrEmpty(searchForTokenOrPolicyid))
            {
                var sel = FindTokenInTxin(redis, searchForTokenOrPolicyid, searchForTokenOrPolicyidTokencount, alltxin);
                if (sel != null)
                    selectedTxIns = sel.ToList();
                if (!selectedTxIns.Any())
                {
                    errormessage = "Needed Token was not found";
                    return null;
                }
            }

            int loopcount = 0;
            do
            {
                loopcount++;
                // Then Calculate, if we need more lovelace (minutxo, if there are more tokens in a single txin)
                long additionalneededlovelace = CalculateAdditionalLovelace(selectedTxIns.ToArray(), searchForTokenOrPolicyid, searchForTokenOrPolicyidTokencount);

                // Then check if we have enough lovelace on the selected txin - and choose more txin - if necessary
                if (!SelectTxInForNeededLovelace(redis,ref selectedTxIns,alltxin, neededLovelace + additionalneededlovelace, collateralArray))
                {
                    errormessage = "Not enough lovelace";
                    return null;
                }
                
                // Check again, if the ada is still enough
                additionalneededlovelace = CalculateAdditionalLovelace(selectedTxIns.ToArray(), searchForTokenOrPolicyid, searchForTokenOrPolicyidTokencount);

                // If yes, then exit the loop
                if (!selectedTxIns.Any())
                    break;

                if (selectedTxIns.Sum(x=>x.LovelaceSummary) >= neededLovelace + additionalneededlovelace)
                    break;

                // This shoud never happen - just in case
                if (loopcount > 10)
                {
                    errormessage = "Loop count exceeded";
                    return null;
                }
            } while (true);


            return selectedTxIns.ToArray();
        }

        private static bool SelectTxInForNeededLovelace(IConnectionMultiplexer redis, ref List<TxInAddressesClass> selectedTxIns,
            AllTxInAddressesClass alltxin, long neededLovelace,
            string[] dontUseThisCollaterals)
        {
            foreach (var inputAddresses in alltxin.TxInAddresses.OrEmptyIfNull().OrderBy(x=>x.TokensSum).ThenByDescending(x=>x.LovelaceSummary))
            {
                foreach (var inputtxin in inputAddresses.TxIn.OrEmptyIfNull().OrderBy(x => x.TokenSum).ThenByDescending(x => x.Lovelace))
                {
                    if (CheckForRecentlyUsedTxHashes(redis, inputtxin.TxHashId))
                        continue;

                    if (dontUseThisCollaterals != null && dontUseThisCollaterals.Any() && dontUseThisCollaterals.IndexOf(inputtxin.TxHashId) >= 0)
                    {
                        // We accept collateral only when it is 5 or below ada and if it has not tokens on it. otherwise we will use it (even if it is a collateral)
                        if (inputtxin.Lovelace <= 5000000 && (inputtxin.Tokens == null || !inputtxin.Tokens.Any()))
                            continue;
                    }

                    selectedTxIns.AddTxIn(inputtxin, inputAddresses);

                    if (selectedTxIns.Sum(x => x.LovelaceSummary) >= neededLovelace)
                        return true;
                }
            }

            return false;
        }

        private static long CalculateAdditionalLovelace(TxInAddressesClass[] selectedTxIns, string searchForTokenOrPolicyid, long searchForTokenOrPolicyidTokencount)
        {
            long minutxo = 1000000;
            long additionalMinutxo = 0;
            long tokens = 0;
            long assets = 0;
            foreach (var selectedTxIn in selectedTxIns.OrEmptyIfNull())
            {
                foreach (var token in selectedTxIn.GetAllTokens().OrEmptyIfNull())
                {
                    if (token.PolicyId == searchForTokenOrPolicyid ||
                        token.PolicyId + "." + token.Tokenname == searchForTokenOrPolicyid ||
                        token.PolicyId + "." + token.TokennameHex == searchForTokenOrPolicyid)
                    {
                        if (token.Quantity >= searchForTokenOrPolicyidTokencount)
                        {
                            assets++;
                            tokens += token.Quantity - searchForTokenOrPolicyidTokencount;
                        }
                    }
                    else
                    {
                        assets++;
                        tokens += token.Quantity;
                    }
                }
            }

            if (assets > 0)
                minutxo = 1400000; // Estiamted minutxo for 1 asset
            additionalMinutxo = assets * 10000; // Estimated additional minutxo per asset

            return minutxo + additionalMinutxo;
        }

        private static TxInAddressesClass[] FindTokenInTxin(IConnectionMultiplexer redis, string searchForTokenOrPolicyid, long tokencount,
            AllTxInAddressesClass alltxin)
        {
            List<TxInAddressesClass> resulttxout = new();
            long counttokensfound = 0;
            foreach (var inputAddresses in alltxin.TxInAddresses.OrEmptyIfNull())
            {
                foreach (var inputtxin in inputAddresses.TxIn.OrEmptyIfNull())
                {
                    if (CheckForRecentlyUsedTxHashes(redis, inputtxin.TxHashId))
                        continue;

                    foreach (var txInTokensClass in inputtxin.Tokens.OrEmptyIfNull())
                    {
                        string tokenassetidNftCip68 = "";
                        string tokenassetidFtCip68 = "";
                        if (!string.IsNullOrEmpty(searchForTokenOrPolicyid))
                        {
                            tokenassetidNftCip68 =
                                ConsoleCommand.CreateMintTokenname("", searchForTokenOrPolicyid, ConsoleCommand.Cip68Type.NftUserToken).ToLower();
                            tokenassetidFtCip68 =
                                ConsoleCommand.CreateMintTokenname("", searchForTokenOrPolicyid, ConsoleCommand.Cip68Type.FtUserToken).ToLower();
                        }


                        if (txInTokensClass.PolicyId == searchForTokenOrPolicyid ||
                            txInTokensClass.PolicyId + "." + txInTokensClass.Tokenname == searchForTokenOrPolicyid ||
                            txInTokensClass.PolicyId + "." + txInTokensClass.TokennameHex == searchForTokenOrPolicyid ||
                            txInTokensClass.PolicyId + "." + txInTokensClass.TokennameHex.ToLower() == tokenassetidNftCip68 ||
                            txInTokensClass.PolicyId + "." + txInTokensClass.TokennameHex.ToLower() == tokenassetidFtCip68)
                        {
                            resulttxout = resulttxout.AddTxIn(inputtxin,inputAddresses);
                            counttokensfound += txInTokensClass.Quantity;
                            if (counttokensfound >= tokencount)
                                return resulttxout.ToArray();
                        }
                    }
                }
            }
            return null;
        }


   
        public static bool CheckForRecentlyUsedTxHashes(IConnectionMultiplexer redis, string tt2TxHash)
        {
            // If a txhash is used within the last 60 sec. - we will not use it again - to prevent to have some tx with already used txins
            var donotusetxin = GlobalFunctions.GetStringFromRedis(redis, "DoNotUseTxIn_" + tt2TxHash);
            return !string.IsNullOrEmpty(donotusetxin);
        }

        public static void SaveRecentlyUsedTxHashes(IConnectionMultiplexer redis, TxInAddressesClass[] txhashes)
        {
            foreach (var txhash in txhashes)
            {
                foreach (var txinClass in txhash.TxIn)
                {
                    GlobalFunctions.SaveStringToRedis(redis, "DoNotUseTxIn_" + txinClass.TxHashId, txinClass.TxHashId, 60);
                }
            }
        }

        public static void DeleteRecentlyUsedTxHashes(IConnectionMultiplexer redis, TxInAddressesClass[] txhashes)
        {
            foreach (var txhash in txhashes)
            {
                foreach (var txinClass in txhash.TxIn)
                {
                    GlobalFunctions.DeleteStringFromRedis(redis, "DoNotUseTxIn_" + txinClass.TxHashId);
                }
            }
        }

        private static TxInAddressesClass[] ConvertToTxInAddresses(AddressTxInClass[] addresses)
        {
            var adr = (from a in addresses
                select new TxInAddressesClass() {Address = a.Address, TxIn = a.Utxo}).ToArray();

            return adr;
        }

        private static List<TxInAddressesClass> AddTxIn(this List<TxInAddressesClass> givenInput, 
            TxInClass txin, TxInAddressesClass address)
        {
            var g1 = givenInput.Find(x => x.Address == address.Address);
            if (g1 == null)
            {
                g1 = new() {Address = address.Address};
                givenInput.Add(g1);
            }

            if (g1.TxIn == null)
            {
                List<TxInClass> giventxin = new();
                var g2 = new TxInClass()
                    { Lovelace = txin.Lovelace, Tokens = txin.Tokens, TxHash = txin.TxHash, TxId = txin.TxId };
                giventxin.Add(g2);
                g1.TxIn = giventxin.ToArray();
            }
            else
            {
                var g2 = g1.TxIn.FirstOrDefault(x => x.TxHash == txin.TxHash && x.TxId == txin.TxId);
                if (g2 == null)
                {
                    List<TxInClass> giventxin = g1.TxIn.ToList();
                    g2 = new()
                        {Lovelace = txin.Lovelace, Tokens = txin.Tokens, TxHash = txin.TxHash, TxId = txin.TxId};
                    giventxin.Add(g2);
                    g1.TxIn = giventxin.ToArray();
                }
            }

            return givenInput;
        }
      
        public static async Task<string> GetSmartContractTxin(string txhash, string smartcontractsAddress)
        {
            if (string.IsNullOrEmpty(txhash))
            {
                return null;
            }

            var transactions = await KoiosFunctions.GetTransactionInformationAsync(txhash);

            if (transactions == null)
                return null;

            foreach (var transaction in transactions)
            {
                var output = transaction.Outputs.FirstOrDefault(x => x.PaymentAddr.Bech32 == smartcontractsAddress);
                if (output == null)
                    continue;

                return output.TxHash + "#" + output.TxIndex;
            }

            return null;
        }

        public static async Task FakeSign(EasynftprojectsContext db, Preparedpaymenttransaction preparedtransaction)
        {
            if (string.IsNullOrEmpty(preparedtransaction.Nftproject.Smartcontractssettings.Fakesignskey))
            {
                var addr = ConsoleCommand.CreateNewPaymentAddress(GlobalFunctions.IsMainnet());
                preparedtransaction.Nftproject.Smartcontractssettings.Fakesignskey = addr.privateskey;
                preparedtransaction.Nftproject.Smartcontractssettings.Fakesignvkey = addr.privatevkey;
                preparedtransaction.Nftproject.Smartcontractssettings.Fakesignaddress = addr.Address;
                await db.SaveChangesAsync();
            }
        }
        public static PreparedpaymenttransactionsSmartcontractOutput GetMarketplaceFeeLovelace(Preparedpaymenttransaction preparedpaymenttransaction, Nftproject project)
        {
            if (preparedpaymenttransaction.Overridemarketplacefee != null)
            {
                long res1 = Math.Max(1000000,
                    Convert.ToInt64((preparedpaymenttransaction.Lovelace ?? 0) / 100 * preparedpaymenttransaction.Overridemarketplacefee ?? 1));
                return new PreparedpaymenttransactionsSmartcontractOutput()
                {
                    Address = preparedpaymenttransaction.Overridemarketplaceaddress,
                    Pkh = GlobalFunctions.GetPkhFromAddress(preparedpaymenttransaction.Overridemarketplaceaddress),
                    Lovelace = res1, PreparedpaymenttransactionsId = preparedpaymenttransaction.Id, Type = "marketplace"
                };
            }

            if (project.Marketplacewhitelabelfee == null || project.Marketplacewhitelabelfee == 0)
                return null;

            long res = Math.Max(1000000,
                Convert.ToInt64((preparedpaymenttransaction.Lovelace??0) / 100 * project.Marketplacewhitelabelfee??0));

            var walletaddress = project.CustomerwalletId != null
                ? project.Customerwallet.Walletaddress
                : project.Customer.Adaaddress;

            var pkh = GlobalFunctions.GetPkhFromAddress(walletaddress);
            return new PreparedpaymenttransactionsSmartcontractOutput()
            {
                Address = walletaddress,
                Pkh = pkh,
                Lovelace = res,
                PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                Type = "marketplace"
            };
        }


        public static PreparedpaymenttransactionsSmartcontractOutput GetNmkrSmartcontractFeeLovelace(Preparedpaymenttransaction preparedpaymenttransaction, Nftproject project)
        {
            if (project.Smartcontractssettings.Percentage == 0)
                return null;
            long res = Math.Max(1000000,
                Convert.ToInt64((preparedpaymenttransaction.Lovelace ?? 0) / 100 * project.Smartcontractssettings.Percentage));

            return new PreparedpaymenttransactionsSmartcontractOutput()
            {
                Address = project.Smartcontractssettings.Address,
                Pkh = project.Smartcontractssettings.Pkh,
                Lovelace = res,
                PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                Type = "nmkr"
            };
        }
        public static long GetRoyaltyLovelace(string policyid, long saleamount, out string address)
        {
            address = null;
            var royalty = KoiosFunctions.GetRoyaltiesFromPolicyId(policyid);
            if (royalty == null || royalty.Percentage==0)
                return 0;
            long res = Math.Max(1000000,
                Convert.ToInt64(saleamount / 100 * royalty.Percentage));
            address = royalty.Address;
            return res;
        }


        public static long GetRefererLovelace(EasynftprojectsContext db, int nftprojectid, long saleamount, out string address)
        {
            address = null;
            var referer = GetRefererPkhPercentage(db,nftprojectid);
            if (referer == null || referer.Percentage==0)
                return 0;
            long res = Math.Max(1000000,
                Convert.ToInt64(saleamount / 100 * referer.Percentage));
            address = referer.Address;
            return res;
        }

     

        private static async Task<ISmartContractFieldsInterface> FillJsonJpgStoreSmartcontract(EasynftprojectsContext db, Preparedpaymenttransaction preparedtransaction, long sellerAmount, PkhPercentageClass marketplace, long marketplaceAmount, PkhPercentageClass royalties, long royaltiesAmount, PkhPercentageClass referer, long refererAmount)
        {
            SmartContractFieldsListClass lists = new SmartContractFieldsListClass();

            // Add Seller
            PkhPercentageClass seller = new PkhPercentageClass() { Address = preparedtransaction.Changeaddress, PublicKeyHash = GlobalFunctions.GetPkhFromAddress(preparedtransaction.Changeaddress) };
         
            // Add Markteplace Amounts
            if (marketplace != null && !string.IsNullOrEmpty(marketplace.PublicKeyHash) && marketplaceAmount > 0)
            {
                lists.list.Add(AddJpgStorePayout(marketplaceAmount, marketplace));
                await AddOutput(db, marketplace, marketplaceAmount, "marketplace", preparedtransaction.Id);
            }

            // Add Royalties Amounts
            if (royalties != null && !string.IsNullOrEmpty(royalties.PublicKeyHash) && royaltiesAmount > 0)
            {
                lists.list.Add(AddJpgStorePayout(royaltiesAmount, royalties));
                await AddOutput(db, royalties, royaltiesAmount, "royalties", preparedtransaction.Id);
            }

            // Add Referer Amounts
            if (referer != null && !string.IsNullOrEmpty(referer.PublicKeyHash) && refererAmount > 0)
            {
                lists.list.Add(AddJpgStorePayout(refererAmount, referer));
                await AddOutput(db, referer, refererAmount, "referer", preparedtransaction.Id);
            }
            lists.list.Add(AddJpgStorePayout(sellerAmount, seller));
            await AddOutput(db, seller, sellerAmount, "seller", preparedtransaction.Id);

            return lists;
        }


        private static ISmartContractFieldsInterface AddJpgStorePayout(long amount, PkhPercentageClass pkcClass)
        {
            SmartContractDatumClass listsellerpkh = new SmartContractDatumClass(0, pkcClass.PublicKeyHash);
            SmartContractFieldsClass listsellerstakehash = new SmartContractFieldsClass(0,
                new SmartContractFieldsClass(0, new SmartContractDatumClass(0, pkcClass.StakeKeyHash)));

            SmartContractFieldsClass emptystakehash = new SmartContractFieldsClass(1);

            SmartContractFieldsMapClass fieldsMap = null;
            if (amount == 0 && pkcClass.TokenCount != null)
            {
                var intV = new IntV(pkcClass.TokenCount??1);
                fieldsMap = new SmartContractFieldsMapClass(pkcClass.TokenPolicyId,
                    new SmartContractFieldsIntListClass(0, 0, new SmartContractFieldsMapClass(pkcClass.TokenNameHex, intV)));
            }
            else
            {
                var intV = new IntV(amount);
                fieldsMap = new SmartContractFieldsMapClass("",
                    new SmartContractFieldsIntListClass(0, 0, new SmartContractFieldsMapClass("", intV)));
            }

            var res = new SmartContractFieldsClass(0, new ISmartContractFieldsInterface[]
            {
                string.IsNullOrEmpty(pkcClass.StakeKeyHash)
                    ? new SmartContractFieldsClass(0,
                        new ISmartContractFieldsInterface[] {listsellerpkh, emptystakehash})
                    : new SmartContractFieldsClass(0,
                        new ISmartContractFieldsInterface[] {listsellerpkh, listsellerstakehash}),
                fieldsMap
            });

            return res;
        }


        private static async Task<ISmartContractFieldsInterface> FillJsonNmkrSmartcontract(EasynftprojectsContext db, Preparedpaymenttransaction preparedtransaction, long sellerAmount)
        {
            SmartContractFieldsListClass lists = new SmartContractFieldsListClass();
            PkhPercentageClass seller = new PkhPercentageClass() { Address = preparedtransaction.Changeaddress, PublicKeyHash = GlobalFunctions.GetPkhFromAddress(preparedtransaction.Changeaddress) };
            SmartContractDatumClass listseller = new SmartContractDatumClass(0, seller.PublicKeyHash);
            var intV = new IntV(sellerAmount);
            listseller.Fields.Add(new SmartContractFieldsMapClass("", new SmartContractFieldsMapClass("", intV)));
            lists.list.Add(listseller);

            foreach (var smartcontractOutput in preparedtransaction.PreparedpaymenttransactionsSmartcontractOutputs)
            {
                if (smartcontractOutput.Lovelace <= 0) continue;
                SmartContractDatumClass listelement = new SmartContractDatumClass(0, smartcontractOutput.Pkh);
                var intVmp = new IntV(smartcontractOutput.Lovelace);
                listelement.Fields.Add(new SmartContractFieldsMapClass("", new SmartContractFieldsMapClass("", intVmp)));
                lists.list.Add(listelement);
            }
            
            await AddOutput(db, seller, sellerAmount, "seller", preparedtransaction.Id);

            return lists;
        }

        public static async Task DeleteOutputs(EasynftprojectsContext db, int id, string type)
        {
            string sql =
                $"delete from preparedpaymenttransactions_smartcontract_outputs where preparedpaymenttransactions_id={id} and type='{type}'";

            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql);
        }
        private static async Task DeleteOutputs(EasynftprojectsContext db, int id)
        {
            string sql =
                $"delete from preparedpaymenttransactions_smartcontract_outputs where preparedpaymenttransactions_id={id}";

            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql);
        }

        private static async Task AddOutput(EasynftprojectsContext db, PkhPercentageClass output, long outputAmount, string outputtype, int preparedtransactionId)
        {
            if (string.IsNullOrEmpty(output.Address) || outputAmount == 0)
                return;

            await db.PreparedpaymenttransactionsSmartcontractOutputs.AddAsync(
                new PreparedpaymenttransactionsSmartcontractOutput()
                {
                    Address = output.Address,
                    Pkh = output.PublicKeyHash,
                    PreparedpaymenttransactionsId = preparedtransactionId,
                    Lovelace = outputAmount,
                    Type = outputtype,
                });
            await db.SaveChangesAsync();
            
        }

        private static async Task AddOutput(EasynftprojectsContext db, PreparedpaymenttransactionsSmartcontractOutput output)
        {
            if (output==null)
                return;
            await db.PreparedpaymenttransactionsSmartcontractOutputs.AddAsync(output);
            await db.SaveChangesAsync();
        }

        public static string FillJsonTemplateSellerAuction(EasynftprojectsContext db,
       Preparedpaymenttransaction preparedtransaction, long? bidamount=null, string buyerpkh=null)
        {
            

            var marketplace = GetMarketplacePkhPercentage(db, preparedtransaction.NftprojectId);
            var royalties = GetRoyaltyPkhPercentage(db, preparedtransaction.Policyid);
            var referer = GetRefererPkhPercentage(db, preparedtransaction.NftprojectId);

            List<SmartContractParameters> parameters = new List<SmartContractParameters>();


            var posixtimecreated = new DateTimeOffset(preparedtransaction.Created).ToUnixTimeSeconds();
            var posixtimeends = new DateTimeOffset(preparedtransaction.Created.AddSeconds(Math.Max(1200,preparedtransaction.Auctionduration ?? 1200))).ToUnixTimeSeconds();


            int royaltypercentage = royalties != null ? Convert.ToInt32(royalties.Percentage * 10) : 0;
            int marketplacepercentage = marketplace != null ? Convert.ToInt32(marketplace.Percentage * 10) : 0;
            int refererpercentage = referer != null ? Convert.ToInt32(referer.Percentage * 10) : 0;
            int sellerpercent = 1000 - royaltypercentage - marketplacepercentage - refererpercentage;

            parameters.Add(new SmartContractParameters(){type = "int",intvalue = posixtimecreated});
            parameters.Add(new SmartContractParameters() { type = "int", intvalue = posixtimeends });
            parameters.Add(new SmartContractParameters() { type = "int", intvalue = Math.Max(5000000,preparedtransaction.Auctionminprice??0) });
            parameters.Add(new SmartContractParameters() { type = "bytes", value = preparedtransaction.Policyid });
            parameters.Add(new SmartContractParameters() { type = "bytes", value = preparedtransaction.Tokenname.ToHex() });
            SmartContractDatumClass scldc = new SmartContractDatumClass(0, preparedtransaction.Sellerpkh, parameters.ToArray());


            var m = new SmartContractFieldsMapClass(preparedtransaction.Sellerpkh, new IntV(sellerpercent));
            if (marketplace != null && marketplacepercentage > 0)
                m.Map.Add(new SmartContractKeyBytesValuesFieldsClass(marketplace.PublicKeyHash,
                    new IntV(marketplacepercentage)));
            if (royalties != null && royaltypercentage > 0)
                m.Map.Add(new SmartContractKeyBytesValuesFieldsClass(royalties.PublicKeyHash, new IntV(royaltypercentage)));
            if (referer != null && refererpercentage > 0)
                m.Map.Add(new SmartContractKeyBytesValuesFieldsClass(referer.PublicKeyHash, new IntV(refererpercentage)));

            scldc.Fields.Add(m);


            if (bidamount == null || string.IsNullOrEmpty(buyerpkh))
            {
                scldc.Fields.Add(new SmartContractDatumClass(1, ""));
            }
            else
            {
                List<SmartContractParameters> parameters1 = new List<SmartContractParameters>();
                parameters1.Add(new SmartContractParameters() { type = "bytes", value = buyerpkh });
                parameters1.Add(new SmartContractParameters() { type = "int", intvalue = (long)bidamount });
                var a = new SmartContractDatumClass(0, "", parameters1.ToArray());
                var b = new SmartContractDatumClass(0, "");
                b.Fields.Add(a);
                scldc.Fields.Add(b);
            }

            return JsonConvert.SerializeObject(scldc, Formatting.Indented);
        }



        public static string FillJsonTemplateRedeemer(EasynftprojectsContext db,
         Preparedpaymenttransaction preparedtransaction, string redeemerPkh)
        {
            // If we have have template files for the seperate smart contracts
            if (preparedtransaction.Smartcontracts.Smartcontractsjsontemplates != null)
            {
                var tmpl = preparedtransaction.Smartcontracts.Smartcontractsjsontemplates.FirstOrDefault(x =>
                    x.Templatetype == "buy");
                if (tmpl != null && !string.IsNullOrEmpty(tmpl.Redeemertemplate))
                {
                    return tmpl.Redeemertemplate;
                }
            }

            // If not, create the JSON File

            SmartContractDatumClass scldc = new SmartContractDatumClass(1, null);
            SmartContractFieldsListClass lists = new SmartContractFieldsListClass();

            SmartContractDatumClass listredeemer = new SmartContractDatumClass(0, redeemerPkh);
            var intV = new IntV(preparedtransaction.Tokencount ?? 1);
            listredeemer.Fields.Add(new SmartContractFieldsMapClass(preparedtransaction.Policyid,
                new SmartContractFieldsMapClass(preparedtransaction.Tokenname, intV)));
            lists.list.Add(listredeemer);

            scldc.Fields.Add(lists);

            return JsonConvert.SerializeObject(scldc, Formatting.None);
        }
        public static string FillJsonTemplateCancelRedeemer(EasynftprojectsContext db,
            Preparedpaymenttransaction preparedtransaction, string redeemerPkh)
        {

            // If we have have template files for the seperate smart contracts
            if (preparedtransaction.Smartcontracts.Smartcontractsjsontemplates != null)
            {
                var tmpl = preparedtransaction.Smartcontracts.Smartcontractsjsontemplates.FirstOrDefault(x =>
                    x.Templatetype == "cancel");
                if (tmpl != null && !string.IsNullOrEmpty(tmpl.Redeemertemplate))
                {
                    return tmpl.Redeemertemplate;
                }
            }


            SmartContractDatumClass scldc = new SmartContractDatumClass(0, null);
            return JsonConvert.SerializeObject(scldc, Formatting.None);
        }

        public static string FillJsonTemplateBuyerDirectsaleOffer(Preparedpaymenttransaction preparedtransaction)
        {
          //  await DeleteOutputs(db,preparedtransaction.Id, "buyer");
            SmartContractDatumClass scldc = new SmartContractDatumClass(0, preparedtransaction.Buyerpkh);
            scldc.Fields.Add(FillJsonJpgStoreSmartcontractDirectsaleOffer(preparedtransaction));

            string json = JsonConvert.SerializeObject(scldc, Formatting.Indented);

            return json;
        }

        private static ISmartContractFieldsInterface FillJsonJpgStoreSmartcontractDirectsaleOffer(Preparedpaymenttransaction preparedtransaction)
        {
            SmartContractFieldsListClass lists = new SmartContractFieldsListClass();

            // Add Buyer
            PkhPercentageClass buyer = new PkhPercentageClass()
            {
                Address = preparedtransaction.Changeaddress,
                PublicKeyHash = GlobalFunctions.GetPkhFromAddress(preparedtransaction.Changeaddress),
                TokenCount = preparedtransaction.Tokencount??1, TokenNameHex = preparedtransaction.Tokenname,
                TokenPolicyId = preparedtransaction.Policyid
            };

            foreach (var a in preparedtransaction.PreparedpaymenttransactionsSmartcontractOutputs)
            {
                if (a.Lovelace > 0 && !string.IsNullOrEmpty(a.Pkh) && a.Type!="buyer")
                {
                    lists.list.Add(AddJpgStorePayout(a.Lovelace, new PkhPercentageClass(){Address = a.Address, PublicKeyHash = a.Pkh}));
                }
            }


            lists.list.Add(AddJpgStorePayout(0, buyer));

            return lists;
        }
        internal static string FillJsonTemplateSellerDirectsale(EasynftprojectsContext db,
            Preparedpaymenttransaction preparedtransaction)
        {
            var res = Task.Run(async () => await FillJsonTemplateSellerDirectsaleAsync(db,preparedtransaction));
            return res.Result;
        }

        internal static async Task<string> FillJsonTemplateSellerDirectsaleAsync(EasynftprojectsContext db,
         Preparedpaymenttransaction preparedtransaction)
        {
            await DeleteOutputs(db, preparedtransaction.Id, "seller");

            long lovelace = 0;

            if (string.IsNullOrEmpty(preparedtransaction.Txinforalreadylockedtransactions))
            {
                lovelace = (preparedtransaction.Lovelace ?? 0) +
                           (preparedtransaction.Lockamount ?? 2000000) -
                           preparedtransaction.PreparedpaymenttransactionsSmartcontractOutputs
                               .Where(x => x.Type != "seller").Sum(x => x.Lovelace);
            }

            SmartContractDatumClass scldc = new SmartContractDatumClass(0, preparedtransaction.Sellerpkh);
            switch (preparedtransaction.Smartcontracts.Type)
            {
                case "directsale":
                    scldc.Fields.Add(await FillJsonNmkrSmartcontract(db, preparedtransaction, lovelace));
                    break;
                case "directsaleV2":
                    scldc.Fields.Add(await FillJsonJpgStoreSmartcontractDirectsale(db, preparedtransaction, lovelace));
                    break;
                case "directsaleoffer":
                    scldc.Fields.Add(await FillJsonJpgStoreSmartcontractDirectsale(db, preparedtransaction, lovelace));
                    break;
            }

            string json = JsonConvert.SerializeObject(scldc, Formatting.Indented);

            return json;
        }


        private static async Task<ISmartContractFieldsInterface> FillJsonJpgStoreSmartcontractDirectsale(EasynftprojectsContext db, Preparedpaymenttransaction preparedtransaction, long sellerAmount)
        {
            SmartContractFieldsListClass lists = new SmartContractFieldsListClass();

            // Add Buyer
            PkhPercentageClass seller = new PkhPercentageClass()
            {
                Address = preparedtransaction.Changeaddress,
                PublicKeyHash = GlobalFunctions.GetPkhFromAddress(preparedtransaction.Changeaddress)
            };

            foreach (var a in preparedtransaction.PreparedpaymenttransactionsSmartcontractOutputs.Where(x=>x.Type!="seller" && x.Lovelace>0 && !string.IsNullOrEmpty(x.Pkh)))
            {
                lists.list.Add(AddJpgStorePayout(a.Lovelace,
                    new PkhPercentageClass() {Address = a.Address, PublicKeyHash = a.Pkh}));
            }


            if (sellerAmount > 0)
            {
                lists.list.Add(AddJpgStorePayout(sellerAmount, seller));
                await AddOutput(db, seller, sellerAmount, "seller", preparedtransaction.Id);
            }

            return lists;
        }

        internal static async Task<bool> CheckForCorrectWalletCancel(EasynftprojectsContext db, Preparedpaymenttransaction preparedtransaction, BuyerClass postparameter)
        {
            if (postparameter.Buyer.Addresses == null || !postparameter.Buyer.Addresses.Any())
            {
                await GlobalFunctions.LogExceptionAsync(db, "CheckorCorrectWalletCancel - Error 1",
                    JsonConvert.SerializeObject(postparameter));
                return false;
            }

            var adr = postparameter.Buyer.Addresses.FirstOrDefault(x => x.Address == preparedtransaction.Selleraddress);
            if (adr != null)
                return true;

            var stakeaddress = Bech32Engine.GetStakeFromAddress(preparedtransaction.Selleraddress);
            var stakeaddress2 =
                Bech32Engine.GetStakeFromAddress(postparameter.Buyer.Addresses.First().Address);

            if (stakeaddress2 != stakeaddress)
            {
                await GlobalFunctions.LogExceptionAsync(db, "CheckorCorrectWalletCancel - Error 2",
                    JsonConvert.SerializeObject(postparameter) + Environment.NewLine+stakeaddress + Environment.NewLine+stakeaddress2);

            }

            return (stakeaddress == stakeaddress2);
        }

        internal static async Task<string> GetDefaultProjectUid(EasynftprojectsContext db, string type)
        {
            var sm = await (from a in db.Smartcontracts
                    .Include(a => a.Defaultproject)
                where a.Type == type
                select a).FirstOrDefaultAsync();
            if (sm == null)
                return null;

            return sm.Defaultproject.Uid;
        }
    }
}
