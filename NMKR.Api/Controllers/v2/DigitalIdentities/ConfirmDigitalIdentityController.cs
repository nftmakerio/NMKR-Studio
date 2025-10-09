using System;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.DigitalIdentities
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class ConfirmDigitalIdentityController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public ConfirmDigitalIdentityController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Confirms a digital identity
        /// </summary>
        /// <param Name="identityprovider">The name of the identit provider</param>
        /// <param Name="projectuid">The uid of the project</param>
        /// <param Name="policyid">The policy id</param>
        /// <param Name="ipfslink">The ipfs link</param>
        /// <response code="200">Returns the Nft Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The nft was not found</response>            
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{identityprovider}/{projectuid}/{policyid}/{ipfslink}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Get(string identityprovider, string projectuid, string policyid, string ipfslink)
        {
            if (Request.Method.Equals("HEAD"))
                return null;

            ApiErrorResultClass result = new();

            if (identityprovider != "IAMX")
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Identity provider not found";
                result.ErrorCode = 10001;
                return StatusCode(401,result);
            }

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var project = await (from a in db.Nftprojects
                where a.Uid == projectuid
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (project==null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Project not found";
                result.ErrorCode = 10002;
                return StatusCode(404,result);
            }

            var digident = await (from a in db.Digitalidentities
                where a.NftprojectId == project.Id && a.Policyid == policyid && a.State == "notactive" &&
                      a.Didprovider == identityprovider
                select a).FirstOrDefaultAsync();

            if (digident == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "No identity request found";
                result.ErrorCode = 10003;
                return StatusCode(406,result);
            }

            ipfslink = GlobalFunctions.UrlDecode(ipfslink);
            ipfslink = ipfslink.Replace("ipfs://","");


            digident.State = "active";
            digident.Didresultreceived=DateTime.Now;
            digident.Ipfshash = ipfslink;
            await db.SaveChangesAsync();


            return Ok();
        }
    }
}
