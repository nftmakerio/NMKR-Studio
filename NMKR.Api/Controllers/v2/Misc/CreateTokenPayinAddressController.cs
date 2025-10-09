using System;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Misc
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class CreateTokenPayinAddressController : Controller
    {
        private readonly IConnectionMultiplexer _redis;

        public CreateTokenPayinAddressController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [HttpGet("{nftprojectid:int}")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, int nftprojectid)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = nftprojectid.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
                return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
               "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            await GlobalFunctions.UpdateLastActionProjectAsync(db,nftprojectid,_redis);

            // Check if there is a free address available
            var freeaddress =await  (from a in db.Premintednftsaddresses
                where a.State == "free" && a.NftprojectId == null
                select a).FirstOrDefaultAsync();

            Premintednftsaddress address = null;

            if (freeaddress == null)
            {
                LogClass.LogMessage(db,
                    "API-CALL:Get Preminted Addresses - NO Free Address available - creating new Address");
                // Create new Address & Keys

                var cn = ConsoleCommand.CreateNewPaymentAddress(GlobalFunctions.IsMainnet());
                if (cn.ErrorCode != 0)
                {
                    LogClass.LogMessage(db,"API-CALL: " + cn.ErrorCode);
                    result.ErrorCode = cn.ErrorCode;
                    result.ErrorMessage = cn.ErrorMessage;
                    result.ResultState = ResultStates.Error;
                    return StatusCode(500, result);
                }


                CryptographyProcessor cp = new();
                string salt = cp.CreateSalt(30);
                string password = salt + GeneralConfigurationClass.Masterpassword;

                Premintednftsaddress newaddress = new()
                {
                    Created = DateTime.Now,
                    State = "reserved",
                    Lovelace = 0,
                    Privatevkey = Encryption.EncryptString(cn.privatevkey, password),
                    Privateskey = Encryption.EncryptString(cn.privateskey, password),
                    Address = cn.Address,
                    Expires = DateTime.Now.AddMinutes(20),
                    NftprojectId = nftprojectid,
                    Salt = salt,
                };

                await db.Premintednftsaddresses.AddAsync(newaddress);
                await db.SaveChangesAsync();
                address = newaddress;
                LogClass.LogMessage(db,"API-CALL: Get Preminted Addresses: " + address.Address);
            }
            else
            {
                freeaddress.NftprojectId = nftprojectid;
                freeaddress.Created = DateTime.Now;
                freeaddress.State = "reserved";
                freeaddress.Lovelace = 0;
                freeaddress.Expires = DateTime.Now.AddMinutes(20);
                await db.SaveChangesAsync();
                address = freeaddress;
                LogClass.LogMessage(db,"API-CALL: Get Preminted Addresses - Found Free Address: " + address.Address);
            }

            GetPaymentAddressResultClass rparc = new() {Expires = (DateTime) address.Expires, PaymentAddress = address.Address, PaymentAddressId = address.Id};

            await db.Database.CloseConnectionAsync();
            return Ok(rparc);
        }
    }
}
