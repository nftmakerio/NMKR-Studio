using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using NMKR.Api.Controllers.SharedClasses;

namespace NMKR.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class GetNftDetailsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetNftDetailsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns detail information about one nft specified by its name (nftproject id)
        /// </summary>
        /// <remarks>
        /// You will receive all information (fingerprint, ipfshash, etc.) about one nft with the submitted name
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="nftname">The Name of the nft you want to receive the details</param>
        /// <response code="200">Returns the Nft Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The nft was not found</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NftDetailsClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}/{nftprojectid:int}/{nftname}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey, int nftprojectid, string nftname)
        {
            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext==null ? IPAddress.None : HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = nftprojectid + nftname;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok( JsonConvert.DeserializeObject<NftDetailsClass>(cachedResult.ResultString));
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

            var project = (from a in db.Nftprojects
                where a.Id == nftprojectid
                select a).FirstOrDefault();

            if (project == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Project not found";
                result.ErrorCode = 404;
                db.Database.CloseConnection();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }

            if (!string.IsNullOrEmpty(project.Tokennameprefix) && nftname.StartsWith(project.Tokennameprefix))
            {
                nftname = nftname.Substring(project.Tokennameprefix.Length);
            }

            var nftx = (from a in db.Nfts
                    .Include(a=>a.Nftproject)
                    .ThenInclude(a=>a.Settings)
                    .AsSplitQuery()
                where a.NftprojectId == nftprojectid &&
                      a.Name == nftname  && a.MainnftId == null
                select a).AsNoTracking().FirstOrDefault();

            if (nftx == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Nft not found";
                result.ErrorCode = 404;
                db.Database.CloseConnection();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }

            var nftdetails = GetNftDetailsClass.GetNftDetails(db, _redis, nftx);

            db.Database.CloseConnection();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, nftdetails, apiparameter);
            return Ok(nftdetails);
        }
    }
}
