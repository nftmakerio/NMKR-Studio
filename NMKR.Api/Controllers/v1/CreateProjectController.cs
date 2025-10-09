using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Asp.Versioning;

namespace NMKR.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]

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
        [HttpPost("{apikey}")]
        [MapToApiVersion("1")]
        public IActionResult Post(string apikey, [FromBody] CreateProjectClass project)
        {
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
               "", apikey, remoteIpAddress?.ToString() ?? string.Empty);
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            if (string.IsNullOrEmpty(project.Projectname))
            {
                result.ErrorCode = 101;
                result.ErrorMessage = "Projectname must not be empty";
                result.ResultState = ResultStates.Error;
                db.Database.CloseConnection();
                return StatusCode(406, result);
            }

            int? userid = GetUserid(db, apikey);

           
            if (project.PolicyExpires && project.PolicyLocksDateTime == null)
            {
                result.ErrorCode = 101;
                result.ErrorMessage = "When the policy will expire, submit an expirationdate";
                result.ResultState = ResultStates.Error;
                db.Database.CloseConnection();
                return StatusCode(406, result);
            }

            if (project.PolicyExpires && project.PolicyLocksDateTime < DateTime.Now)
            {
                result.ErrorCode = 102;
                result.ErrorMessage = "Policy expiration must be in the future";
                result.ResultState = ResultStates.Error;
                db.Database.CloseConnection();
                return StatusCode(406, result);
            }

            if (project.TokennamePrefix.Length > 15)
            {
                result.ErrorCode = 103;
                result.ErrorMessage = "Tokennameprefix is too long (max. 15 chars)";
                result.ResultState = ResultStates.Error;
                db.Database.CloseConnection();
                return StatusCode(406, result);
            }

            if (!string.IsNullOrEmpty(project.TokennamePrefix) && !GlobalFunctions.isAlphaNumeric(project.TokennamePrefix))
            {
                result.ErrorCode = 103;
                result.ErrorMessage = "Tokennameprefix contains invalid characters";
                result.ResultState = ResultStates.Error;
                db.Database.CloseConnection();
                return StatusCode(406, result);
            }


            if (project.Policy != null)
            {
                if (!GlobalFunctions.IsValidJson(project.Policy.PolicyScript, out var formatedmetadata1))
                {
                    result.ErrorCode = 104;
                    result.ErrorMessage = "Policy Script is not a valid JSON Document";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    return StatusCode(406, result);
                }

                if (string.IsNullOrEmpty(project.Policy.PrivateSigningkey))
                {
                    result.ErrorCode = 105;
                    result.ErrorMessage = "Signing Key is not correct";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
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
                    db.Database.CloseConnection();
                    return StatusCode(406, result);
                }

                if (skey.Description != "Payment Signing Key")
                {
                    result.ErrorCode = 107;
                    result.ErrorMessage = "Signing Key is not correct";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    return StatusCode(406, result);
                }

                if (string.IsNullOrEmpty(skey.CborHex))
                {
                    result.ErrorCode = 108;
                    result.ErrorMessage = "Signing Key is not correct";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
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
                    db.Database.CloseConnection();
                    return StatusCode(406, result);
                }

                if (vkey.Description != "Payment Verification Key")
                {
                    result.ErrorCode = 110;
                    result.ErrorMessage = "Verification Key is not correct";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    return StatusCode(406, result);
                }

                if (string.IsNullOrEmpty(vkey.CborHex))
                {
                    result.ErrorCode = 111;
                    result.ErrorMessage = "Verification Key is not correct";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    return StatusCode(406, result);
                }


                var keyhash = ConsoleCommand.GetKeyhash(project.Policy.PrivateVerifykey);
                if (keyhash == "")
                {
                    result.ErrorCode = 112;
                    result.ErrorMessage = "Verification Key is not correct";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    return StatusCode(406, result);
                }

                if (!project.Policy.PolicyScript.Contains(keyhash))
                {
                    result.ErrorCode = 113;
                    result.ErrorMessage = "Policy Script does not match with Verification Key";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    return StatusCode(406, result);
                }

                var policyid = ConsoleCommand.GetPolicyId(project.Policy.PolicyScript);
                if (policyid != project.Policy.PolicyId)
                {
                    result.ErrorCode = 114;
                    result.ErrorMessage = "Policy ID does not match with Policy Script";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    return StatusCode(406, result);
                }

                var oldproj = (from a in db.Nftprojects
                    where a.Policyid == policyid
                    select a).FirstOrDefault();
                if (oldproj != null)
                    project.PolicyLocksDateTime = oldproj.Policyexpire;

            }

            if (string.IsNullOrEmpty(project.Metadata))
            {
                project.Metadata = LoadDefaultMetadata(db);
            }

            var chk1 = new CheckMetadataForCip25Fields();
            var checkmetadata = chk1.CheckMetadata(project.Metadata, project.Policy?.PolicyId, "", true, false);
            if (!checkmetadata.IsValid)
            {
                result.ErrorCode = 115;
                result.ErrorMessage = checkmetadata.ErrorMessage;
                result.ResultState = ResultStates.Error;
                db.Database.CloseConnection();
                return StatusCode(406, result);
            }


            int? payoutwallet = null;
               

            if (userid == null)
            {
                result.ErrorCode = 124;
                result.ErrorMessage = "Internal Error";
                result.ResultState = ResultStates.Error;
                db.Database.CloseConnection();
                return StatusCode(500,result);
            }

            if (!string.IsNullOrEmpty(project.PayoutWalletaddress))
            {
                var pow = (from a in db.Customerwallets
                    where a.Walletaddress == project.PayoutWalletaddress && a.CustomerId==userid
                    select a).FirstOrDefault();

                if (pow == null)
                {
                    result.ErrorCode = 116;
                    result.ErrorMessage = "Payout wallet is not registered in your account";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    return StatusCode(406, result);
                }

                if (pow.State != "active")
                {
                    result.ErrorCode = 117;
                    result.ErrorMessage = "Payout wallet is not activated";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    return StatusCode(406, result);
                }

                payoutwallet = pow.Id;
            }

            if (project.AddressExpiretime < 5 || project.AddressExpiretime > 60)
            {
                result.ErrorCode = 118;
                result.ErrorMessage = "Address expire time must be between 5 and 60 minutes";
                result.ResultState = ResultStates.Error;
                db.Database.CloseConnection();
                return StatusCode(406, result);
            }

            if (project.MaxNftSupply < 1 )
            {
                result.ErrorCode = 119;
                result.ErrorMessage = "MaxNftSupply must be between 1 and "+ long.MaxValue;
                result.ResultState = ResultStates.Error;
                db.Database.CloseConnection();
                return StatusCode(406, result);
            }


            var r1 = CreateNewProject(db, project, apikey,userid, payoutwallet, out CreateNewProjectResultClass cnprc);
            if (r1.ResultState != ResultStates.Ok)
                return StatusCode(500, r1);

            db.Database.CloseConnection();
            return Ok(cnprc);
        }

        private ApiErrorResultClass CreateNewProject(EasynftprojectsContext db, CreateProjectClass project, string apikey, int? userid, int? payoutwallet, out CreateNewProjectResultClass cnprc)
        {
            cnprc = new();

            ApiErrorResultClass result = new() {ErrorCode = 0, ErrorMessage = "", ResultState = ResultStates.Ok};
            var qt = ConsoleCommand.GetQueryTip();

            if (qt == null)
            {
                result.ErrorCode = 120;
                result.ErrorMessage = "Internal error while quering slot";
                result.ResultState = ResultStates.Error;
                return result;
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
                        var s1 = slot - qt.Slot;
                        project.PolicyLocksDateTime= DateTime.Now.AddSeconds((long)s1);
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
                    return result;
                }


                var keyhash = ConsoleCommand.GetKeyhash(cn.privatevkey);
                if (string.IsNullOrEmpty(keyhash))
                {
                    result.ErrorCode = 122;
                    result.ErrorMessage = "Internal Error while creating Policy Keyhash";
                    result.ResultState = ResultStates.Error;
                    return result;
                }

                
                PolicyScript ps = new() {Type = "all"};
                List<PolicyScriptScript> ls = new();
                ls.Add(new() {KeyHash = keyhash, Type = "sig"});
                if (project.PolicyExpires && project.PolicyLocksDateTime!=null)
                {
                    var diffInSeconds = (((DateTime)(project.PolicyLocksDateTime)).Date.AddMinutes(1) - DateTime.Now).TotalSeconds;
                    slot = (long)qt.Slot + (long)diffInSeconds;
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
                    return result;
                }

                address = cn.Address;
                vkey = cn.privatevkey;
                skey = cn.privateskey;
            }



            var solanaaddress = SolanaFunctions.CreateNewWallet();
            CryptographyProcessor cp = new();
            string password = cp.CreateSalt(30);
            if (userid != null)
            {
                var customer = (from a in db.Customers
                    where a.Id == userid
                    select a).FirstOrDefault();

                Nftproject pro = new()
                {
                    CustomerId = (int) userid,
                    Password = password,
                    Description = project.Description,
                    Minutxo = nameof(MinUtxoTypes.twoadaeverynft),
                    State = "active",
                    Projectname = project.Projectname,
                    Projecturl = project.Projecturl,
                    Policyexpire = project.PolicyExpires ? project.PolicyLocksDateTime : null,
                    Policyaddress = address,
                    Policyskey = Encryption.EncryptString(skey, password),
                    Policyvkey = Encryption.EncryptString(vkey, password),
                    Policyscript = policscript,
                    Policyid = policyid,
                    Maxsupply = project.MaxNftSupply,
                    Version = "1.0",
                    CustomerwalletId = payoutwallet,
                    Tokennameprefix = project.TokennamePrefix??="",
                    Metadata = JsonFormatter.FormatJson(project.Metadata),
                    Expiretime = project.AddressExpiretime,
                    Created = DateTime.Now,
                    SettingsId = customer.DefaultsettingsId, // Default Settings
                    Uid = Guid.NewGuid().ToString(),
                    Lockslot = slot,
                    Solanapublickey = solanaaddress.Address,
                    Solanaseedphrase = Encryption.EncryptString(solanaaddress.SeedPhrase, password)
                };

                db.Nftprojects.Add(pro);
                db.SaveChanges();
                cnprc.PolicyScript = pro.Policyscript;
                cnprc.PolicyExpiration = pro.Policyexpire;
                cnprc.Metadata = pro.Metadata;
                cnprc.ProjectId = pro.Id;
                cnprc.PolicyId = pro.Policyid;
                cnprc.Uid = pro.Uid;

           //     ConsoleCommand.RegisterPolicyAtPoolpm(pro.Policyid, pro.Policyscript);

            }
            else
            {
                result.ErrorCode = 124;
                result.ErrorMessage = "Internal Error";
                result.ResultState = ResultStates.Error;
                return result;
            }
            return result;
        }

        private int? GetUserid(EasynftprojectsContext db, string apikey)
        {
            string hash = HashClass.GetHash(SHA256.Create(), apikey);


            var t = (from a in db.Apikeys
                    .Include(a => a.Apikeyaccesses)
                where a.Apikeyhash == hash
                select a).AsNoTracking().FirstOrDefault();

            if (t != null)
                return t.CustomerId;

            return null;
        }


        private string LoadDefaultMetadata(EasynftprojectsContext _db)
        {
            var metatemplate = (from a in _db.Defaulttemplates
                where a.Id == 1
                select a).FirstOrDefault();
            return metatemplate.Template;
        }
    }
}
