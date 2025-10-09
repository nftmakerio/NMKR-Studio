using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Whitelists
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class ManageWhitelistController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public ManageWhitelistController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        /// <summary>
        /// Gets all entries of a projects whitelist
        /// </summary>
        /// <remarks>
        /// With this call you can retrieve all entries of a whitelist of a project (if the project has one)
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of the project</param>
        /// <response code="200">Returns the complete whitelist and how much are already sold</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
        /// <response code="406">Some data are not correct - eg wrong wallet address</response>     
        /// <response code="404">The project was not found in our database or not assiged to your account</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GetWhitelistEntriesClass>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Whitelists" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = "";

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<GetWhitelistEntriesClass>>(cachedResult.ResultString));
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




            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            // Get Project
            var project = await (from a in db.Nftprojects
                                 where a.Uid == projectuid
                                 select a).AsNoTracking().FirstOrDefaultAsync();

            // Reject if not exists
            if (project == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Project not found";
                result.ErrorCode = 404;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }

            // Get Salecondition
            var salecondtion = await (from a in db.Nftprojectsaleconditions
                                      where a.NftprojectId == project.Id && a.Condition == "countedwhitelistedaddresses" &&
                                            a.State == "active"
                                      select a).AsNoTracking().FirstOrDefaultAsync();

            // Reject if not exists
            if (salecondtion == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Whitelist not configured or not active in this project";
                result.ErrorCode = 404;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }

            // Get Address in Whitelist
            var whitelist = await (from a in db.Countedwhitelists
                    .Include(a => a.Countedwhitelistusedaddresses)
                where a.SaleconditionsId == salecondtion.Id
                select new GetWhitelistEntriesClass
                {
                    Addresss = a.Address, Created = a.Created,
                    TotalSoldNftsOrTokens = a.Countedwhitelistusedaddresses.Sum(x => x.Countnft),
                    CountNftsOrTokens = a.Maxcount, Stakeaddress = a.Stakeaddress, SoldNftsOrTokens =
                        (from ax in db.Countedwhitelistusedaddresses
                            where ax.CountedwhitelistId == a.Id
                            select new SoldNftsOrTokensFromWhitelist()
                            {
                                Created = ax.Created, Transactionid = ax.Transactionid, Countnft = ax.Countnft,
                                Originatoraddress = ax.Originatoraddress, Usedaddress = ax.Usedaddress
                            }).ToArray()
                }).AsNoTracking().ToListAsync();


            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, whitelist, apiparameter);
            await db.Database.CloseConnectionAsync();
            return Ok(whitelist);
        }




        /// <summary>
        /// Adds an entry to a projects whitelist
        /// </summary>
        /// <remarks>
        /// With this call you can add an entry to a whitelist of a project (if the project has one)
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of the project</param>
        /// <param Name="address">The address you want to add</param>
        /// <param Name="countofnfts">The count of nfts this address can buy</param>
        /// <response code="200">Returns when the address was added to the whitelist</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
        /// <response code="406">Some data are not correct - eg wrong wallet address</response>     
        /// <response code="404">The project was not found in our database or not assiged to your account</response>            
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{projectuid}/{address}/{countofnfts:long}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Whitelists" }
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid, string address, long countofnfts)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = address;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
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


            

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            // Check Address
            var check = ConsoleCommand.CheckIfAddressIsValid(db, address, GlobalFunctions.IsMainnet(),
                out string outaddress, out Blockchain blockchain, false);

            if (check == false)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Address is not a valid cardano address";
                result.ErrorCode = 406;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return StatusCode(406, result);
            }

            // Get Project
            var project = await (from a in db.Nftprojects
                where a.Uid == projectuid
                select a).AsNoTracking().FirstOrDefaultAsync();

            // Reject if not exists
            if (project == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Project not found";
                result.ErrorCode = 404;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }

            // Get Salecondition
            var salecondtion = await (from a in db.Nftprojectsaleconditions
                where a.NftprojectId == project.Id && a.Condition == nameof(SaleConditionsTypes.countedwhitelistedaddresses) &&
                      a.State == "active"
                select a).AsNoTracking().FirstOrDefaultAsync();

            // Reject if not exists
            if (salecondtion == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Whitelist not configured or not active in this project";
                result.ErrorCode = 404;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }

            // Get Address in Whitelist
            var whitelist = await (from a in db.Countedwhitelists
                where a.SaleconditionsId == salecondtion.Id && a.Address==address
                select a).AsNoTracking().FirstOrDefaultAsync();

            // Reject if already exists
            if (whitelist != null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Address already exists in the whitelist";
                result.ErrorCode = 406;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return StatusCode(406,result);
            }

            await db.Countedwhitelists.AddAsync(new()
            {
                Address = address, SaleconditionsId = salecondtion.Id, Created = DateTime.Now, Maxcount = countofnfts,
                Stakeaddress = Bech32Engine.GetStakeFromAddress(address)
            });
            await db.SaveChangesAsync();


            return Ok();


        }


        /// <summary>
        /// Deletes an entry from a projects whitelist
        /// </summary>
        /// <remarks>
        /// With this call you can delete an entry from a whitelist of a project (if the project has one)
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of the project</param>
        /// <param Name="address">The address you want to delete - use * for all addresses in the list</param>
        /// <response code="200">Returns, when the address was successfully deleted from the list</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
        /// <response code="406">Some data are not correct - eg wrong wallet address</response>     
        /// <response code="404">The project was not found in our database or not assiged to your account</response>            
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpDelete("{projectuid}/{address}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Whitelists" }
        )]
        public async Task<IActionResult> Delete([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid, string address)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;


            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = address;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
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




            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            if (address != "*")
            {
                // Check Address
                var check = ConsoleCommand.CheckIfAddressIsValid(db, address, GlobalFunctions.IsMainnet(),
                    out string outaddress, out Blockchain blockchain, false);

                if (check == false)
                {
                    result.ResultState = ResultStates.Error;
                    result.ErrorMessage = "Address is not a valid cardano address";
                    result.ErrorCode = 406;
                    await db.Database.CloseConnectionAsync();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                    return StatusCode(406, result);
                }
            }


            // Get Project
            var project = await (from a in db.Nftprojects
                                 where a.Uid == projectuid
                                 select a).AsNoTracking().FirstOrDefaultAsync();

            // Reject if not exists
            if (project == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Project not found";
                result.ErrorCode = 404;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }

            // Get Salecondition
            var salecondtion = await (from a in db.Nftprojectsaleconditions
                                      where a.NftprojectId == project.Id && a.Condition == nameof(SaleConditionsTypes.countedwhitelistedaddresses) &&
                                            a.State == "active"
                                      select a).AsNoTracking().FirstOrDefaultAsync();

            // Reject if not exists
            if (salecondtion == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Whitelist not configured or not active in this project";
                result.ErrorCode = 404;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }

            if (address != "*")
            {
                // Get Address in Whitelist
                var whitelist = await (from a in db.Countedwhitelists
                    where a.SaleconditionsId == salecondtion.Id && a.Address == address
                    select a).FirstOrDefaultAsync();

                // Reject if already exists
                if (whitelist == null)
                {
                    result.ResultState = ResultStates.Error;
                    result.ErrorMessage = "Address does not exists in the whitelist";
                    result.ErrorCode = 404;
                    await db.Database.CloseConnectionAsync();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                    return StatusCode(404, result);
                }

                db.Countedwhitelists.Remove(whitelist);
                await db.SaveChangesAsync();
            }
            else
            {
                await GlobalFunctions.ExecuteSqlWithFallbackAsync(db,
                    $"delete from countedwhitelist where saleconditions_id={salecondtion.Id}");
            }

            return Ok();
        }

    }
}
