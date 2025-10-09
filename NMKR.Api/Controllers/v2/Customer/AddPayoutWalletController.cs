using System;
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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

// Apikey testnet: 3dc408b5f7494d8eb25b46633eb279ce

namespace NMKR.Api.Controllers.v2.Customer
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class AddPayoutWalletController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public AddPayoutWalletController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        /// <summary>
        /// Adds a payout wallet to your account
        /// </summary>
        /// <remarks>
        /// With this call you can add a payout wallet in your account. You have to confirm the wallet by clicking the link in the email
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="walletaddress">The address of the wallet you want to add</param>
        /// <response code="200">Returns the Apiresultclass with the information about the address</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
        /// <response code="406">Some data are not correct - eg wrong wallet address</response>     
        /// <response code="404">The project was not found in our database or not assiged to your account</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{walletaddress}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Customer" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string walletaddress)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;


            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = walletaddress;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<CheckAddressResultClass>(cachedResult.ResultString));
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
            var customer = CheckApiAccess.GetCustomer(_redis, db, apikey);
            if (customer == null)
            {
                result.ErrorCode = 124;
                result.ErrorMessage = "Customer not found";
                result.ResultState = ResultStates.Error;
                return StatusCode(401, result);
            }

            var t = await (from a in db.Customerwallets
                where a.Walletaddress == walletaddress && a.CustomerId == customer.Id && a.State != "deleted"
                select a).FirstOrDefaultAsync();
            if (t != null)
            {
                if (t.State == "notactive")
                {
                    if (!string.IsNullOrEmpty(apikey) && !apikey.StartsWith("token") && t.Confirmationvalid < DateTime.Now)
                    {
                        SendEmail(t.Confirmationcode, customer.Email, customer.Id);
                        result.ErrorCode = 128;
                        result.ErrorMessage = "Wallet address already exists - confirmationcode resended";
                        result.ResultState = ResultStates.Error;
                        return StatusCode(406, result);
                    }
                }

                result.ErrorCode = 126;
                result.ErrorMessage = "Wallet address already exists";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (!ConsoleCommand.CheckIfAddressIsValid(db,walletaddress, GlobalFunctions.IsMainnet(), out string outaddress, out Blockchain blockchain))
            {
                result.ErrorCode = 127;
                result.ErrorMessage = "Wallet address is not a valid Cardano Address";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            string confirmationcode = GlobalFunctions.GetGuid();
            Customerwallet cw = new()
            {
                Comment = "Created by API - IP:"+remoteIpAddress.ToString(),
                Walletaddress = walletaddress,
                Created = DateTime.Now,
                CustomerId = customer.Id,
                Ipaddress = remoteIpAddress.ToString(),
                State =  "active",
                Confirmationcode = confirmationcode,
                Confirmationvalid = DateTime.Now.AddMinutes(30)
            };


            db.Customerwallets.Add(cw);
            await db.SaveChangesAsync();
           /* if (!string.IsNullOrEmpty(apikey) && !apikey.StartsWith("token"))
                SendEmail(confirmationcode, customer.Email, customer.Id);*/

            result.ErrorCode = 0;
            result.ErrorMessage = "";
            result.ResultState = ResultStates.Ok;
            return Ok(result);
        }

        public void SendEmail(string code, string email, int userid)
        {
            Dictionary<string, string> d = new()
            {
                {"{confirmationcode}", System.Web.HttpUtility.UrlEncode(code)}
            };
            SendMailClass smc = new();
            smc.SendConfirmationMail(ConfirmationTypes.ConfirmNewWalletAddress, email, d, userid);
        }
    }
}
