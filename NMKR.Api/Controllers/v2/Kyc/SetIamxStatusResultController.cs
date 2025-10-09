using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes.IAMX;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace NMKR.Api.Controllers.v2.Kyc
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class SetIamxStatusResultController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public SetIamxStatusResultController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [HttpPost()]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Tools"}
        )]
        public async Task<IActionResult> Post([FromBody] IamxStatusResultClass statusResult)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            if (string.IsNullOrEmpty(statusResult._Id))
            {
                return StatusCode(406, "User id is not valid");
            }
            var user = await (from a in db.Customers
                where a.Kycaccesstoken == statusResult._Id
                select a).FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found");

            if (user.Kycstatus!="PENDING")
                return BadRequest("User state is not pending");

            
            if (statusResult.KycStatus == "approved")
            {
                user.Kycstatus = "GREEN";
                user.Kycresultmessage = JsonConvert.SerializeObject(statusResult);
                user.Kycprocessed=DateTime.Now;
                await db.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
