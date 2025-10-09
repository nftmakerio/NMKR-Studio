using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.BITCOIN;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Projects
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class CreateProjectController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public CreateProjectController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Creates a new Project
        /// </summary>
        /// <remarks>
        /// WIth this Controller you can create a new project
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on studio.nmkr.io</param>
        /// <response code="200">Returns the UploadNftResult Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">No Image Content was provided. Send a file either as Base64 or as Link or IPFS Hash</response>            
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="409">There is a conflict with the provided images. Send a file either as Base64 or as Link or IPFS Hash</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateNewProjectResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Projects" }
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, [FromBody] CreateProjectClassV2 project)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = "";

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
            await GlobalFunctions.LogMessageAsync(db, "API: Create Project " + project.Projectname,
                JsonConvert.SerializeObject(project, Formatting.Indented));


            if (string.IsNullOrEmpty(project.Projectname))
            {
                result.ErrorCode = 101;
                result.ErrorMessage = "Projectname must not be empty";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            var customer = CheckApiAccess.GetCustomer(_redis,db, apikey);
            if (customer == null)
            {
                result.ErrorCode = 124;
                result.ErrorMessage = "Customer not found";
                result.ResultState = ResultStates.Error;
                return StatusCode(401, result);
            }
            string twh = project.TwitterHandle;
            if (!GlobalFunctions.CheckTwitterHandle(ref twh))
            {
                result.ErrorCode = 126;
                result.ErrorMessage = "Twitter handle is not correct";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            project.TwitterHandle = twh;


            if (project.PolicyExpires && project.PolicyLocksDateTime == null)
            {
                result.ErrorCode = 101;
                result.ErrorMessage = "When the policy will expire, submit an expirationdate";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if (project.PolicyExpires && project.PolicyLocksDateTime < DateTime.Now)
            {
                result.ErrorCode = 102;
                result.ErrorMessage = "Policy expiration must be in the future";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if (!string.IsNullOrEmpty(project.TokennamePrefix) && project.TokennamePrefix.Length > 15)
            {
                result.ErrorCode = 103;
                result.ErrorMessage = "Tokennameprefix is too long (max. 15 chars)";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if (!string.IsNullOrEmpty(project.TokennamePrefix) && project.TokennamePrefix.Contains("\""))
            {
                result.ErrorCode = 103;
                result.ErrorMessage = "Quotation marks in the Tokenprefix are not allowed";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if ((project.EnableFiat ?? false) && (customer.Kycstatus != "GREEN" || (await GlobalFunctions.GetWebsiteSettingsBoolAsync(db,"fiatenable")==false)))
            {
                // KYC is not green or FIAT is not enabled
                result.ErrorCode = 126;
                result.ErrorMessage = "Your account is not verified. You can not enable FIAT";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if (string.IsNullOrEmpty(project.StorageProvider))
            {
                project.StorageProvider = "IPFS";
            }

            if (project.StorageProvider.ToUpper() != "IPFS" && project.StorageProvider.ToUpper() != "IAGON")
            {
                result.ErrorCode = 177;
                result.ErrorMessage = "Storage Provider is not correct - Only IPFS or IAGON";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if (project.MetadataStandard != "CIP25" && project.MetadataStandard != "CIP68")
            {
                result.ErrorCode = 178;
                result.ErrorMessage = "Metadata Standard is not correct - Only CIP25 or CIP68";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if (project.MetadataStandard == "CIP68" && (string.IsNullOrEmpty(project.Cip68ReferenceAddress) ||
                                                        !ConsoleCommand.IsValidCardanoAddress(
                                                            project.Cip68ReferenceAddress,
                                                            GlobalFunctions.IsMainnet())))
            {
                result.ErrorCode = 179;
                result.ErrorMessage = "CIP68 Reference Address is not correct";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if (!project.EnableSolana && !project.EnableCardano && !project.EnableAptos && !project.EnableBitcoin)
            {
                result.ErrorCode = 180;
                result.ErrorMessage = "You must enable at least one blockchain";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }



            if (project.Policy != null)
            {
                if (!GlobalFunctions.IsValidJson(project.Policy.PolicyScript, out var formatedmetadata1))
                {
                    result.ErrorCode = 104;
                    result.ErrorMessage = "Policy Script is not a valid JSON Document";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

                if (string.IsNullOrEmpty(project.Policy.PrivateSigningkey))
                {
                    result.ErrorCode = 105;
                    result.ErrorMessage = "Signing Key is not correct";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

                Skeycheck skey = new();
                try
                {
                    skey = JsonConvert.DeserializeObject<Skeycheck>(project.Policy.PrivateSigningkey);
                }
                catch
                {
                    result.ErrorCode = 106;
                    result.ErrorMessage = "Signing Key is not correct";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }
                if (skey==null)
                {
                    result.ErrorCode = 107;
                    result.ErrorMessage = "Signing Key is not correct";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }
                if (skey.Description != "Payment Signing Key")
                {
                    result.ErrorCode = 107;
                    result.ErrorMessage = "Signing Key is not correct";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

                if (string.IsNullOrEmpty(skey.CborHex))
                {
                    result.ErrorCode = 108;
                    result.ErrorMessage = "Signing Key is not correct";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

                Skeycheck vkey = new();
                try
                {
                    vkey = JsonConvert.DeserializeObject<Skeycheck>(project.Policy.PrivateVerifykey);
                }
                catch
                {
                    result.ErrorCode = 109;
                    result.ErrorMessage = "Verification Key is not correct";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

                if (vkey == null)
                {
                    result.ErrorCode = 110;
                    result.ErrorMessage = "Verification Key is not correct";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

                if (vkey.Description != "Payment Verification Key")
                {
                    result.ErrorCode = 110;
                    result.ErrorMessage = "Verification Key is not correct";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

                if (string.IsNullOrEmpty(vkey.CborHex))
                {
                    result.ErrorCode = 111;
                    result.ErrorMessage = "Verification Key is not correct";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }


                var keyhash = ConsoleCommand.GetKeyhash(project.Policy.PrivateVerifykey);
                if (keyhash == "")
                {
                    result.ErrorCode = 112;
                    result.ErrorMessage = "Verification Key is not correct";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

                if (!project.Policy.PolicyScript.Contains(keyhash))
                {
                    result.ErrorCode = 113;
                    result.ErrorMessage = "Policy Script does not match with Verification Key";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

                var policyid = ConsoleCommand.GetPolicyId(project.Policy.PolicyScript);
                if (policyid != project.Policy.PolicyId)
                {
                    result.ErrorCode = 114;
                    result.ErrorMessage = "Policy ID does not match with Policy Script";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

                var oldproj = (from a in db.Nftprojects
                    where a.Policyid == policyid
                    select a).FirstOrDefault();
                if (oldproj != null)
                    project.PolicyLocksDateTime = oldproj.Policyexpire;
            }

            if (string.IsNullOrEmpty(project.MetadataTemplate))
            {
                project.MetadataTemplate = await LoadDefaultMetadata(db, project);
            }


            if (!GlobalFunctions.IsValidJson(project.MetadataTemplate, out var formatedmetadata))
            {
                result.ErrorCode = 115;
                result.ErrorMessage = "Metadata is not a valid JSON object";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }


            var chk1 = new CheckMetadataForCip25Fields();
            var checkmetadata = chk1.CheckMetadata(project.MetadataTemplate, null, "", true, false);
            if (!checkmetadata.IsValid)
            {
                result.ErrorCode = 115;
                result.ErrorMessage = checkmetadata.ErrorMessage;
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }



            int? payoutwallet = null;
            if (project.EnableCardano)
            {
                if (!string.IsNullOrEmpty(project.PayoutWalletaddress))
                {

                    if (!ConsoleCommand.IsValidCardanoAddress(project.PayoutWalletaddress,GlobalFunctions.IsMainnet()))
                    {
                        result.ErrorCode = 1250;
                        result.ErrorMessage = $"Payout wallet is not a valid Cardano {(GlobalFunctions.IsMainnet()?"Mainnet":"Preprod")} wallet";
                        result.ResultState = ResultStates.Error;
                        await db.Database.CloseConnectionAsync();
                        return StatusCode(406, result);
                    }


                    var pow = (from a in db.Customerwallets
                        where a.Walletaddress == project.PayoutWalletaddress && a.CustomerId == customer.Id &&
                              a.State != "deleted" && a.Cointype == Coin.ADA.ToString()
                               select a).FirstOrDefault();

                    if (pow == null)
                    {
                        string confirmationcode = GlobalFunctions.GetGuid();
                        pow = new()
                        {
                            Comment = "Created by API Call",
                            Walletaddress = project.PayoutWalletaddress,
                            Created = DateTime.Now,
                            CustomerId = customer.Id,
                            Ipaddress = "",
                            State = "active", //apikey.StartsWith("token") ? "active" : "notactive",
                            Confirmationcode = confirmationcode,
                            Cointype = Coin.ADA.ToString(),
                            Confirmationvalid = DateTime.Now.AddMinutes(30),
                        };


                        await db.Customerwallets.AddAsync(pow);
                        await db.SaveChangesAsync();
                    }

                    payoutwallet = pow.Id;
                }
                else
                {
                    if (!apikey.Contains("token"))
                    {
                        result.ErrorCode = 120;
                        result.ErrorMessage = "You must specify a cardano payout wallet";
                        result.ResultState = ResultStates.Error;
                        await db.Database.CloseConnectionAsync();
                        return StatusCode(406, result);
                    }
                }
            }


            int? payoutwalletsolana = null;
            if (project.EnableSolana)
            {
                if (project.EnableSolana && !string.IsNullOrEmpty(project.PayoutWalletaddressSolana))
                {
                    if (!SolanaFunctions.IsValidSolanaPublicKey(project.PayoutWalletaddressSolana))
                    {
                        result.ErrorCode = 1250;
                        result.ErrorMessage = $"Payout wallet is not a valid Solana wallet";
                        result.ResultState = ResultStates.Error;
                        await db.Database.CloseConnectionAsync();
                        return StatusCode(406, result);
                    }

                    var pow = (from a in db.Customerwallets
                        where a.Walletaddress == project.PayoutWalletaddressSolana && a.CustomerId == customer.Id &&
                              a.State != "deleted" && a.Cointype == Coin.SOL.ToString()
                        select a).FirstOrDefault();

                    if (pow == null)
                    {
                        string confirmationcode = GlobalFunctions.GetGuid();
                        pow = new()
                        {
                            Comment = "Created by API Call",
                            Walletaddress = project.PayoutWalletaddressSolana,
                            Created = DateTime.Now,
                            CustomerId = customer.Id,
                            Ipaddress = "",
                            State = "active",
                            Confirmationcode = confirmationcode,
                            Cointype = Coin.SOL.ToString(),
                            Confirmationvalid = DateTime.Now.AddMinutes(30),
                        };


                        await db.Customerwallets.AddAsync(pow);
                        await db.SaveChangesAsync();
                    }

                    payoutwalletsolana = pow.Id;
                }
                else
                {
                    result.ErrorCode = 120;
                    result.ErrorMessage = "You must specify a Solana payout wallet";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }
            }

            int? payoutwalletaptos = null;
            if (project.EnableAptos)
            {
                if (customer.Purchasedmints < 1)
                {
                    result.ErrorCode = 4332;
                    result.ErrorMessage = $"You need at least one mint coupon for the aptos collection";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

                if (string.IsNullOrEmpty(project.AptosCollectionName))
                {
                    result.ErrorCode = 1244;
                    result.ErrorMessage = $"Aptos Collection Name is required";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

                if (!string.IsNullOrEmpty(project.PayoutWalletaddressAptos))
                {
                    IBlockchainFunctions aptos = new AptosBlockchainFunctions();

                    if (!aptos.CheckForValidAddress(project.PayoutWalletaddressAptos, GlobalFunctions.IsMainnet()))
                    {
                        result.ErrorCode = 1250;
                        result.ErrorMessage = $"Payout wallet is not a valid Aptos wallet";
                        result.ResultState = ResultStates.Error;
                        await db.Database.CloseConnectionAsync();
                        return StatusCode(406, result);
                    }

                    var pow = (from a in db.Customerwallets
                        where a.Walletaddress == project.PayoutWalletaddressAptos && a.CustomerId == customer.Id &&
                              a.State != "deleted" && a.Cointype == Coin.APT.ToString()
                        select a).FirstOrDefault();

                    if (pow == null)
                    {
                        string confirmationcode = GlobalFunctions.GetGuid();
                        pow = new()
                        {
                            Comment = "Created by API Call",
                            Walletaddress = project.PayoutWalletaddressAptos,
                            Created = DateTime.Now,
                            CustomerId = customer.Id,
                            Ipaddress = "",
                            State = "active",
                            Confirmationcode = confirmationcode,
                            Cointype = Coin.APT.ToString(),
                            Confirmationvalid = DateTime.Now.AddMinutes(30),
                        };


                        await db.Customerwallets.AddAsync(pow);
                        await db.SaveChangesAsync();
                    }

                    payoutwalletaptos = pow.Id;
                }
                else
                {
                    result.ErrorCode = 120;
                    result.ErrorMessage = "You must specify a Aptos payout wallet";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }
            }

            int? payoutwalletbitcoin = null;
            if (project.EnableBitcoin)
            {
                if (!string.IsNullOrEmpty(project.PayoutWalletaddressBitcoin))
                {
                    IBlockchainFunctions bitcoin = new BitcoinBlockchainFunctions();

                    if (!bitcoin.CheckForValidAddress(project.PayoutWalletaddressBitcoin, GlobalFunctions.IsMainnet()))
                    {
                        result.ErrorCode = 1250;
                        result.ErrorMessage = $"Payout wallet is not a valid Bitcoin wallet";
                        result.ResultState = ResultStates.Error;
                        await db.Database.CloseConnectionAsync();
                        return StatusCode(406, result);
                    }

                    var pow = (from a in db.Customerwallets
                               where a.Walletaddress == project.PayoutWalletaddressBitcoin && a.CustomerId == customer.Id &&
                                     a.State != "deleted" && a.Cointype == Coin.BTC.ToString()
                               select a).FirstOrDefault();

                    if (pow == null)
                    {
                        string confirmationcode = GlobalFunctions.GetGuid();
                        pow = new()
                        {
                            Comment = "Created by API Call",
                            Walletaddress = project.PayoutWalletaddressBitcoin,
                            Created = DateTime.Now,
                            CustomerId = customer.Id,
                            Ipaddress = "",
                            State = "active",
                            Confirmationcode = confirmationcode,
                            Cointype = Coin.BTC.ToString(),
                            Confirmationvalid = DateTime.Now.AddMinutes(30),
                        };


                        await db.Customerwallets.AddAsync(pow);
                        await db.SaveChangesAsync();
                    }

                    payoutwalletbitcoin = pow.Id;
                }
                else
                {
                    result.ErrorCode = 120;
                    result.ErrorMessage = "You must specify a Bitcoin payout wallet";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }
            }


            if (project.AddressExpiretime < 5 || project.AddressExpiretime > 60)
            {
                result.ErrorCode = 118;
                result.ErrorMessage = "Address expire time must be between 5 and 60 minutes";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if (project.MaxNftSupply < 1 )
            {
                result.ErrorCode = 119;
                result.ErrorMessage = "MaxNftSupply must be between 1 and "+ long.MaxValue;
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }


            var r1 =  await CreateNewProject(db, project, apikey,customer, payoutwallet,payoutwalletsolana, payoutwalletaptos,payoutwalletbitcoin, null);
            if (r1.result.ResultState != ResultStates.Ok)
                return StatusCode(406, r1);

            await db.Database.CloseConnectionAsync();
            return Ok(r1.cnprc);
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
        private class CreateProjectResultClass
        {
            public ApiErrorResultClass result { get; set; }
            public CreateNewProjectResultClass cnprc { get; set; }
        }
        private async Task<CreateProjectResultClass> CreateNewProject(EasynftprojectsContext db, CreateProjectClassV2 project, string apikey, NMKR.Shared.Model.Customer customer, int? payoutwallet,int?payoutwalletsolana,int? payoutwalletaptos, int? payoutwalletbitcoin, int? customerwalletusdc)
        {
            ApiErrorResultClass result = new() { ErrorCode = 0, ErrorMessage = "", ResultState = ResultStates.Ok };
            CreateProjectResultClass result1 = new()
                {cnprc = new(), result = result};

            var qt = await ConsoleCommand.GetSlotAsync();

            if (qt is null or 0)
            {
                result.ErrorCode = 120;
                result.ErrorMessage = "Internal error while quering slot";
                result.ResultState = ResultStates.Error;
                return result1;
            }

            string policyid = "";
            string policscript = "";
            string skey = "";
            string vkey = "";
            string address = "";
            long? slot = null;
            if (project.Policy != null)
            {

                policyid = project.Policy.PolicyId;
                policscript = project.Policy.PolicyScript;
                skey = project.Policy.PrivateSigningkey;
                vkey = project.Policy.PrivateVerifykey;
                address = ConsoleCommand.CreatePaymentAddressFromKeyfile(vkey, GlobalFunctions.IsMainnet()); 

                project.PolicyExpires = false;
                PolicyScript ps = JsonConvert.DeserializeObject<PolicyScript>(policscript);
                if (ps != null && ps.Scripts != null && ps.Scripts.Any())
                {
                    var t = ps.Scripts.Find(x => x.Type == "before");
                    if (t != null)
                    {
                        slot = t.Slot;
                        var s1 = slot - qt;
                        project.PolicyLocksDateTime = DateTime.Now.AddSeconds((long)s1);
                        project.PolicyExpires = true;
                    }
                }

            }
            else
            {
                var cn = ConsoleCommand.CreateNewPaymentAddress(GlobalFunctions.IsMainnet());

                if (cn.ErrorCode != 0)
                {
                    result.ErrorCode = 121;
                    result.ErrorMessage = "Internal error while creating Policy Address";
                    result.ResultState = ResultStates.Error;
                    return result1;
                }


                var keyhash = ConsoleCommand.GetKeyhash(cn.privatevkey);
                if (string.IsNullOrEmpty(keyhash))
                {
                    result.ErrorCode = 122;
                    result.ErrorMessage = "Internal Error while creating Policy Keyhash";
                    result.ResultState = ResultStates.Error;
                    return result1;
                }

                PolicyScript ps = new() { Type = "all" };
                List<PolicyScriptScript> ls = new();
                ls.Add(new() { KeyHash = keyhash, Type = "sig" });
                if (project.PolicyExpires && project.PolicyLocksDateTime != null)
                {
                    var diffInSeconds = (((DateTime)(project.PolicyLocksDateTime)).Date.AddMinutes(1) - DateTime.Now).TotalSeconds;
                    slot = (long)qt + (long)diffInSeconds;
                    ls.Add(new() { Type = "before", Slot = slot });
                }

                ps.Scripts = ls;
                policscript = JsonConvert.SerializeObject(ps);

                policyid = ConsoleCommand.GetPolicyId(policscript);
                if (string.IsNullOrEmpty(policyid))
                {
                    result.ErrorCode = 123;
                    result.ErrorMessage = "Internal Error while creating Policy ID";
                    result.ResultState = ResultStates.Error;
                    return result1;
                }

                address = cn.Address;
                vkey = cn.privatevkey;
                skey = cn.privateskey;
            }

            SaveProjectFunctions spf = new SaveProjectFunctions();

            result = spf.CheckSaleConditions(db, project.SaleConditions, result);
            if (result.ResultState == ResultStates.Error)
                return result1;

            result = spf.CheckPricelists(db, project.Pricelist,customer, result);
            if (result.ResultState == ResultStates.Error)
                return result1;

            result = spf.CheckDiscounts(db, project.Discounts, result);
            if (result.ResultState == ResultStates.Error)
                return result1;


            result = await CheckAdditionalPayoutWallets(db, project,customer, result);
            if (result.ResultState == ResultStates.Error)
                return result1;

            result = spf.CheckNotifications(db, project.Notifications, result);
            if (result.ResultState == ResultStates.Error)
                return result1;

            IBlockchainFunctions solana = new SolanaBlockchainFunctions();
            var solanaaddress = solana.CreateNewWallet();

            IBlockchainFunctions aptos = new AptosBlockchainFunctions();
            var aptosaddres= aptos.CreateNewWallet();

            IBlockchainFunctions bitcoin = new BitcoinBlockchainFunctions();
            var bitcoinaddres = bitcoin.CreateNewWallet();

            var strategy = db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var dbContextTransaction = await db.Database.BeginTransactionAsync();
                CryptographyProcessor cp = new();
                string password = cp.CreateSalt(30);
                Nftproject pro = new()
                {
                    CustomerId = (int) customer.Id,
                    Password = password,
                    Description = project.Description,
                    Minutxo = project.CardanoSendbackToCustomer.ToString(),
                    State = "active",
                    Projectname = project.Projectname,
                    Projecturl = project.Projecturl,
                    Policyexpire = project.PolicyExpires ? project.PolicyLocksDateTime : null,
                    Policyaddress = address,
                    Policyskey = Encryption.EncryptString(skey, password),
                    Policyvkey = Encryption.EncryptString(vkey, password),
                    Policyscript = policscript,
                    Policyid = policyid,
                    Maxsupply = Convert.ToInt32(project.MaxNftSupply), // TODO: Change to long
                    Version = "1.0",
                    CustomerwalletId = payoutwallet,
                    SolanacustomerwalletId = payoutwalletsolana,
                    Tokennameprefix = project.TokennamePrefix ??= "",
                    Metadata = JsonFormatter.FormatJson(project.MetadataTemplate),
                    Expiretime = project.AddressExpiretime,
                    Created = DateTime.Now,
                    SettingsId = customer.DefaultsettingsId, // Default Settings
                    Uid = Guid.NewGuid().ToString(),
                    Lockslot = slot,
                    Enablefiat = customer.Kycstatus == "GREEN" && (project.EnableFiat ?? false),
                    Enabledecentralpayments = project.EnableDecentralPayments ?? false,
                    Enablecrosssaleonpaywindow = false, // project.EnableCrossSaleOnPaymentgateway,
                    Activatepayinaddress = project.ActivatePayinAddress ?? false,
                    Paymentgatewaysalestart = project.Paymentgatewaysalestart,
                    //   UsdcwalletId = customerwalletusdc,
                    DefaultpromotionId = customer.DefaultpromotionId,
                    Twitterhandle = project.TwitterHandle,
                    Cip68 = project.MetadataStandard == "CIP68",
                    Cip68referenceaddress = project.Cip68ReferenceAddress,
                    Cip68extrafield = project.Cip68ExtraField,
                    Solanapublickey = solanaaddress.Address,
                    Solanaseedphrase = Encryption.EncryptString(solanaaddress.SeedPhrase, password),
                    Solanacollectionfamily = project.SolanaCollectionFamily,
                    Solanasymbol = project.SolanaSymbol,
                    Solanacollectionimage = project.SolanaCollectionImageUrl,
                    Solanacollectionimagemimetype = project.SolanaCollectionImageMimeType,
                    Aptoscollectionimage = project.AptosCollectionImageUrl,
                    Aptoscollectionimagemimetype = project.AptosCollectionImageMimeType,
                    Aptoscollectionname = project.AptosCollectionName,
                    Enablecardano = project.EnableCardano, // deprecated
                    Enablesolana = project.EnableSolana,  // deprecated
                    Enabledcoins = (project.EnableCardano ? Coin.ADA+" " : "") + (project.EnableSolana ? Coin.SOL+" " : "") + (project.EnableAptos ? Coin.APT + " " : "") + (project.EnableBitcoin ? Coin.BTC + " " : ""),
                    Solanacollectiontransaction = project.SolanaCreateVerifiedCollection==true ? "<PENDING>" :null,
                    Aptosaddress = aptosaddres.Address,
                    Aptosseedphrase = Encryption.EncryptString(aptosaddres.SeedPhrase, password),
                    Aptospublickey = aptosaddres.privatevkey,
                    AptoscustomerwalletId = payoutwalletaptos,
                    Aptoscollectioncreated=null,
                    Aptoscollectiontransaction = project.EnableAptos?"<PENDING>":null,

                    Bitcoinaddress = bitcoinaddres.Address,
                    Bitcoinseedphrase = Encryption.EncryptString(bitcoinaddres.SeedPhrase, password),
                    Bitcoinpublickey = bitcoinaddres.privatevkey,
                    Bitcoinprivatekey = bitcoinaddres.privateskey,
                    BitcoincustomerwalletId = payoutwalletbitcoin,

                };




                await db.Nftprojects.AddAsync(pro);
                await db.SaveChangesAsync();



                if (project.EnableCardano)
                {
                    result1.cnprc.PolicyScript = pro.Policyscript;
                    result1.cnprc.PolicyExpiration = pro.Policyexpire;
                    result1.cnprc.Metadata = pro.Metadata;
                    result1.cnprc.ProjectId = pro.Id;
                    result1.cnprc.PolicyId = pro.Policyid;
                }

                if (project.EnableSolana)
                {
                    IBlockchainFunctions solana = new SolanaBlockchainFunctions();
                    var mdSolana = solana.GetMetadataFromCip25Metadata(pro.Metadata, pro);
                    result1.cnprc.MetadataTemplateSolana = mdSolana;
                    result1.cnprc.SolanaUpdateAuthority = pro.Solanapublickey;
                }

                if (project.EnableAptos)
                {
                    IBlockchainFunctions aptos = new AptosBlockchainFunctions();
                    var mdAptos = aptos.GetMetadataFromCip25Metadata(pro.Metadata, pro);
                    result1.cnprc.MetadataTemplateAptos = mdAptos;
                    result1.cnprc.AptosCollectionAddress = pro.Aptosaddress;
                }

                result1.cnprc.Uid = pro.Uid;
                result1.cnprc.EnabledCoins = pro.Enabledcoins;
                result1.cnprc.Created = pro.Created;

                //  ConsoleCommand.RegisterPolicyAtPoolpm(pro.Policyid, pro.Policyscript);

                await spf.SavePricelists(db, project.Pricelist, project.MaxNftSupply, pro.Id,
                    customer.DefaultpromotionId);
                await spf.SaveSaleConditions(db, project.SaleConditions, pro.Id);
                await spf.SaveDiscounts(db, project.Discounts, pro.Id);
                await spf.SaveNotifications(db, project.Notifications, pro.Id);
                await SaveAdditionalPayoutWallets(db, project, pro.Id, customer.Id);
               
                await dbContextTransaction.CommitAsync();
            });

            return result1;
        }
        private class CheckPercentClass
        {
            public Blockchain Blockchain { get; set; }
            public double ValuePercent { get; set; }
        }
        private async Task <ApiErrorResultClass> CheckAdditionalPayoutWallets(EasynftprojectsContext db, CreateProjectClassV2 project, NMKR.Shared.Model.Customer customer, ApiErrorResultClass result)
        {
            List<CheckPercentClass > checkPercentClasses = new();
            if (project.AdditionalPayoutWallets == null)
                return result;
            foreach (var projectAdditionalPayoutWallet in project.AdditionalPayoutWallets)
            {
                if (string.IsNullOrEmpty(projectAdditionalPayoutWallet.PayoutWallet))
                    continue;
                var wallet =await (from a in db.Customerwallets
                    where a.CustomerId == customer.Id && a.Walletaddress == projectAdditionalPayoutWallet.PayoutWallet &&
                          a.State == "active"
                    select a).FirstOrDefaultAsync();

                if (wallet == null)
                {
                    result.ErrorCode = 124;
                    result.ErrorMessage = "Walletaddress not registered on NMKR Studio or not activated.";
                    result.ResultState = ResultStates.Error;
                    return result;
                }
                if (wallet.Cointype !=GlobalFunctions.ConvertToCoin(projectAdditionalPayoutWallet.Blockchain).ToString())
                {
                    result.ErrorCode = 124;
                    result.ErrorMessage = "Blockchain is wrong. Wallet is for the "+GlobalFunctions.ConvertToBlockchain(wallet.Cointype.ToEnum<Coin>()) + " blockchain";
                    result.ResultState = ResultStates.Error;
                    return result;
                }

                if (projectAdditionalPayoutWallet is {ValuePercent: not null, ValueFixInLovelace: not null})
                {
                    result.ErrorCode = 125;
                    result.ErrorMessage = "Specify either a value in percent or a fixed value for an additional wallet address. But not both";
                    result.ResultState = ResultStates.Error;
                    return result;
                }


                if (projectAdditionalPayoutWallet.ValuePercent is > 80)
                {
                    result.ErrorCode = 125;
                    result.ErrorMessage = "More than 80% for an additional wallet address is not possible";
                    result.ResultState = ResultStates.Error;
                    return result;
                }

                if (projectAdditionalPayoutWallet is {ValueFixInLovelace: < 1000000, Blockchain: Blockchain.Cardano})
                {
                    result.ErrorCode = 126;
                    result.ErrorMessage = "The minimum fixed value for an additional wallet address is 1 ADA (1000000 lovelace)";
                    result.ResultState = ResultStates.Error;
                    return result;
                }
                if (projectAdditionalPayoutWallet.ValuePercent != null && string.IsNullOrEmpty(projectAdditionalPayoutWallet.CustompropertyCondition))
                {
                    if (checkPercentClasses.Find(x => x.Blockchain == projectAdditionalPayoutWallet.Blockchain) != null)
                    {
                        checkPercentClasses.First(x => x.Blockchain == projectAdditionalPayoutWallet.Blockchain).ValuePercent += (double)projectAdditionalPayoutWallet.ValuePercent;
                    }
                    else
                    {
                        checkPercentClasses.Add(new CheckPercentClass(){Blockchain = projectAdditionalPayoutWallet.Blockchain, ValuePercent = (double)projectAdditionalPayoutWallet.ValuePercent });
                    }
                }
            }

            foreach (var checkPercentClass in checkPercentClasses.Where(checkPercentClass => checkPercentClass.ValuePercent > 80))
            {
                result.ErrorCode = 127;
                result.ErrorMessage = $"More than 80% in the {checkPercentClass.Blockchain.ToString()} blockchain for all additional wallet addresses is not possible";
                result.ResultState = ResultStates.Error;
                return result;
            }
           

            return result;
        }


        private async Task SaveAdditionalPayoutWallets(EasynftprojectsContext db, CreateProjectClassV2 project, int proId, int customerid)
        {
            if (project.AdditionalPayoutWallets == null)
                return;
            foreach (var projectAdditionalPayoutWallet in project.AdditionalPayoutWallets)
            {
                if (string.IsNullOrEmpty(projectAdditionalPayoutWallet.PayoutWallet))
                    continue;

                Nftprojectsadditionalpayout ap = new()
                {
                    NftprojectId = proId, 
                    WalletId = GetWalletId(db, projectAdditionalPayoutWallet.PayoutWallet, customerid),
                    Valuepercent = projectAdditionalPayoutWallet.ValuePercent,
                    Valuetotal = projectAdditionalPayoutWallet.ValueFixInLovelace,
                    Coin = GlobalFunctions.ConvertToCoin(projectAdditionalPayoutWallet.Blockchain).ToString(),
                    Custompropertycondition = projectAdditionalPayoutWallet.CustompropertyCondition
                };
                await db.Nftprojectsadditionalpayouts.AddAsync(ap);
                await db.SaveChangesAsync();
            }
        }

        private int GetWalletId(EasynftprojectsContext db, string payoutWallet, int customerid)
        {
            var wallet = (from a in db.Customerwallets
                where a.Walletaddress == payoutWallet && a.State == "active" && a.CustomerId==customerid
                select a).AsNoTracking().FirstOrDefault();
            if (wallet == null)
                return 0;

            return wallet.Id;
        }
        
        private async Task<string> LoadDefaultMetadata(EasynftprojectsContext db, CreateProjectClassV2 project )
        {
            string description = $"721_{project.StorageProvider.ToUpper()}";
            var metatemplate = await (from a in db.Defaulttemplates
                where a.Description == description
                select a).FirstOrDefaultAsync();

            return metatemplate.Template;
        }
    }
}
