using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Projects
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class DeleteProjectController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public DeleteProjectController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Deletes a project
        /// </summary>
        /// <remarks>
        /// With this call you can delete a project 
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project (not the id)</param>
        /// <response code="200">Returns the Apiresultclass with the information about the address incl. the assigned NFTs</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The project was not found in our database or not assiged to your account</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiErrorResultClass))]
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
            string apiparameter = projectuid;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<CheckAddressResultClass>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                projectuid, apikey, remoteIpAddress?.ToString() ?? string.Empty);
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            NMKR.Shared.Model.Customer customer = CheckApiAccess.GetCustomer(_redis,db, apikey);

            var nftproject = await (from a in db.Nftprojects
                where a.Uid == projectuid &&  customer.Id == a.CustomerId
                select a).FirstOrDefaultAsync();

            if (nftproject != null)
            {
                var nftscount = await (from a in db.Nfts
                    where a.NftprojectId == nftproject.Id
                    select a).CountAsync();

                if (nftscount > 0)
                {
                    nftproject.State = "deleted";
                    nftproject.Activatepayinaddress = false;
                }
                else
                {
                    db.Nftprojects.Remove(nftproject);
                }

                await db.SaveChangesAsync();
                result.ResultState = ResultStates.Ok;
                return Ok(result);
            }

            result.ResultState = ResultStates.Error;
            result.ErrorCode = 553;
            result.ErrorMessage = "Project UID not found";
            return NotFound(result);
        }
    }
}
