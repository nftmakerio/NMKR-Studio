using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Asp.Versioning;

namespace NMKR.Api.Controllers.v2.Tools
{
//    [ApiExplorerSettings(IgnoreApi = true)]
   
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    
    public class CheckUtxoController : ControllerBase
    {
        /// <summary>
        /// Returns the utxo of an address
        /// </summary>
        /// <param Name="address">The cardano address of the wallet</param>
        /// <response code="200">Returns the AssetsAssociatedWithAccount Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TxInAddressesClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{address}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Get(string address, [FromQuery] Dataproviders dataprovider=Dataproviders.Default)
        {
            var utxo = await ConsoleCommand.GetNewUtxoAsync(address, dataprovider);
            return Ok(utxo);
        }
}
}
