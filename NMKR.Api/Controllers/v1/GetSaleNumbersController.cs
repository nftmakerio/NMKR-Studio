using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers
{
    /// <summary>
    /// Returns the actual sold nfts - internal function
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]

    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class GetSaleNumbersController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns the actual sold nfts - internal function - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetSaleNumbersController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [HttpGet]
        [MapToApiVersion("1")]
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
            var sn = await (from a in db.Salenumbers
                select a).FirstOrDefaultAsync();
                
            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, sn, apiparameter,120);
            return Ok(sn);
        }
    }
}
