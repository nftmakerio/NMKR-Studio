using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Tools
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetAmountOfSpecificTokenInWalletController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetAmountOfSpecificTokenInWalletController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }



        /// <summary>
        /// Returns the quantity of a specific token in a wallet
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="address">The cardano address of the wallet</param>
        /// <param Name="policyid">The policyid of the token</param>
        /// <param Name="tokenname">The tokenname</param>
        /// <response code="200">Returns the AssetsAssociatedWithAccount Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AssetsAssociatedWithAccount))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{address}/{policyid}/{tokenname}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string address, string policyid, string tokenname)
        {
            return await Post( apikey,policyid, tokenname, new[] {address});
        }




        /// <summary>
        /// Returns the quantity of a specific token in a wallet
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="address">The cardano address of the wallet</param>
        /// <param Name="policyid">The policyid of the token</param>
        /// <param Name="tokenname">The tokenname</param>
        /// <response code="200">Returns the AssetsAssociatedWithAccount Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AssetsAssociatedWithAccount))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{policyid}/{tokenname}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
             string policyid, string tokenname, [FromBody] string[] addresses)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter =string.Join(',', addresses);


            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<AssetsAssociatedWithAccount>(cachedResult.ResultString));
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

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);


            List<AddressStakeClass> newAddressList = new List<AddressStakeClass>();

            foreach (var adr in addresses)
            {
                string stakeaddress = Bech32Engine.GetStakeFromAddress(adr);
                if (newAddressList.FirstOrDefault(x => x.StakeAddress == stakeaddress) == null)
                {
                    newAddressList.Add(new AddressStakeClass {Address = adr, StakeAddress = stakeaddress});
                }
            }

            AssetsAssociatedWithAccount aaww = new AssetsAssociatedWithAccount(policyid, tokenname.ToHex(), 0, Blockchain.Cardano);

            foreach (var adr in newAddressList)
            {
                var assets = (await ConsoleCommand.GetAllAssetsInWalletAsync(_redis, adr.Address)).FirstOrDefault(x => x.PolicyIdOrCollection == policyid && x.AssetNameInHex == tokenname.ToHex());
                if (assets != null)
                {
                    aaww.Quantity += assets.Quantity;
                }
            }


            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, aaww, apiparameter);
            return Ok(aaww);
        }

        private class AddressStakeClass
        {
            public string Address { get; set; }
            public string StakeAddress { get; set; }
        }
    }
}
