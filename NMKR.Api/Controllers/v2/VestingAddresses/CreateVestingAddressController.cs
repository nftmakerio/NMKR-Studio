using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using NMKR.Shared.Classes.Vesting;
using System.Collections.Generic;
using System;
using Asp.Versioning;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Newtonsoft.Json;
using NMKR.Shared.Enums;

namespace NMKR.Api.Controllers.v2.VestingAddresses
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class CreateVestingAddressController : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;

        public CreateVestingAddressController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Creates a vesting/staking address. Assets can be locked on a vesting address for a certain period of time. 
        /// </summary>
        /// <param Name="apikey">The apikey you have created on studio.nmkr.io</param>
        /// <response code="200"></response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateVestingAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{customerid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Vesting Addresses"}
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore] [FromHeader(Name = "authorization")] string apikey,
            int customerid, [FromBody] CreateVestingAddressClass createVestingAddress)
        {
            // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = customerid.ToString();


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                return Unauthorized(result);
            }

            var customerid1 = CheckCachedAccess.GetCustomerIdFromApikey(apikey);

            if (customerid1 == null)
            {
                result.ErrorMessage = "The apikey is not valid";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1901;
                return Unauthorized(result);
            }

            if (customerid1 != -1 && customerid1 != customerid)
            {
                result.ErrorMessage = "The apikey is not valid to this customerid";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1902;
                return Unauthorized(result);
            }


            if (createVestingAddress.LockedUntil < DateTime.Now)
            {
                result.ErrorMessage = "The locked until date must be in the future";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1903;
                return StatusCode(406, result);
            }

            if (createVestingAddress.LockedUntil > DateTime.Now.AddYears(100))
            {
                result.ErrorMessage = "The locked until date must be in the next 100 years";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1904;
                return StatusCode(406, result);
            }

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            if (!ConsoleCommand.CheckIfAddressIsValid(db, createVestingAddress.UnlockAddress, GlobalFunctions.IsMainnet(),
                    out string adaaddress, out Blockchain blockchain, false, false))
            {
                result.ErrorMessage = "The address is not a valid Cardano address";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1905;
                return StatusCode(406, result);
            }


            var pkh = GlobalFunctions.GetPkhFromAddress(createVestingAddress.UnlockAddress);
            TimeSpan difference = createVestingAddress.LockedUntil - DateTime.Now;
            var slot = await ConsoleCommand.GetSlotAsync();
            var lockslot = (slot??0)+ (long)difference.TotalSeconds; 

            PolicyScript ps = new PolicyScript() { Scripts = new List<PolicyScriptScript>() { new() { KeyHash = pkh, Type = "sig" }, new() { Slot = lockslot, Type = "after" } }, Type = "all" };
            var address = ConsoleCommand.CreateSimpleScriptAddress(ps);

            if (string.IsNullOrEmpty(address))
            {
                result.ErrorMessage = "The address could not be created";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1906;
                return StatusCode(406, result);
            }

            var la = new Lockedasset()
            {
                Created = DateTime.Now,
                Changeaddress = createVestingAddress.UnlockAddress,
                Lockwalletpkh = pkh,
                Lockassetaddress = address,
                Lovelace = 0,
                Lockeduntil = createVestingAddress.LockedUntil,
                Policyscript = JsonConvert.SerializeObject(ps),
                Lockslot = lockslot,
                CustomerId = customerid,
                Locktxid = "",
                Walletname = "API",
                Description = createVestingAddress.Description,
            };
            await db.Lockedassets.AddAsync(la);
            await db.SaveChangesAsync();


            CreateVestingAddressResultClass cva = new CreateVestingAddressResultClass()
            {
                Address = address,
                LockedUntil = createVestingAddress.LockedUntil, LockedUntilSlot = lockslot, ActualSlot = slot??0,
                UnlockAddress = createVestingAddress.UnlockAddress, Pkh = pkh, Description = createVestingAddress.Description
            };

            return Ok(cva);
        }
    }
}
