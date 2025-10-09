using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class GetProjectDetailsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetProjectDetailsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        /// <summary>
        /// Returns detail information about a project 
        /// </summary>
        /// <remarks>
        /// You will receive all information about this project
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="customerid">Your customer id</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <response code="200">Returns the NftProjectsDetails Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The nft was not found</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NftProjectsDetails))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}/{customerid:int}/{nftprojectid:int}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey,int customerid, int nftprojectid)
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = nftprojectid.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<NftProjectsDetails>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }



            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var proj = (from a in db.Nftprojects
                    .Include(a => a.Customerwallet)
                    .AsSplitQuery()
                    .Include(a => a.Usdcwallet)
                    .AsSplitQuery()
                where a.Id == nftprojectid && a.State != "deleted" && a.CustomerId == customerid
                select a).AsNoTracking().FirstOrDefault();


            if (proj == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Project not found";
                result.ErrorCode = 404;
                db.Database.CloseConnection();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }
           
            var proj1 = new NftProjectsDetails(db, proj);

            db.Database.CloseConnection();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, proj1, apiparameter);
            return Ok(proj1);
        }



        /// <summary>
        /// Returns detail information about a project 
        /// </summary>
        /// <remarks>
        /// You will receive all information about this project
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="uid">The uid of the project</param>
        /// <response code="200">Returns the NftProjectsDetails Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The nft was not found</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NftProjectsDetails))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}/{projectuid}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey, string projectuid)
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = projectuid;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<NftProjectsDetails>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }



            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
               "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var proj = (from a in db.Nftprojects
                    .Include(a => a.Customerwallet)
                    .AsSplitQuery()
                    .Include(a => a.Usdcwallet)
                    .AsSplitQuery()
                        where a.Uid == projectuid && a.State != "deleted" 
                        select a).AsNoTracking().FirstOrDefault();


            if (proj == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Project not found";
                result.ErrorCode = 404;
                db.Database.CloseConnection();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }

            var proj1 = new NftProjectsDetails(db, proj);

            db.Database.CloseConnection();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, proj1, apiparameter);
            return Ok(proj1);
        }


    }
}
