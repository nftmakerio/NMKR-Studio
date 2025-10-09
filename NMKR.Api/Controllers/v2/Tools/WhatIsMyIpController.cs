using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Asp.Versioning;

namespace NMKR.Api.Controllers.v2.Tools
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]

    public class WhatIsMyIpController : ControllerBase
    {
        /// <summary>
        /// Returns the ip address of an api server
        /// </summary>
        /// <response code="200">Returns the Ip Address</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Get()
        {
            var ip = await GlobalFunctions.WhatIsMyIp();
            return Ok(ip);
        }
    }
}