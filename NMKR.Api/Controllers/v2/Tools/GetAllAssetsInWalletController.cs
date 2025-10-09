using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.Cardano;
using NMKR.Shared.Blockchains.Solana;
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
    public class GetAllAssetsInWalletController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetAllAssetsInWalletController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }



        /// <summary>
        /// Returns all assets that are in a wallet
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="address">The cardano address of the wallet</param>
        /// <response code="200">Returns the AssetsAssociatedWithAccount Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AssetsAssociatedWithAccount[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{address}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string address, [FromQuery]Blockchain blockchain=Blockchain.Cardano)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = address;


            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<AssetsAssociatedWithAccount[]>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                 "", apikey, remoteIpAddress?.ToString() ?? string.Empty);
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var addressvalid=ConsoleCommand.CheckIfAddressIsValid(db, address, GlobalFunctions.IsMainnet(), out string outaddress,
                    out Blockchain blockchain1, true, true) ;

            if (!addressvalid)
            {
                var errorResult = new ApiErrorResultClass
                {
                    ResultState = ResultStates.Error, ErrorMessage = "The address is not valid", ErrorCode = 6635
                };
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, errorResult, apiparameter);
                return StatusCode(406, errorResult);
            }

            if (blockchain1 != blockchain)
            {
                var errorResult = new ApiErrorResultClass
                {
                    ResultState = ResultStates.Error, ErrorMessage = "The address is not valid for this blockchain", ErrorCode = 6636
                };
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, errorResult, apiparameter);
                return StatusCode(406, errorResult);
            }

            if (address.StartsWith("stake") && blockchain == Blockchain.Cardano)
            {
                outaddress = address;
            }

            IBlockchainFunctions? blockchainFunctions = null;
            switch (blockchain)
            {
                case Blockchain.Cardano:
                    blockchainFunctions = new CardanoBlockchainFunctions();
                    break;
                case Blockchain.Solana:
                    blockchainFunctions = new SolanaBlockchainFunctions();
                    break;
                case Blockchain.Aptos:
                    blockchainFunctions= new AptosBlockchainFunctions();
                    break;
            }

            if (blockchainFunctions == null)
            {
                var errorResult = new ApiErrorResultClass
                {
                    ResultState = ResultStates.Error,
                    ErrorMessage = "Unknown Blockchain for this function",
                    ErrorCode = 6353
                };
                return StatusCode(500, errorResult);
            }

         
            var assets = await blockchainFunctions.GetAllAssetsInWalletAsync(_redis, outaddress);

            if (assets == null)
            {
                var errorResult = new ApiErrorResultClass
                {
                    ResultState = ResultStates.Error, ErrorMessage = "No assets found in this wallet", ErrorCode = 6634
                };
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, errorResult, apiparameter);
                return StatusCode(404, errorResult);
            }
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, assets, apiparameter);
            return Ok(assets);
        }
    }
}
