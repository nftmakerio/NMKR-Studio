using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Projects
{
    /// <summary>
    /// Returns the saleconditions for this project (project uid)
    /// </summary>
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetDiscountsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns the saleconditions for this project (project uid) - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetDiscountsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        /// <summary>
        /// Returns the discounts for this project (project uid)
        /// </summary>
        /// <remarks>
        /// If you call this function, you will get all active discounts for this project
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project (not the id)</param>
        /// <response code="200">Returns an array of the GetDiscountsClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GetDiscountsClass>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Projects" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = projectuid.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<GetDiscountsClass>>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
               "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }
            return await GetDiscounts(result, new(apikey, apifunction, apiparameter), projectuid);
        }

        internal async Task<IActionResult> GetDiscounts(ApiErrorResultClass result, CachedApiCallValues cachedApiCallValues, string projectuid)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var sc = await (from a in db.Pricelistdiscounts
                    .Include(a => a.Nftproject)
                where a.Nftproject.Uid == projectuid && a.State == "active"
                select new GetDiscountsClass()
                {
                    Condition = a.Condition.ToEnum<PricelistDiscountTypes>(),
                    Description = a.Description, 
                    DiscountInPercent = a.Sendbackdiscount,
                    MinOrMaxValue = (long)a.Minvalue,
                    MinValue1 = a.Minvalue,
                    MinValue2 = a.Minvalue2,
                    MinValue3 = a.Minvalue3,
                    MinValue4 = a.Minvalue4,
                    MinValue5 = a.Minvalue5,
                    PolicyId1 = a.Policyid,
                    PolicyId2 = a.Policyid2,
                    PolicyId3 = a.Policyid3,
                    PolicyId4 = a.Policyid4,
                    PolicyId5 = a.Policyid5,
                    Operator=a.Operator,
                    Couponcode=a.Couponcode,

                }).ToListAsync();

            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 200, sc);
            return Ok(sc);
        }
    }

}
