using System.Linq;
using NMKR.Shared.Classes.Customers;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NMKR.Shared.Enums;
using System.Collections.Generic;
using Asp.Versioning;

namespace NMKR.Api.Controllers.v2.Customer
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetSubcustomersController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetSubcustomersController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns a list with all subcustomers
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="subcustomerid">The id of your customer</param>
        /// <response code="200">Returns the GetTransactionsClass</response>
        /// <response code="201">Returns the results as CSV File</response>
        /// <response code="200">Returns the result as ZIP File</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The project was not found in our database or not assiged to your account</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SubcustomerClass[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{customerid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Customer" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore] [FromHeader(Name = "authorization")] string apikey,
            int customerid)
        {
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = customerid.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
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
                result.ErrorMessage = "Customer id not correct";
                result.ResultState = ResultStates.Error;
                return StatusCode(401, result);
            }

            if (customer.SubcustomerId != null)
            {
                result.ErrorCode = 734;
                result.ErrorMessage = "Customer id not a master customer";
                result.ResultState = ResultStates.Error;
                return StatusCode(401, result);
            }

            var subcustomers = await (from a in db.Customers
                where a.SubcustomerId == customerid
                select a).AsNoTracking().ToArrayAsync();


            List<SubcustomerClass> res = new List<SubcustomerClass>();
            foreach (var a in subcustomers)
            {
                res.Add(new SubcustomerClass
                {
                    Created = a.Created,
                    SubcustomerId = a.Id,
                    Description = a.Subcustomerdescription,
                    ExternalId = a.Subcustomerexternalid,
                    MintCoupons = a.Newpurchasedmints,
                    MintcouponPayinAddresses = GetPayinAddresses(db,a),
                    LoginUsername = a.Email,
                });
            }
            

            return Ok(res);
        }

        private SubcustomerMintcouponPayinAddresses[] GetPayinAddresses(EasynftprojectsContext db, NMKR.Shared.Model.Customer c)
        {
            List<SubcustomerMintcouponPayinAddresses> payin = new List<SubcustomerMintcouponPayinAddresses>
            {
                new()
                {
                    Blockchain = Blockchain.Cardano, Coin = Coin.ADA, Address = c.Adaaddress,
                    PricePerMintCoupon = GlobalFunctions.GetPricePerMintCoupon(db, Blockchain.Cardano, c.Id),
                    Network = GlobalFunctions.IsMainnet() ? "Mainnet" : "Preprod"
                },
                new()
                {
                    Blockchain = Blockchain.Solana, Coin = Coin.SOL, Address = c.Solanapublickey,
                    PricePerMintCoupon = GlobalFunctions.GetPricePerMintCoupon(db, Blockchain.Solana, c.Id),
                    Network = GlobalFunctions.IsMainnet() ? "Mainnet" : "Devnet"
                },
                new()
                {
                    Blockchain = Blockchain.Aptos, Coin = Coin.APT, Address = c.Aptosaddress,
                    PricePerMintCoupon = GlobalFunctions.GetPricePerMintCoupon(db, Blockchain.Aptos, c.Id),
                    Network = GlobalFunctions.IsMainnet() ? "Mainnet" : "Devnet"
                }
            };
            return payin.ToArray();
        }
    }
}
