using System;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Misc
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetSaleNumbersController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetSaleNumbersController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [HttpGet]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Get()
        {

            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = "";
            string apikey = "";

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<Salenumber>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var sn =await (from a in db.Salenumbers
                select a).FirstOrDefaultAsync();
                
            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, sn, apiparameter);
            return Ok(sn);
        }
        [HttpGet("{date}")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Get(DateTime date)
        {

            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = date.ToString();
            string apikey = "";

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<SalenumbersExtendedClass>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var transactions = await (from a in db.Transactions
                    .Include(a=>a.TransactionNfts)
                    .AsSplitQuery()
                where a.Created.Date == date.Date
                select a).ToListAsync();

            var rates = await GlobalFunctions.GetNewRatesAsync(_redis, Coin.ADA);

            SalenumbersExtendedClass sn = new()
            {Soldnfts = transactions.Sum(x => x.TransactionNfts.Count), 
                SoldTokens = transactions.Sum(x => x.TransactionNfts.Sum(x=>x.Tokencount)), 
                CountTransactions = transactions.Count, 
                TotalAda = transactions.Sum(x=>x.Projectada),
                TotalEur = rates.EurRate * transactions.Sum(x => x.Projectada)
            };



            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, sn, apiparameter);
            return Ok(sn);
        }
    }
}
