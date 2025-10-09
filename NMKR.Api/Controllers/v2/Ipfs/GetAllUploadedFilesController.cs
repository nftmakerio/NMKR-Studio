using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.IPFS;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Api.Controllers.v2.Ipfs
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetAllUploadedFilesController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetAllUploadedFilesController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns a list with all uploaded files to IPFS. The files/NFTs that are in a project are not returned
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="customerid">The customerid on NMKR Studio</param>
        /// <param Name="maxCount">The maximum number of elements that are returned. The maximum value is 1000 </param>
        /// <param Name="page">The page no of the results</param>
        /// <response code="200">Returns the GetAllUploadedFilesResult Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">No Image Content was provided. Send a file either as Base64 or as Link or IPFS Hash</response>            
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="409">There is a conflict with the provided images. Send a file either as Base64 or as Link or IPFS Hash</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAllUploadedFilesResultClass[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{customerid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "IPFS" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore] [FromHeader(Name = "authorization")] string apikey,
            int customerid, [FromQuery] int maxCount = 100, [FromQuery] int page = 1)
        {
            // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = "";

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

            var customer = CheckApiAccess.GetCustomer(_redis, db, apikey);
            if (customer == null)
            {
                result.ErrorCode = 124;
                result.ErrorMessage = "Customer id not correct";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (customer.Id != customerid)
            {
                result.ErrorCode = 126;
                result.ErrorMessage = "Customer id not correct";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            if (page<1)
            {
                result.ErrorCode = 852;
                result.ErrorMessage = "Page number must be greater than 0";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            if (maxCount > 1000)
            {
                result.ErrorCode = 853;
                result.ErrorMessage = "MaxCount must be less than 1000";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            var files= await (from a in db.Ipfsuploads
                              where a.CustomerId == customerid
                              select new GetAllUploadedFilesResultClass
                              {
                                  IpfsHash = a.Ipfshash,
                                  Name = a.Name,
                                  FileSize = a.Filesize,
                                  Uploaded = a.Created
                              }).Skip((page-1)* maxCount).Take(maxCount).AsNoTracking().ToArrayAsync();

            return Ok(files);
        }
    }
}
