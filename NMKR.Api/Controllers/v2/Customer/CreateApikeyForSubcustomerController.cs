using System;
using System.Linq;
using System.Security.Cryptography;
using NMKR.Shared.Classes.Customers;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace NMKR.Api.Controllers.v2.Customer
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class CreateApikeyForSubcustomerController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public CreateApikeyForSubcustomerController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Creates a subcustomer
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="subcustomerid">The id of your customer</param>
        /// <response code="200">Returns the GetTransactionsClass</response>
        /// <response code="201">Returns the results as CSV File</response>
        /// <response code="200">Returns the result as ZIP File</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The project was not found in our database or not assiged to your account</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateSubcustomerResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{customerid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Customer" }
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore] [FromHeader(Name = "authorization")] string apikey,
            int customerid, [FromBody] CreateSubcustomerApikeyClass createApikeyClass)
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
            if (customer.Id != customerid)
            {
                result.ErrorCode = 126;
                result.ErrorMessage = "Customer id not correct";
                result.ResultState = ResultStates.Error;
                return StatusCode(401, result);
            }

            if (customer.State != "active")
            {
                result.ErrorCode = 125;
                result.ErrorMessage = "Customer not active";
                result.ResultState = ResultStates.Error;
                return StatusCode(403, result);
            }
            var subcustomer = await (from a in db.Customers
                where a.Id == createApikeyClass.SubcustomerId
                select a).AsNoTracking().FirstOrDefaultAsync();
            if (subcustomer == null) {
                result.ErrorCode = 166;
                result.ErrorMessage = "Subcustomer id not correct";
                result.ResultState = ResultStates.Error;
                return StatusCode(401, result);
            }
            if (subcustomer.State != "active")
            {
                result.ErrorCode = 167;
                result.ErrorMessage = "Subcustomer not active";
                result.ResultState = ResultStates.Error;
                return StatusCode(403, result);
            }
            if (subcustomer.SubcustomerId==null)
            {
                result.ErrorCode = 168;
                result.ErrorMessage = "Subcustomer id is not valid";
                result.ResultState = ResultStates.Error;
                return StatusCode(403, result);
            }
            if (subcustomer.SubcustomerId != customer.Id)
            {
                result.ErrorCode = 169;
                result.ErrorMessage = "Subcustomer id is not valid";
                result.ResultState = ResultStates.Error;
                return StatusCode(403, result);
            }


            string newapikey = GlobalFunctions.GetGuid();
            string hash = HashClass.GetHash(SHA256.Create(), newapikey);
            if (createApikeyClass.ExpiryDate>DateTime.Now.AddYears(3))
                createApikeyClass.ExpiryDate = DateTime.Now.AddYears(3);

            NMKR.Shared.Model.Apikey ap = new()
            {
                Created = DateTime.Now,
                Expiration = createApikeyClass.ExpiryDate,
                CustomerId = subcustomer.Id,
                State = "active",
                Uploadnft = true,
                Listnft = true,
                Checkaddresses = true,
                Createprojects = true,
                Purchaserandomnft = true,
                Purchasespecificnft = true,
                Walletvalidation = true,
                Paymenttransactions = true,
                Listprojects = true,
                Apikeyhash = hash,
                Makepayouts = true,
                Comment = createApikeyClass.Description,
                Apikeystartandend = GlobalFunctions.GetStartAndEnd(newapikey)
            };

            await db.Apikeys.AddAsync(ap);
            await db.SaveChangesAsync();

            CreateSubcustomerApikeyResultClass res = new CreateSubcustomerApikeyResultClass()
                { Apikey = newapikey, ExpiryDate = ap.Expiration };


            return Ok(res);
        }
    }
}
