using System;
using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Asp.Versioning;
using NMKR.Shared.Functions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers
{
    /// <summary>
    /// Get counts controller description
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class GetCountsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetCountsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns the count of the sold, reserved and free nfts (project id)
        /// </summary>
        /// <remarks>
        /// You will get the count of all sold, reserved and free nfts of a particular project
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <response code="200">Returns the NftCountsClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NftCountsClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}/{nftprojectid:int}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey,int nftprojectid)
        {

            if (Request.Method.Equals("HEAD"))
                return null;


            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = nftprojectid.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok( JsonConvert.DeserializeObject<NftCountsClass>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }

            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                 "", apikey, remoteIpAddress?.ToString() ?? string.Empty);
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            NftCountsClass ncc = new();
            var project = (from a in db.Nftprojects
                where a.Id == nftprojectid
                select a).FirstOrDefault();

            if (project != null)
            {
                ncc.NftTotal = project.Total1;

                if (project.Maxsupply == 1)
                {
                    ncc.Free = Math.Max(0, project.Free1 - (project.Nftsblocked ?? 0));
                    ncc.Reserved = project.Reserved1;
                    ncc.Sold = project.Sold1;
                    ncc.Error = project.Error1;
                    ncc.TotalTokens = project.Total1;
                    ncc.Blocked = (project.Blocked1 ?? 0);
                    ncc.TotalBlocked = project.Nftsblocked??0;
                }
                else
                {
                    ncc.Sold = project.Tokenssold1;
                    ncc.Reserved = project.Tokensreserved1;
                    ncc.Free = Math.Max(0, project.Totaltokens1 - project.Tokenssold1 - project.Tokensreserved1 - (project.Nftsblocked ?? 0));
                    ncc.Error = project.Error1;
                    ncc.TotalTokens = project.Totaltokens1;
                    ncc.TotalBlocked = project.Nftsblocked??0;
                }
            }

            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, ncc, apiparameter);
            db.Database.CloseConnection();
            return Ok(ncc);
        }

        /// <summary>
        /// Returns the count of the sold, reserved and free nfts (project uid)
        /// </summary>
        /// <remarks>
        /// You will get the count of all sold, reserved and free nfts of a particular project
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <response code="200">Returns the NftCountsClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NftCountsClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}/{projectuid}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey, string projectuid)
        {

            if (Request.Method.Equals("HEAD"))
                return null;


            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = projectuid;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<NftCountsClass>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }

            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
               "", apikey, remoteIpAddress?.ToString() ?? string.Empty);
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {
                NftCountsClass ncc = new();
                var project = (from a in db.Nftprojects
                    where a.Uid == projectuid
                    select a).FirstOrDefault();

                if (project != null)
                {
                    ncc.NftTotal = project.Total1;

                    if (project.Maxsupply == 1)
                    {
                        ncc.Free = Math.Max(0, project.Free1 - (project.Nftsblocked ?? 0));
                        ncc.Reserved = project.Reserved1;
                        ncc.Sold = project.Sold1;
                        ncc.Error = project.Error1;
                        ncc.TotalTokens = project.Total1;
                        ncc.Blocked = (project.Blocked1 ?? 0);
                        ncc.TotalBlocked = project.Nftsblocked ?? 0;
                    }
                    else
                    {
                        ncc.Sold = project.Tokenssold1;
                        ncc.Reserved = project.Tokensreserved1;
                        ncc.Free = Math.Max(0, project.Totaltokens1 - project.Tokenssold1 - project.Tokensreserved1 - (project.Nftsblocked ?? 0));
                        ncc.Error = project.Error1;
                        ncc.TotalTokens = project.Totaltokens1;
                        ncc.TotalBlocked = project.Nftsblocked ?? 0;
                    }
                }

                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, ncc, apiparameter);
                db.Database.CloseConnection();
                return Ok(ncc);
            }
        }
    }
}
