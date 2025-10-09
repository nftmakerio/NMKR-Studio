using System;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using NMKR.Shared.Classes.Customers;
using System.Collections.Generic;
using Asp.Versioning;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.Solana;

namespace NMKR.Api.Controllers.v2.Customer
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class CreateSubcustomerController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public CreateSubcustomerController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Creates a subcustomer
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="customerid">The id of your customer</param>
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
            int customerid, [FromBody] CreateSubcustomerClass createSubcustomerClass)
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
                "", apikey, remoteIpAddress?.ToString() ?? string.Empty);
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

            if (customer.State!="active")
            {
                result.ErrorCode = 125;
                result.ErrorMessage = "Customer not active";
                result.ResultState = ResultStates.Error;
                return StatusCode(403, result);
            }

            var cn = ConsoleCommand.CreateNewPaymentAddress(GlobalFunctions.IsMainnet());
            if (cn.ErrorCode != 0)
            {
                result.ErrorCode = 500;
                result.ErrorMessage = "Internal error";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            CryptographyProcessor cp = new();
            string salt = cp.CreateSalt(30);
            IBlockchainFunctions solanaBlockchainFunctions = new SolanaBlockchainFunctions();
            var solanawallet = solanaBlockchainFunctions.CreateNewWallet();


            IBlockchainFunctions aptosBlockchainFunctions = new AptosBlockchainFunctions();
            var aptoswallet=aptosBlockchainFunctions.CreateNewWallet();


            NMKR.Shared.Model.Customer c = new NMKR.Shared.Model.Customer()
            {
                Email=customer.Email,
                Password = customer.Password,
                Salt = salt,
                Company = customer.Company,
                Firstname = customer.Firstname,
                Lastname = customer.Lastname,
                Street = customer.Street,
                Zip = customer.Zip,
                City = customer.City,
                CountryId = customer.CountryId,
                Ustid = customer.Ustid,
                Confirmationcode = customer.Confirmationcode,
                State = customer.State,
                Created = DateTime.Now,
                Ipaddress = customer.Ipaddress,
                Failedlogon = customer.Failedlogon,
                Twofactor = customer.Twofactor,
                Mobilenumber = customer.Mobilenumber,
                Lockeduntil = customer.Lockeduntil,
                Avatarid = customer.Avatarid,
                Pendingpassword = customer.Pendingpassword,
                Pendingpasswordcreated = customer.Pendingpasswordcreated,
                Sendmailonlogon = customer.Sendmailonlogon,
                Sendmailonlogonfailure = customer.Sendmailonlogonfailure,
                Sendmailonpayout = customer.Sendmailonpayout,
                Sendmailonnews = customer.Sendmailonnews,
                Sendmailonservice = customer.Sendmailonservice,
                Privateskey = Encryption.EncryptString(cn.privateskey, salt),
                Privatevkey = Encryption.EncryptString(cn.privatevkey, salt),
                Adaaddress = cn.Address,
                Lovelace = 0,
                Addressblocked = false,
                Blockcounter = 0,
                Sendmailonsale = customer.Sendmailonsale,
                DefaultsettingsId = customer.DefaultsettingsId,
                MarketplacesettingsId = customer.MarketplacesettingsId,
                Checkaddressalways = true,
                Ftppassword = customer.Ftppassword,
                Referal = customer.Referal,
                Checkaddresscount = 0,
                Lastcheckforutxo = customer.Lastcheckforutxo,
                Comments = "",
                Twofactorenabled = customer.Twofactorenabled,
                Kycaccesstoken = customer.Kycaccesstoken,
                Kycprocessed = customer.Kycprocessed,
                Kycstatus = customer.Kycstatus,
                Checkkycstate = customer.Checkkycstate,
                Showkycstate = customer.Showkycstate,
                Showpayoutbutton = customer.Showpayoutbutton,
                Kycresultmessage = customer.Kycresultmessage,
                Splitroyaltyaddressespercentage = customer.Splitroyaltyaddressespercentage,
                Purchasedmints = 0,
                DefaultpromotionId = customer.DefaultpromotionId,
                Lasttxhash = customer.Lasttxhash,
                Internalaccount = customer.Internalaccount,
                Chargemintandsendcostslovelace = customer.Chargemintandsendcostslovelace,
                Connectedwallettype = "",
                Connectedwalletchangeaddress = "",
                Donotneedtolocktokens = customer.Donotneedtolocktokens,
                Kycprovider = customer.Kycprovider,
                Newpurchasedmints = 0,
                Solanapublickey = solanawallet.Address,
                Solanaseedphrase = Encryption.EncryptString(solanawallet.SeedPhrase, salt),
                Lamports = 0,
                Soladdressblocked = false,
                Sollastcheckforutxo = customer.Sollastcheckforutxo,

                Aptosprivatekey = Encryption.EncryptString(aptoswallet.privateskey, salt),
                Aptosaddress = aptoswallet.Address,
                Aptosseed = Encryption.EncryptString(aptoswallet.SeedPhrase, salt),
                Octas = 0,
                Aptaddressblocked = false,
                Aptlastcheckforutxo = customer.Aptlastcheckforutxo,

                SubcustomerId = customer.Id,
                Subcustomerdescription = createSubcustomerClass.Description,
                Subcustomerexternalid = createSubcustomerClass.ExternalId
            };


            await db.Customers.AddAsync(c);
            await db.SaveChangesAsync();
            c.Email=c.Email.Replace("@", $"@{c.Id}.");
            await db.SaveChangesAsync();

            List<SubcustomerMintcouponPayinAddresses> payin = new List<SubcustomerMintcouponPayinAddresses>
            {
                new SubcustomerMintcouponPayinAddresses()
                {
                    Blockchain = Blockchain.Cardano, Coin = Coin.ADA, Address = c.Adaaddress,
                    PricePerMintCoupon = GlobalFunctions.GetPricePerMintCoupon(db, Blockchain.Cardano, customer.Id),
                    Network = GlobalFunctions.IsMainnet() ? "Mainnet" : "Preprod"
                },
                new SubcustomerMintcouponPayinAddresses()
                {
                    Blockchain = Blockchain.Solana, Coin = Coin.SOL, Address = c.Solanapublickey,
                    PricePerMintCoupon = GlobalFunctions.GetPricePerMintCoupon(db, Blockchain.Solana, customer.Id),
                    Network = GlobalFunctions.IsMainnet() ? "Mainnet" : "Devnet"
                },
                new SubcustomerMintcouponPayinAddresses()
                {
                    Blockchain = Blockchain.Aptos, Coin = Coin.APT, Address = c.Aptosaddress,
                    PricePerMintCoupon = GlobalFunctions.GetPricePerMintCoupon(db, Blockchain.Aptos, customer.Id),
                    Network = GlobalFunctions.IsMainnet() ? "Mainnet" : "Devnet"
                }
            };


            CreateSubcustomerResultClass res = new CreateSubcustomerResultClass()
            {
                Created = c.Created, SubcustomerId = c.Id, MintcouponPayinAddresses = payin.ToArray(),
                Description = createSubcustomerClass.Description,
                ExternalId = createSubcustomerClass.ExternalId
            };

            return Ok(res);

        }

      
    }
}