using System;
using System.Linq;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes.RoyaltySplitAddresses;
using Swashbuckle.AspNetCore.Annotations;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using NMKR.Shared.Enums;

namespace NMKR.Api.Controllers.v2.RoyaltySplitAddresses
{

    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class UpdateSplitAddressController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public UpdateSplitAddressController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        /// <summary>
        /// Updates a split address
        /// </summary>
        /// <param Name="apikey">The apikey you have created on studio.nmkr.io</param>
        /// <response code="200"></response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetSplitAddressClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPut("{customerid}/{address}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Split Addresses" }
        )]
        public async Task<IActionResult> Put([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, int customerid,string address, [FromBody] CreateSplitAddressClass createSplitAddress)
        {
          //  string apikey = Request.Headers["authorization"];
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

            if (string.IsNullOrEmpty(createSplitAddress.MainAddress))
            {
                result.ErrorMessage = "Mainaddress can not be empty";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1951;
                return StatusCode(406, result);
            }
            if (createSplitAddress.ThresholdInAda < 10)
            {
                result.ErrorMessage = "Threshold should be more than 10 ada";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1952;
                return StatusCode(406, result);
            }
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            var rsa = await (from a in db.Splitroyaltyaddresses
                where a.Address == address && a.CustomerId == customerid
                select a).FirstOrDefaultAsync();

            if (rsa == null)
            {
                result.ErrorMessage = "Split Address not found - or not connected to your account";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1956;
                return StatusCode(406, result);
            }


            if (!ConsoleCommand.CheckIfAddressIsValid(db, createSplitAddress.MainAddress, GlobalFunctions.IsMainnet(), out string address2,
                    out Blockchain blockchain, true, false))
            {
                result.ErrorMessage = "Main address is neither a valid cardano address nor a valid ada handle";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1953;
                return StatusCode(406, result);
            }

            foreach (var split in createSplitAddress.Splits)
            {
                if (!ConsoleCommand.CheckIfAddressIsValid(db, split.Address, GlobalFunctions.IsMainnet(), out string address3,
                        out Blockchain blockchain1, true, false))
                {
                    result.ErrorMessage = "Split address is neither a valid cardano address nor a valid ada handle";
                    result.ResultState = ResultStates.Error;
                    result.ErrorCode = 1954;
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                    return StatusCode(406, result);
                }

                if (split.Percentage <= 0)
                {
                    result.ErrorMessage = "Percentage on split address can not be zero or less";
                    result.ResultState = ResultStates.Error;
                    result.ErrorCode = 1955;
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                    return StatusCode(406, result);
                }
                if (split.Percentage >= 95)
                {
                    result.ErrorMessage = "Percentage on split address can not be greater than 95%";
                    result.ResultState = ResultStates.Error;
                    result.ErrorCode = 1955;
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                    return StatusCode(406, result);
                }
            }



            var cn = ConsoleCommand.CreateNewPaymentAddress(GlobalFunctions.IsMainnet());
            CryptographyProcessor cp = new();
            string salt = cp.CreateSalt(30);


            rsa.Comment = createSplitAddress.Comment;
            rsa.State = createSplitAddress.IsActive ? "active" : "notactive";
            rsa.Minthreshold = createSplitAddress.ThresholdInAda;
            await db.SaveChangesAsync();

            // Delete old splits
            await db.Database.ExecuteSqlRawAsync(
                $"delete from splitroyaltyaddressessplits where splitroyaltyaddresses_id={rsa.Id}");

            Splitroyaltyaddressessplit spas = new()
            {
                Address = createSplitAddress.MainAddress,
                State = "active",
                SplitroyaltyaddressesId = rsa.Id,
                Created = DateTime.Now,
                IsMainReceiver = true,
                Percentage = 100,
            };
            await db.Splitroyaltyaddressessplits.AddAsync(spas);
            await db.SaveChangesAsync();
            await SaveSplits(db, rsa.Id, createSplitAddress);


            var splitaddress = await (from a in db.Splitroyaltyaddresses
                        .Include(a => a.Splitroyaltyaddressessplits)
                                      where a.CustomerId == customerid && a.Id == rsa.Id
                                      select new GetSplitAddressClass()
                                      {
                                          Address = a.Address,
                                          Comment = a.Comment,
                                          Created = a.Created,
                                          Lovelace = a.Lovelace,
                                          Lastcheck = a.Lastcheck,
                                          ThresholdInAda = a.Minthreshold,
                                          IsActive = a.State == "active",
                                          Splits = (from b in a.Splitroyaltyaddressessplits
                                                    select new GetSplits
                                                    {
                                                        OptionalValidFromDate = b.Activefrom,
                                                        Created = b.Created,
                                                        OptionalValidToDate = b.Activeto,
                                                        Address = b.Address,
                                                        IsActive = b.State == "active",
                                                        IsMainReceiver = b.IsMainReceiver,
                                                        Percentage = (b.IsMainReceiver == true ? 100f : b.Percentage / 100f)
                                                    }
                                              ).ToList()
                                      }
                ).AsNoTracking().FirstOrDefaultAsync();


            return Ok(splitaddress);
        }

        private async Task SaveSplits(EasynftprojectsContext db, int id, CreateSplitAddressClass createSplitAddress)
        {
            foreach (var split in createSplitAddress.Splits)
            {
                if (string.IsNullOrEmpty(split.Address))
                    continue;

                Splitroyaltyaddressessplit spas1 = new()
                {
                    Address = split.Address,
                    State = split.IsActive ? "active" : "notactive",
                    SplitroyaltyaddressesId = id,
                    Created = DateTime.Now,
                    IsMainReceiver = false,
                    Percentage = split.PercentageInt,
                    Activefrom = split.OptionalValidFromDate,
                    Activeto = split.OptionalValidToDate
                };
                await db.Splitroyaltyaddressessplits.AddAsync(spas1);
                await db.SaveChangesAsync();
            }
        }
    }
}
