using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Misc
{
    [ApiExplorerSettings(IgnoreApi = true)]

    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetUserProjectDetailsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetUserProjectDetailsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [HttpGet("{emailaddress}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string emailaddress)
        {
          // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = emailaddress;

            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<NftProjectsDetails>>(cachedResult.ResultString));
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

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);


            var user=await (from a in db.Customers 
                where a.Email == emailaddress 
                select a).AsNoTracking().FirstOrDefaultAsync();


            if (user==null)
            {
                result.ErrorCode = 50;
                result.ErrorMessage = "Customer not found";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, new(apikey, apifunction, apiparameter), 404, result);
                await db.Database.CloseConnectionAsync();
                return NotFound(result);
            }

            var projectdetails = await (from a in db.Nftprojects
                where a.CustomerId == user.Id
                select new NftProjectsDetails(db, a)).AsNoTracking().ToListAsync();

            CheckCachedAccess.SetCachedResult(_redis, new(apikey, apifunction, apiparameter), 200, projectdetails);
            return Ok(projectdetails);
        }
    }
}
