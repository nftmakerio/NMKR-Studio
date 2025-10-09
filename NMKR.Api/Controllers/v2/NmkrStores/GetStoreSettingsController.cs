using System.Collections.Generic;
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
    public class GetStoreSettingsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetStoreSettingsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<string, string>))]
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
           // string apikey = Request.Headers["authorization"];
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
                    return Ok(JsonConvert.DeserializeObject<Dictionary<string, string>>(cachedResult.ResultString));
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

            nmkrstorename = nmkrstorename.ToLower().Replace("https://", "");

            string nmkrstorenameSmall = nmkrstorename.Replace(".nmkr.store", "");

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var nmkrStore = await (from a in db.Nftprojects
                        .Include(a => a.Whitelabelstoresettings)
                        .ThenInclude(a => a.Storesettings).AsSplitQuery()
                        .Include(a => a.Whitelabelstorecollections).AsSplitQuery()
                        .Include(a=>a.Customer).AsSplitQuery()
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
                nmkrStore.Uid, apikey, remoteIpAddress?.ToString() ?? string.Empty);
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            //home_page_collection_policies
            Dictionary<string, string> storeSettings = new Dictionary<string, string>();
            foreach (var storeWhitelabelstoresetting in nmkrStore.Whitelabelstoresettings)
            {
                if (storeWhitelabelstoresetting.Storesettings.Settingsname == "projectId")
                {
                    continue;
                }

                var v1 = storeWhitelabelstoresetting.Value;
                if ((storeWhitelabelstoresetting.Storesettings.Settingstype == "image" ||
                     storeWhitelabelstoresetting.Storesettings.Settingstype == "favicon") &&
                    !v1.ToLower().StartsWith("http"))
                {
                    v1 = GeneralConfigurationClass.IPFSGateway + v1;
                }


                storeSettings.Add(storeWhitelabelstoresetting.Storesettings.Settingsname,
                    v1);
            }


            string v = "";
            foreach (var whitelabelstorecollection in nmkrStore.Whitelabelstorecollections.Where(x => x.Showonfrontpage == true))
            {
                if (v != "")
                    v += ",";
                v += whitelabelstorecollection.Policyid;
            }

            if (!string.IsNullOrEmpty(v))
                storeSettings.Add("home_page_collection_policies", v);


        /*    string customer = nmkrStore.Customer.Company;
            if (string.IsNullOrEmpty(customer))
                customer = nmkrStore.Customer.Firstname + " " + nmkrStore.Customer.Lastname;
        */
            storeSettings.Add(key: "projectUid", nmkrStore.Uid);
            storeSettings.Add(key: "projectId", nmkrStore.Id.ToString());
            if (!storeSettings.ContainsKey("base_url"))
                storeSettings.Add(key: "base_url", $"https://{nmkrstorenameSmall}/nmkr.store");
            if (!storeSettings.ContainsKey("client_name"))
                storeSettings.Add(key: "client_name", nmkrStore.Projectname);
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, storeSettings, apiparameter);
            return Ok(storeSettings);
        }
    }
}
