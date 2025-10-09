using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes.Vesting;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Api.Controllers.v2.VestingAddresses
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetVestingAddressesController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetVestingAddressesController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns all vesting addresses from a customer account
        /// </summary>
        /// <param Name="apikey">The apikey you have created on studio.nmkr.io</param>
        /// <response code="200">Returns the vesting/locking addresses</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetVestingAddressClass[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{customerid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Vesting Addresses"}
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore] [FromHeader(Name = "authorization")] string apikey,
            int customerid)
        {
            //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = customerid.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            var customerid1 = CheckCachedAccess.GetCustomerIdFromApikey(apikey);

            if (customerid1 == null)
            {
                result.ErrorMessage = "The apikey is not valid";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1901;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            if (customerid1 != -1 && customerid1 != customerid)
            {
                result.ErrorMessage = "The apikey is not valid to this customerid";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1902;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            var vestingaddresses = await (from a in db.Lockedassets
                                   where a.CustomerId == customerid && a.State != "deleted" && a.Walletname=="API"
                                                      select new GetVestingAddressClass()
                                                      {
                    Address = a.Lockassetaddress,
                    Created = a.Created,
                    LockedUntil = a.Lockeduntil,
                    LockedUntilSlot = a.Lockslot,
                    UnlockAddress = a.Changeaddress,
                    Pkh = a.Lockwalletpkh,
                    Description = a.Description,
                }).ToArrayAsync();



            return Ok(vestingaddresses);
        }
    }
}
