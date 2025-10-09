using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.PricelistFunctions
{
    public static class GetPricelistClass
    {
        public static async Task<IEnumerable <PricelistClass> > GetPriceList(EasynftprojectsContext db, Nftproject project, IConnectionMultiplexer _redis, bool returnAllPrices = false)
        {
            var pl = await(from a in db.Pricelists
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Settings)
                where a.NftprojectId == project.Id &&

                      (a.Validfrom == null || returnAllPrices || a.Validfrom <= DateTime.Now) &&
                      (a.Validto == null || returnAllPrices || a.Validto >= DateTime.Now) && 

                      a.State == "active"
                select a).ToListAsync();

            List<PricelistClass> pl1 = new();

            foreach (var pricelist in pl)
            {
                pl1.Add(GetSinglePriceFromPricelist(db, project, _redis, pricelist));
            }

            return pl1;
        }

        public static async Task<PricelistClass> GetPriceForCountOfNfts(EasynftprojectsContext db, Nftproject project, IConnectionMultiplexer _redis, long noOfNfts, Coin coin)
        {
            var pl = await (from a in db.Pricelists
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Settings)
                where a.NftprojectId == project.Id && (a.Validfrom == null || a.Validfrom <= DateTime.Now) &&
                      (a.Validto == null || a.Validto >= DateTime.Now) && a.State == "active" && a.Countnftortoken==noOfNfts 
                select a).ToListAsync();

            return pl.Count switch
            {
                0 => null,
                _ => GetSinglePriceFromPricelist(db, project, _redis,
                    pl.FirstOrDefault(x => x.Currency == coin.ToString() || x.Currency== Coin.USD.ToString() || x.Currency== Coin.EUR.ToString() || x.Currency==Coin.JPY.ToString())),
            };
        }


        private static PricelistClass GetSinglePriceFromPricelist(EasynftprojectsContext db, Nftproject project,
            IConnectionMultiplexer _redis, Pricelist pricelist)
        {
            if (pricelist == null) return null;

            double eur = 0;
            double usd = 0;
            double jpy = 0;
            DateTime effdate = DateTime.Now;
            var rates = GlobalFunctions.GetNewRates(_redis, pricelist.Currency.ToEnum<Coin>());
            if (rates != null)
            {
                eur = rates.EurRate;
                usd = rates.UsdRate;
                jpy = rates.JpyRate;
                effdate = rates.EffectiveDate;
            }

            long priceada = 0;
            long pricesolana = 0;
            long priceaptos = 0;
            long pricebitcoin = 0;

            long referenceprice = 0;
            long divider = 1000000;


            if (pricelist.Nftproject.Enabledcoins.Contains(Coin.SOL.ToString()) &&
                (pricelist.Currency == Coin.SOL.ToString() || pricelist.Currency == Coin.USD.ToString() ||
                 pricelist.Currency == Coin.EUR.ToString() || pricelist.Currency == Coin.JPY.ToString()))
            {
                pricesolana = GlobalFunctions.GetPriceinLamport(_redis, pricelist, Coin.SOL);
                referenceprice= pricesolana;
                divider= 1000000000;
                if (rates == null)
                {
                     rates = GlobalFunctions.GetNewRates(_redis, Coin.SOL);
                    if (rates != null)
                    {
                        eur = rates.EurRate;
                        usd = rates.UsdRate;
                        jpy = rates.JpyRate;
                        effdate = rates.EffectiveDate;
                    }
                }
            }

            if (pricelist.Nftproject.Enabledcoins.Contains(Coin.APT.ToString()) &&
                (pricelist.Currency == Coin.APT.ToString() || pricelist.Currency == Coin.USD.ToString() ||
                 pricelist.Currency == Coin.EUR.ToString() || pricelist.Currency == Coin.JPY.ToString()))
            {
                priceaptos = GlobalFunctions.GetPriceinOcta(_redis, pricelist, Coin.APT); 
                referenceprice= priceaptos;
                divider = 100000000;
                if (rates == null)
                {
                    rates = GlobalFunctions.GetNewRates(_redis, Coin.APT);
                    if (rates != null)
                    {
                        eur = rates.EurRate;
                        usd = rates.UsdRate;
                        jpy = rates.JpyRate;
                        effdate = rates.EffectiveDate;
                    }
                }
            }

            if (pricelist.Nftproject.Enabledcoins.Contains(Coin.BTC.ToString()) &&
                (pricelist.Currency == Coin.BTC.ToString() || pricelist.Currency == Coin.USD.ToString() ||
                 pricelist.Currency == Coin.EUR.ToString() || pricelist.Currency == Coin.JPY.ToString()))
            {
                pricebitcoin = GlobalFunctions.GetPriceinSatoshi(_redis, pricelist, Coin.BTC);
                referenceprice = pricebitcoin;
                divider = 100000000;
                if (rates == null)
                {
                    rates = GlobalFunctions.GetNewRates(_redis, Coin.BTC);
                    if (rates != null)
                    {
                        eur = rates.EurRate;
                        usd = rates.UsdRate;
                        jpy = rates.JpyRate;
                        effdate = rates.EffectiveDate;
                    }
                }
            }

            if (pricelist.Nftproject.Enabledcoins.Contains(Coin.ADA.ToString()) &&
                (pricelist.Currency == Coin.ADA.ToString() || pricelist.Currency == Coin.USD.ToString() ||
                 pricelist.Currency == Coin.EUR.ToString() || pricelist.Currency == Coin.JPY.ToString()))
            {
                priceada = GlobalFunctions.GetPriceInEntities(_redis, pricelist);
                referenceprice = priceada;
                divider = 1000000;
                if (rates == null)
                {
                    rates = GlobalFunctions.GetNewRates(_redis, Coin.ADA);
                    if (rates != null)
                    {
                        eur = rates.EurRate;
                        usd = rates.UsdRate;
                        jpy = rates.JpyRate;
                        effdate = rates.EffectiveDate;
                    }
                }
            }

            bool freemint = false;

            // This is the "Hack" for the Free NFT Mints. 2 ADA is the minimum price for the NFTs - But we send it back
            if (pricelist.Nftproject.Enabledcoins.Contains(Coin.ADA.ToString()) &&
                (pricelist.Currency == Coin.ADA.ToString() || pricelist.Currency == Coin.USD.ToString() ||
                 pricelist.Currency == Coin.EUR.ToString() || pricelist.Currency == Coin.JPY.ToString()) &&
                priceada == 0 && !project.Enabledecentralpayments)
            {
                priceada = 2000000;
                freemint = true;
            }

            // This is the "Hack" for the Free NFT Mints. 0,1 Sol is the minimum price for the NFTs - But we send it back
            if (pricelist.Nftproject.Enabledcoins.Contains(Coin.SOL.ToString()) &&
                (pricelist.Currency == Coin.SOL.ToString() || pricelist.Currency == Coin.USD.ToString() ||
                 pricelist.Currency == Coin.EUR.ToString() || pricelist.Currency == Coin.JPY.ToString()) &&
                pricesolana == 0 && !project.Enabledecentralpayments)
            {
                pricesolana = 10000000;
                freemint = true;
            }

            // This is the "Hack" for the Free NFT Mints. 0,5 Apt is the minimum price for the NFTs - But we send it back
            if (pricelist.Nftproject.Enabledcoins.Contains(Coin.APT.ToString()) &&
                (pricelist.Currency == Coin.APT.ToString() || pricelist.Currency == Coin.USD.ToString() ||
                 pricelist.Currency == Coin.EUR.ToString() || pricelist.Currency == Coin.JPY.ToString()) &&
                priceaptos == 0 && !project.Enabledecentralpayments)
            {
                priceaptos = 50000000;
                freemint = true;
            }

            var model = new PaybuttonClass(project)
            {
                Pricelist = pricelist
            };
            long sendback = GlobalFunctions.CalculateSendbackToUser(db, _redis, pricelist.Countnftortoken, pricelist.NftprojectId);



            PricelistClass pl1 = new()
            {
                CountNft = pricelist.Countnftortoken,
                PriceInLovelace = priceada,
                PriceInEur = (float) Math.Round((eur * referenceprice / divider), 2),
                PriceInUsd = (float) Math.Round((usd * referenceprice / divider), 2),
                PriceInJpy = (float) Math.Round((jpy * referenceprice / divider), 2),
                Effectivedate = effdate,
                PaymentGatewayLinkForRandomNftSale = model.PaywindowLink,
                Currency = pricelist.Currency,
                SendBackCentralPaymentInLovelace = sendback,
                PriceInLovelaceCentralPayments = project.Enabledecentralpayments ? priceada+sendback : priceada,
                AdditionalPriceInTokens = GetAdditionalTokens(pricelist),
                PriceInLamport = pricesolana,
                PriceInOctas=priceaptos,
                PriceInSatoshis = pricebitcoin,
                ValidFrom = pricelist.Validfrom,
                ValidTo = pricelist.Validto,
                FreeMint = freemint,
            };
            return pl1;
        }

        private static Tokens[] GetAdditionalTokens(Pricelist pricelist)
        {
            if (pricelist.Priceintoken != null && pricelist.Priceintoken != 0)
            {
                return new Tokens[1]
                {
                    new()
                    {
                        AssetNameInHex = pricelist.Assetnamehex?? pricelist.Tokenassetid.ToHex(),
                        AssetName = pricelist.Tokenassetid,
                        CountToken = (long) pricelist.Priceintoken/(pricelist.Tokenmultiplier??1),
                        PolicyId = pricelist.Tokenpolicyid,
                        TotalCount=(long) pricelist.Priceintoken,
                        Multiplier=pricelist.Tokenmultiplier??1,
                        Decimals=GlobalFunctions.GetDecimalsFromMultiplier(pricelist.Tokenmultiplier),
                    }
                };
            }

            return new Tokens[] { };
        }


    }
}
