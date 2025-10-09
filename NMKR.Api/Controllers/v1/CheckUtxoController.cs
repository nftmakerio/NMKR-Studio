using Asp.Versioning;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace NMKR.Api.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]

    public class CheckUtxoController : ControllerBase
    {
        [HttpGet("{address}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string address)
        {

            BuildTransactionClass bt = new();
            var utxo = ConsoleCommand.GetNewUtxo(address);

            return Ok(JsonConvert.SerializeObject(utxo));
        }
}
}
