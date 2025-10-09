using System.Linq;
using System.Net;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Api.Controllers.v2.NmkrStores
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetStoreCollectionsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetStoreCollectionsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NmkrStoresCollectionsClass[]))]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{nmkrstorename}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "NMKR Stores" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string nmkrstorename)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");
            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = (HttpContext.Connection.RemoteIpAddress) ?? IPAddress.None;

            string apifunction = this.GetType().Name;
            string apiparameter = nmkrstorename;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<NmkrStoresCollectionsClass[]>(cachedResult.ResultString));
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

            nmkrstorename = nmkrstorename.ToLower().Replace("https://", "");

            string nmkrstorenameSmall = nmkrstorename.Replace(".nmkr.store", "");

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var nmkrStore = await (from a in db.Nftprojects
                        .Include(a => a.Whitelabelstoresettings)
                        .ThenInclude(a => a.Storesettings).AsSplitQuery()
                        .Include(a => a.Whitelabelstorecollections).AsSplitQuery()
                                   where (a.Projecturl == nmkrstorename || a.Projecturl == nmkrstorenameSmall) && a.Projecttype == "marketplace-whitelabel" && a.State == "active"
                                   select a).AsNoTracking().FirstOrDefaultAsync();

            if (nmkrStore == null)
            {
                result.ErrorCode = 3113;
                result.ErrorMessage = "Store not found";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }


            // Check the Apikey again with the projectuid - to prevent access to a foreign project
            result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                nmkrStore.Uid, apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            var collections = (from a in nmkrStore.Whitelabelstorecollections
                    select new NmkrStoresCollectionsClass()
                        {PolicyId = a.Policyid, ShowOnFrontpage = a.Showonfrontpage??false, StoreUid = nmkrStore.Uid})
                .ToArray();
           

            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, collections, apiparameter);
            return Ok(collections);
        }
    }
}
