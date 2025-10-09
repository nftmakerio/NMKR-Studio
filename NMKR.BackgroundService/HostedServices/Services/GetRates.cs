using System;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Cli;
using NMKR.Shared.Model;
using MassTransit;
using StackExchange.Redis;


namespace NMKR.BackgroundService.HostedServices.Services
{
    public class GetRates : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkrates;
            if (server.Checkrates == false || counter % 100 != 0)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);

            await StaticBackgroundServerClass.LogAsync(db, "Catch Rates ", "", serverid);

            string eur = GetWebClass
                .Get(
                    "https://min-api.cryptocompare.com/data/pricemultifull?fsyms=BTC,ADA,SOL,APT,HBAR,SONY,ETH,MATIC&tsyms=EUR,USD,JPY")
                .Result;

            try
            {
                var eurExchangeClass = Newtonsoft.Json.JsonConvert.DeserializeObject<CryptoComparePricesClass>(eur);

                if (eurExchangeClass != null)
                {
                    //obsolete
                    await SetPrice(db, eurExchangeClass.RAW.ADA.EUR.PRICE, 
                        eurExchangeClass.RAW.ADA.USD.PRICE,
                        eurExchangeClass.RAW.ADA.JPY.PRICE,
                        eurExchangeClass.RAW.ADA.USD.PRICE / eurExchangeClass.RAW.BTC.USD.PRICE,

                        eurExchangeClass.RAW.SOL.EUR.PRICE, 
                        eurExchangeClass.RAW.SOL.USD.PRICE,
                        eurExchangeClass.RAW.SOL.JPY.PRICE,
                        eurExchangeClass.RAW.SOL.USD.PRICE / eurExchangeClass.RAW.BTC.USD.PRICE,
                        cancellationToken);
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"ADA Price is: {eurExchangeClass.RAW.ADA.EUR.PRICE}", "", serverid);


                    await SetNewRates(db,eurExchangeClass, cancellationToken);

                }
                else
                {
                    await StaticBackgroundServerClass.LogAsync(db, "Could not receive the ADA Price", "", serverid);
                }
            }
            catch (Exception e)
            {
                await StaticBackgroundServerClass.EventLogException(db, 1, e, serverid);
            }


            // And Cardano Era
            BuildTransactionClass bt = new();
            var tip = CliFunctions.GetQueryTipFromCli(mainnet, ref bt);
            if (tip != null)
            {
                GlobalFunctions.SaveStringToRedis(redis, "Era", tip.Era, 86400);
            }

            

            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private async Task SetNewRates(EasynftprojectsContext db, CryptoComparePricesClass eurExchangeClass, CancellationToken cancellationToken)
        {
            await AddRate(db, Coin.ADA, eurExchangeClass.RAW.ADA);
            await AddRate(db, Coin.SOL, eurExchangeClass.RAW.SOL);
            await AddRate(db, Coin.APT, eurExchangeClass.RAW.APT);
            await AddRate(db, Coin.HBAR, eurExchangeClass.RAW.HBAR);
            await AddRate(db, Coin.ETH, eurExchangeClass.RAW.ETH);
            await AddRate(db, Coin.SONY, eurExchangeClass.RAW.SONY);
            await AddRate(db, Coin.MATIC, eurExchangeClass.RAW.MATIC);
            await AddRate(db, Coin.BTC, eurExchangeClass.RAW.BTC);


            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task AddRate(EasynftprojectsContext db, Coin coin, PRICES prices)
        {
            if (prices==null)
                return;

            await db.Newrates.AddAsync(new Newrate() { Currency = "EUR", Coin = coin.ToString(), Effectivedate = DateTime.UtcNow, Price = prices.EUR.PRICE});
            await db.Newrates.AddAsync(new Newrate() { Currency = "USD", Coin = coin.ToString(), Effectivedate = DateTime.UtcNow, Price = prices.USD.PRICE });
            await db.Newrates.AddAsync(new Newrate() { Currency = "JPY", Coin = coin.ToString(), Effectivedate = DateTime.UtcNow, Price = prices.JPY.PRICE });
        }

        // obsolete
        private async Task SetPrice(EasynftprojectsContext db,double eurorate, double usdrate, double jpyrate, double btcrate, double soleurorate, double solusdrate, double soljpyrate, double solbtcrate, CancellationToken cancellationToken)
        {
            await db.Rates.AddAsync(new() { 
                    Created = DateTime.Now, 
                    Eurorate = (float)eurorate, Usdrate = (float)usdrate, Jpyrate = (float)jpyrate, Btcrate = (float)btcrate ,
                    Soleurorate = (float)soleurorate, Solusdrate = (float)solusdrate, Soljpyrate = (float)soljpyrate, Solbtcrate = (float)solbtcrate },
                cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
