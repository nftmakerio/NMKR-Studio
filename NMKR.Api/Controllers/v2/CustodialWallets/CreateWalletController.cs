using System;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Cardano_Sharp;
using NMKR.Shared.Classes.CustodialWallets;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.CustodialWallets
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class CreateWalletController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public CreateWalletController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Creates an Managed Wallet
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="walletpassword">A password for the access to the wallet. DON'T LOOSE THE PASSWORD. We can not recover your wallet without the password </param>
        /// <param Name="walletname">An optional name for the wallet </param>
        /// <response code="200">Returns the CreateWalletResultClass Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateWalletResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{customerid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Managed Wallets" }
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, int customerid, [FromBody] CreateManagedWalletClass createManagedWallet)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = createManagedWallet.walletname;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }
            var customerid1 = CheckCachedAccess.GetCustomerIdFromApikey(apikey);

            if (customerid1 == null)
            {
                result.ErrorMessage = "The apikey is not valid";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1901;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            if (customerid1 != -1 && customerid1 != customerid)
            {
                result.ErrorMessage = "The apikey is not valid to this customerid";
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 1902;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            if (string.IsNullOrEmpty(createManagedWallet.walletpassword))
            {
                result.ErrorCode = 4439;
                result.ErrorMessage = "walletpassword is empty";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            createManagedWallet.walletpassword = GlobalFunctions.UrlDecode(createManagedWallet.walletpassword);

            if (createManagedWallet.walletpassword.Length < 6)
            {
                result.ErrorCode = 4440;
                result.ErrorMessage = "Walletpassword is to short. Minimum length is 6";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            if (createManagedWallet.walletpassword.Length > 64)
            {
                result.ErrorCode = 4441;
                result.ErrorMessage = "Walletpassword is to long. Maximum length is 64";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var customer = await (from a in db.Customers
                where a.Id == customerid
                                  select a).FirstOrDefaultAsync();

            if (customer == null)
            {
                result.ErrorCode = 4436;
                result.ErrorMessage = "Customer not found";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }

            var cn = CardanoSharpFunctions.CreateNewPaymentAddress(GlobalFunctions.IsMainnet(), createManagedWallet.enterpriseaddress);
            if (cn.ErrorCode != 0)
            {
                result.ErrorCode = cn.ErrorCode;
                result.ErrorMessage = cn.ErrorMessage;
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            string salt = createManagedWallet.walletpassword;
            string password = salt + GeneralConfigurationClass.Masterpassword;

            Custodialwallet custodialwallet = new Custodialwallet()
            {
                Address = cn.Address,
                Uid = GlobalFunctions.GetGuid(),
                Skey = Encryption.EncryptString(cn.privateskey, password),
                Vkey = Encryption.EncryptString(cn.privatevkey, password),
                Walletname = GlobalFunctions.UrlDecode(createManagedWallet.walletname),
                Created = DateTime.Now,
                State = "active",
                CustomerId = customerid,
                Pincode = "",
                Salt = "",
                Seedphrase = Encryption.EncryptString(cn.SeedPhrase, password),
                Wallettype = createManagedWallet.enterpriseaddress ? "enterprise":"base"
            };
            await db.Custodialwallets.AddAsync(custodialwallet);
            await db.SaveChangesAsync();

            CreateWalletResultClass cwrc = new CreateWalletResultClass()
            {
                Address = cn.Address, AdressType = createManagedWallet.enterpriseaddress ? "enterprise" : "base",
                WalletName = createManagedWallet.walletname ?? "",
                Network = GlobalFunctions.GetNetworkName(), SeedPhrase = cn.SeedPhrase,
                Pkh = Convert.ToHexString(cn.pkh)
            };

            return Ok(cwrc);
        }
    }
}
