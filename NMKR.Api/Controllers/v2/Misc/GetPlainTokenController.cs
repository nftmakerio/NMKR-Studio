using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes.Plain;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using Trivial.Security;

namespace NMKR.Api.Controllers.v2.Misc
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetPlainTokenController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetPlainTokenController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]

        [HttpGet("{userid:int}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Get(int userid)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);


            var user = await (from a in db.Customers
                where a.Id == userid
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (user == null)
                return StatusCode(406);

            PlainCustomerTokenClass pctc = new PlainCustomerTokenClass()
            {
                Email = new Email() {EmailEmail = user.Email, IsVerified = user.State == "active"}, ExternalId = userid.ToString(),
                FullName = user.Firstname + " " + user.Lastname, ShortName = user.Firstname
            };
            // Sign token with System.IdentityModel.Tokens.Jwt
            // TODO: Replace with actual RSA private key from configuration
            var token=SignToken("YOUR_RSA_PRIVATE_KEY_HERE", pctc);

            return Ok(JsonConvert.SerializeObject(token));
        }

        private string SignToken(string secret, PlainCustomerTokenClass pctc)
        {
            var sign = HashSignatureProvider.CreateHS256(secret);
            var jwt = new JsonWebToken<PlainCustomerTokenClass>(pctc, sign);
            var header = jwt.ToAuthenticationHeaderValue();

            // Revert - not needed here, just for information
            //var jwtSame = JsonWebToken<PlainCustomerTokenClass>.Parse(header.Parameter, sign); // jwtSame.ToEncodedString() == header.Parameter

            return header.Parameter;
        }
    }
}