using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.DigitalIdentities
{
    /// <summary>
    /// Returns information about the identities (if the identity token was created) of a project
    /// </summary>
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetIdentityAccountsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;
        /// <summary>
        /// Returns information about the identities (if the identity token was created) of a project
        /// </summary>
        /// <param name="redis"></param>
        public GetIdentityAccountsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns information about the identities (if the identity token was created) of a project
        /// </summary>
        /// <remarks>
        /// You will receive all identities which are connected to this project
        /// </remarks>
        /// <param Name="policyid">The policyid of the project</param>
        /// <response code="200">Returns the Identities (if available)</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The nft was not found</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(IdentityInformationClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{policyid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Projects" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string policyid)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = policyid;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                {
                    string json = JsonConvert.DeserializeObject<string>(cachedResult.ResultString);
                    return Content(json, "application/json");
                }
                else
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


            var identity = await RequestIdentity(policyid);
            if (string.IsNullOrEmpty(identity))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Identity cannot be retrieved";
                result.ErrorCode = 500;
                return StatusCode(500,result);
            }

         

            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, identity, apiparameter);
            return Content(identity, "application/json");
        }
        private async Task<string> RequestIdentity(string policyid)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new("POST"), "https://nftidentityservice.iamx.id/did/lookup");
            request.Content = new StringContent("{\"policyid\":\""+policyid+"\"}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return string.Empty;
            string responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

    }
}
