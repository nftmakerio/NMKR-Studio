using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
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
    public class GetSaleConditionsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns the saleconditions for this project (project uid) - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetSaleConditionsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        /// <summary>
        /// Returns the saleconditions for this project (project uid)
        /// </summary>
        /// <remarks>
        /// If you call this funtion, you will get all active saleconditions for this project
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project (not the id)</param>
        /// <response code="200">Returns an array of the GetSaleconditionsClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GetSaleconditionsClass>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Projects" }
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
            string apiparameter = projectuid.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<GetSaleconditionsClass>>(cachedResult.ResultString));
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
            return await GetSaleCondtions(result, new(apikey, apifunction, apiparameter), projectuid);
        }

        internal async Task<IActionResult> GetSaleCondtions(ApiErrorResultClass result, CachedApiCallValues cachedApiCallValues, string projectuid)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var sc = await (from a in db.Nftprojectsaleconditions
                    .Include(a=>a.Countedwhitelists)
                    .ThenInclude(a=>a.Countedwhitelistusedaddresses)
                    .AsSplitQuery()
                    .Include(a => a.Usedaddressesonsaleconditions)
                    .AsSplitQuery()
                where a.Nftproject.Uid ==projectuid && a.State == "active"
                select a).ToListAsync();

            List<GetSaleconditionsClass> pl1 = new();

            foreach (var salecondtion in sc)
            {
                pl1.Add(new()
                {
                    AlreadyUsedAddressOrStakeaddress = GetUsedAddresses(salecondtion),
                    BlacklistedAddresses = string.IsNullOrEmpty(salecondtion.Blacklistedaddresses)?null: salecondtion.Blacklistedaddresses.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries),
                    Description = salecondtion.Description,
                    Condition = salecondtion.Condition.ToEnum<SaleConditionsTypes>(),
                    WhitelistedAddresses = GetWhiteListedAddresses(salecondtion),
                    OnlyOneSalePerWhitelistAddress = salecondtion.Onlyonesaleperwhitlistaddress,
                    MinOrMaxValue = salecondtion.Maxvalue,
                    PolicyId1 = salecondtion.Policyid,
                    PolicyId2 = salecondtion.Policyid2,
                    PolicyId3 = salecondtion.Policyid3,
                    PolicyId4 = salecondtion.Policyid4,
                    PolicyId5 = salecondtion.Policyid5,
                    PolicyId6 = salecondtion.Policyid6,
                    PolicyId7 = salecondtion.Policyid7,
                    PolicyId8 = salecondtion.Policyid8,
                    PolicyId9 = salecondtion.Policyid9,
                    PolicyId10 = salecondtion.Policyid10,
                    PolicyId11 = salecondtion.Policyid11,
                    PolicyId12 = salecondtion.Policyid12,
                    PolicyId13 = salecondtion.Policyid13,
                    PolicyId14 = salecondtion.Policyid14,
                    PolicyId15 = salecondtion.Policyid15,
                    PolicyProjectname = salecondtion.Policyprojectname
                });
            }

            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 200, pl1);
            return Ok(pl1);
        }

        private WhitelistetedCountClass[] GetWhiteListedAddresses(Nftprojectsalecondition salecondtion)
        {
            if (salecondtion.Condition == nameof(SaleConditionsTypes.whitlistedaddresses) &&
                !string.IsNullOrEmpty(salecondtion.Whitlistaddresses))
            {
                var t = salecondtion.Whitlistaddresses.Split(new string[] {"\n"},
                    StringSplitOptions.RemoveEmptyEntries);
                var t1 = (from a in t
                    select new WhitelistetedCountClass()
                    {
                        Address = a, CountNft = 1,
                        StakeAddress = Bech32Engine.GetStakeFromAddress(a)
                    }).ToArray();
                return t1;
            }

            if (salecondtion.Condition == nameof(SaleConditionsTypes.countedwhitelistedaddresses))
            {
                var t1 = (from a in salecondtion.Countedwhitelists
                    select new WhitelistetedCountClass()
                        {Address = a.Address, StakeAddress = a.Stakeaddress, CountNft = a.Maxcount}).ToArray();
                return t1;
            }

            return null;
        }

        public WhitelistetedCountClass[] GetUsedAddresses(Nftprojectsalecondition salecondition)
        {
            switch (salecondition.Condition)
            {
                case nameof(SaleConditionsTypes.whitlistedaddresses):
                {
                    var used = (from a in salecondition.Usedaddressesonsaleconditions
                        select new WhitelistetedCountClass()
                        {
                            Address = a.Address, CountNft = 1,
                            StakeAddress = Bech32Engine.GetStakeFromAddress(a.Address)
                        }).ToArray();
                    return used;
                }
                case nameof(SaleConditionsTypes.countedwhitelistedaddresses):
                {
                    List<WhitelistetedCountClass> used = new();
                    foreach (var salecondtionCountedwhitelist in salecondition.Countedwhitelists)
                    {
                        foreach (var salecondtionCountedwhitelistCountedwhitelistusedaddress in salecondtionCountedwhitelist.Countedwhitelistusedaddresses)
                        {
                            used.Add(new()
                            {
                                Address = salecondtionCountedwhitelistCountedwhitelistusedaddress.Originatoraddress,
                                StakeAddress = salecondtionCountedwhitelist.Stakeaddress,
                                CountNft = salecondtionCountedwhitelistCountedwhitelistusedaddress.Countnft
                            });
                        }
                    }
                    return used.ToArray();
                }
                default:
                    return null;
            }
        }

    }
   
}
