using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Iagon;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Iagon;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Nft
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class UploadNftController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public UploadNftController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Upload a File to a project and pin it to IPFS
        /// </summary>
        /// <remarks>
        /// With this API you can upload a file to IPFS and add it to a project. You can upload the file as BASE64 Content or as URL Link or as IPFS Hash.
        /// If you submit Metadata, this Metadata will be used instead of the Metadatatemplate from the project. You can either submit Metadata or MetadataPlaceholder, but not both (because it makes no sense).
        /// The Metadata field is optional and if you dont use it, it will use the Template from your project. It is poosible to mix both versions in one project. You can have one nft with own metadata and other nfts
        /// with the template.
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project</param>
        /// <param Name="nft">The UploadNft Class as Body Content</param>
        /// <param Name="uploadsource">Specifiy an optional uploadsource parameter (for information only) eg. uploaded via website x etc.</param>
        /// <response code="200">Returns the UploadNftResult Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">No Image Content was provided. Send a file either as Base64 or as Link or IPFS Hash</response>            
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="409">There is a conflict with the provided images. Send a file either as Base64 or as Link or IPFS Hash</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UploadNftResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{projectuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "NFT" }
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid, [FromBody] UploadNftClassV2 nft, [FromQuery] string? uploadsource=null)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = projectuid;

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



            var project1 = await (from a in db.Nftprojects
                    .Include(a=>a.Settings)
                where a.Uid == projectuid
                select a).FirstOrDefaultAsync();

            if (project1 == null)
            {
                LogClass.LogMessage(db, "API-CALL from " + remoteIpAddress + ": ERROR: Project not found " + projectuid);
                result.ErrorCode = 570;
                result.ErrorMessage = $"Project UID ({projectuid}) not found. Please contact support";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            int nftprojectid = project1.Id;

            await GlobalFunctions.UpdateLastActionProjectAsync(db, nftprojectid, _redis);

            if (nft == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 99;
                result.ErrorMessage = "Invalid JSON Data submitted";
                LogClass.LogMessage(db,"API-CALL: Invalid JSON Data submitted " + nftprojectid);
                return StatusCode(500, result);
            }
            LogClass.LogMessage(db,"API-CALL: NFT Upload started " + nftprojectid + " - " + nft.Tokenname);

            if (nft.PreviewImageNft == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 66;
                result.ErrorMessage = "No Previewimage provided";
                LogClass.LogMessage(db,"API-CALL: No Previewimage provided " + nftprojectid + " - " + nft.Tokenname);
                return StatusCode(406, result);
            }

            result = await CheckFiles(db, nft.PreviewImageNft, "PreviewImage", nft.Description);
            if (result.ResultState != ResultStates.Ok)
            {
                LogClass.LogMessage(db,"API-CALL: Upload Error 406 " + nftprojectid + " - " + nft.Tokenname);

                return StatusCode(406, result);
            }

            int i = 0;
            if (nft.Subfiles != null)
            {
                foreach (var file in nft.Subfiles)
                {
                    i++;

                    result = await CheckFiles(db, nft.PreviewImageNft, "Subfile " + i,  nft.Description);
                    if (result.ResultState != ResultStates.Ok)
                    {
                        LogClass.LogMessage(db,"API-CALL: Upload Error 409 " + nftprojectid + " - " + nft.Tokenname);
                        return StatusCode(409, result);
                    }


                    if (file.MetadataPlaceholder != null)
                    {
                        foreach (var placeholderClass in file.MetadataPlaceholder)
                        {
                            if (placeholderClass.Value==null)
                                continue;
                            if (placeholderClass.Value.Length <= 255) continue;
                            if (placeholderClass.Value.StartsWith("[") && placeholderClass.Value.EndsWith("]"))
                                continue;
                            result.ErrorCode = 116;
                            result.ErrorMessage = "Metadata placeholder value is too long (max. 255 characters) - " +
                                                  placeholderClass.Value;
                            result.ResultState = ResultStates.Error;
                            LogClass.LogMessage(db,"Metadata placeholder value is too long (max. 255 characters) " +
                                                   nftprojectid + " - " + nft.Tokenname);
                            return StatusCode(406, result);
                        }
                    }
                }
            }


            if (string.IsNullOrEmpty(nft.Tokenname))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 66;
                result.ErrorMessage = "No Tokenname provided";
                LogClass.LogMessage(db,"API-CALL: No Tokenname provided " + nftprojectid + " - " + nft.Tokenname);
                return StatusCode(406, result);
            }




            var t = await (from a in db.Nfts
                where a.NftprojectId == nftprojectid
                      && a.Name == nft.Tokenname && a.State != "deleted"
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (t != null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 66;
                result.ErrorMessage = "Tokenname already exists";
                LogClass.LogMessage(db, "API-CALL: Tokenname already exists " + nftprojectid + " - " + nft.Tokenname);
                return StatusCode(409, result);
            }

            if (nft.Tokenname.Length > 32)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 71;
                result.ErrorMessage = "Tokenname is too long";
                return StatusCode(406, result);
            }


            if (!string.IsNullOrEmpty(nft.Displayname) && nft.Displayname.Length > 255)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 70;
                result.ErrorMessage = "Displayname is too long";
                return StatusCode(406, result);
            }

            long mincosts = GlobalFunctions.CalculateMinutxoNew(project1, 1);
            if (nft.PriceInLovelace!=null && nft.PriceInLovelace < mincosts)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 75;
                result.ErrorMessage = $"Price must be at least {mincosts} lovelace";
                return StatusCode(406, result);
            }




            var project = await (from a in db.Nftprojects
                where a.Id == nftprojectid
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (project == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 99;
                LogClass.LogMessage(db,"API-CALL: Upload - internal error " + nftprojectid + " - " + nft.Tokenname);
                result.ErrorMessage = "Internal Error"; // Should never happen
                return StatusCode(500, result);
            }
            var res = new UploadNftResultClass();


            if (nft.PriceInLovelace != null && project.Maxsupply>1)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 79;
                result.ErrorMessage = "The has a maxupply greater than 1 - you can not set the price here. Use the pricelist";
                return StatusCode(406, result);
            }


            if (!string.IsNullOrEmpty(nft.MetadataOverride) && !project.Cip68)
            {
                var chk1=new CheckMetadataForCip25Fields();
                var checkmetadata = chk1.CheckMetadata(nft.MetadataOverride, project.Policyid, "", true, false);
                if (!checkmetadata.IsValid)
                {
                    result.ErrorCode = 115;
                    result.ErrorMessage = checkmetadata.ErrorMessage;
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }
            }

            if (!string.IsNullOrEmpty(nft.MetadataOverrideCip68) && project.Cip68)
            {
                if (!GlobalFunctions.IsValidJson(nft.MetadataOverride, out var formatedmetadata))
                {
                    result.ErrorCode = 115;
                    result.ErrorMessage = "Metadata CIP68 is not a valid JSON object";
                    result.ResultState = ResultStates.Error;
                    LogClass.LogMessage(db, "API-CALL: Metadata CIP68 is not valid JSON " + nftprojectid + " - " + nft.Tokenname);
                    return StatusCode(406, result);
                }
                nft.MetadataOverrideCip68 = formatedmetadata;
                await db.SaveChangesAsync();
            }

            var strategy = db.Database.CreateExecutionStrategy();
            var error = false;
            await strategy.ExecuteAsync(async () =>
            {
                await using var dbContextTransaction = await db.Database.BeginTransactionAsync();
                string tokenname2 = GlobalFunctions.FilterTokenname(nft.Tokenname);
                var result1 = await UploadFiles(db, nft.PreviewImageNft, project, tokenname2, nft.Displayname,
                    nft.Description, null, nft.MetadataOverride, nft.MetadataPlaceholder, nft.PriceInLovelace, nft.IsBlocked, uploadsource);
                result = result1.result;
                if (result.ResultState != ResultStates.Ok)
                {
                    await dbContextTransaction.RollbackAsync();
                    LogClass.LogMessage(db, "API-CALL: NFT Upload Rollback");
                    error = true;
                    //  return StatusCode(406, result);
                }

                if (!error)
                {
                    res.NftId = result1.nftid;
                    res.IpfsHashMainnft = result1.ipfshash;
                    res.NftUid = result1.uid;
                    res.AssetId =
                        GlobalFunctions.GetAssetId(project.Policyid, project.Tokennameprefix, tokenname2);
                    List<string> subhashes = new();
                    i = 0;
                    if (nft.Subfiles != null)
                    {
                        foreach (var subfile in nft.Subfiles)
                        {
                            i++;
                            result1 = await UploadFiles(db, subfile.Subfile, project, tokenname2,
                                nft.Displayname,
                                subfile.Description, res.NftId, null, subfile.MetadataPlaceholder, null, false, uploadsource);
                            result = result1.result;
                            if (result.ResultState != ResultStates.Ok)
                            {
                                await dbContextTransaction.RollbackAsync();
                                LogClass.LogMessage(db,
                                    "API-CALL: Upload Error 406-2 " + nftprojectid + " - " + nft.Tokenname);
                                error = true;
                                break;
                                // return StatusCode(406, result);
                            }

                            if (!error)
                                subhashes.Add(result1.ipfshash);
                        }
                    }

                    if (!error)
                    {
                        res.IpfsHashSubfiles = subhashes.ToArray();
                        await dbContextTransaction.CommitAsync();
                        // mid = result1.nftid;
                    }
                }
            });

            if (result.ResultState != ResultStates.Ok)
            {
                return StatusCode(406, result);
            }
            GetMetadataClass gm = new(res.NftId, "", true);
            var metadata = (await gm.MetadataResultAsync()).Metadata;
            if (project1.Enabledcoins.Contains(Coin.ADA.ToString()))
            {
                res.Metadata = metadata;
            }

            if (project1.Enabledcoins.Contains(Coin.APT.ToString()))
            {
                IBlockchainFunctions aptos = new AptosBlockchainFunctions();
                res.MetadataAptos = aptos.GetMetadataFromCip25Metadata(metadata, project1);
            }
            if (project1.Enabledcoins.Contains(Coin.SOL.ToString()))
            {
                IBlockchainFunctions solana = new SolanaBlockchainFunctions(); 
                res.MetadataSolana = solana.GetMetadataFromCip25Metadata(metadata, project1);
            }


            LogClass.LogMessage(db,"API-CALL: NFT Upload finished " + nftprojectid + " - " + nft.Tokenname + " - IPFS: "+(GeneralConfigurationClass.IPFSGateway + res.IpfsHashMainnft));
            await db.Database.CloseConnectionAsync();

            return Ok(res);
        }

        private class UploadFilesResultClass
        {
            public ApiErrorResultClass result { get; init; }
            public int nftid { get; set; }
            public string ipfshash { get; set; }
            public string uid { get; set; }
        }

        private async Task<UploadFilesResultClass> UploadFiles(EasynftprojectsContext db, NftFileV2 nft, Nftproject project, string tokenname,string displayname, string description, int? mainnftid, string metadata, MetadataPlaceholderClass[] placeholder, long? price, bool? isblocked, string uploadsource)
        {
            
            UploadFilesResultClass result1 = new()
                {result = new() { ErrorCode = 0, ErrorMessage = "", ResultState = ResultStates.Ok }, nftid = 0, ipfshash = "", uid = null};

            var path = GeneralConfigurationClass.TempFilePath;
            string filename = GlobalFunctions.GetGuid();

            if (nft == null)
            {
                result1.result.ResultState = ResultStates.Error;
                result1.result.ErrorMessage = "Missing (Sub)File Information";
                result1.result.ErrorCode = 119;
                return result1;
            }
            if (nft.Mimetype == "image/jpg")
                nft.Mimetype = "image/jpeg";

            if (!string.IsNullOrEmpty(nft.FileFromBase64) || !string.IsNullOrEmpty(nft.FileFromsUrl))
            {
                switch (nft.Mimetype)
                {
                    case "image/png":
                        filename += ".png";
                        break;
                    case "image/jpeg":
                        filename += ".jpeg";
                        break;
                    case "image/gif":
                        filename += ".gif";
                        break;
                }
            }

            if (!string.IsNullOrEmpty(nft.FileFromBase64))
            {
                nft.FileFromBase64 = nft.FileFromBase64.Replace("data:image/jpeg;base64,", "");
                try
                {
                    await System.IO.File.WriteAllBytesAsync(path + filename, Convert.FromBase64String(nft.FileFromBase64));
                }
                catch
                {
                    result1.result.ErrorMessage = "Image is not a BASE64 Image";
                    result1.result.ResultState = ResultStates.Error;
                    return result1;
                }

                var ipfs = await IpfsFunctions.AddFileAsync(path + filename);
                Ipfsadd ia = Ipfsadd.FromJson(ipfs);

                IagonUploadResultClass iagon = null;
                if (project.Storage == "iagon")
                {
                    iagon = IagonFunctions.AddFile(path + filename, project.Uid, tokenname);
                }
                System.IO.File.Delete(path+filename);
              

                var n = await SaveNft(db, path + filename, tokenname,displayname,description, ia.Hash, project, nft, mainnftid, metadata, price, isblocked, uploadsource, iagon);
                result1.ipfshash = ia.Hash;
                result1.nftid = n.Id;
                result1.uid = n.Uid;
                await SavePlaceholder(db, n.Id, placeholder);
            }

            if (!string.IsNullOrEmpty(nft.FileFromsUrl))
            {
                try
                {
                    LogClass.LogMessage(db,"API-CALL: NFT Upload - Downloading from url: "+nft.FileFromsUrl);

                    System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                        delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                            System.Security.Cryptography.X509Certificates.X509Chain chain,
                            System.Net.Security.SslPolicyErrors sslPolicyErrors)
                        {
                            return true; // **** Always accept
                        };

                    using var client = new WebDownload();
                    // ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                    if (nft.FileFromsUrl.ToLower().StartsWith("http://") ||
                        nft.FileFromsUrl.ToLower().StartsWith("https://"))
                        await client.DownloadFileTaskAsync(nft.FileFromsUrl, path + filename);
                    else
                    {
                        result1.result.ErrorCode = 406;
                        result1.result.ErrorMessage = "Url not correct";
                        result1.result.ResultState = ResultStates.Error;
                        return result1;
                    }
                }
                catch (Exception e)
                {
                    LogClass.LogMessage(db,"API-CALL: EXCEPTION NFT Upload - Downloading from url: " + nft.FileFromsUrl + " - " + e.Message);
                    result1.result.ErrorCode = 404;
                    result1.result.ErrorMessage = "Error while downloading from URL - " + e.Message;
                    result1.result.ResultState = ResultStates.Error;
                    return result1;
                }

                var ipfs = await IpfsFunctions.AddFileAsync(path + filename);
                Ipfsadd ia = Ipfsadd.FromJson(ipfs);
                IagonUploadResultClass iagon = null;
                if (project.Storage == "iagon")
                {
                    iagon = IagonFunctions.AddFile(path + filename, project.Uid, tokenname);
                }

                System.IO.File.Delete(path + filename);
              

                var n = await SaveNft(db, path + filename, tokenname,displayname, description,ia.Hash, project, nft, mainnftid, metadata, price, isblocked, uploadsource, iagon);
                result1.ipfshash = ia.Hash;
                result1.nftid = n.Id;
                result1.uid = n.Uid;
                await SavePlaceholder(db, n.Id, placeholder);
            }

            if (!string.IsNullOrEmpty(nft.FileFromIPFS))
            {
                bool checkIpfs = nft.FileFromIPFS.All(Char.IsLetterOrDigit);
                if (!checkIpfs)
                {
                    result1.result.ErrorCode = 406;
                    result1.result.ErrorMessage =
                        "IPFS Hash is not valid. Only the Hash - no ipfs:// or the gateway address";
                    result1.result.ResultState = ResultStates.Error;
                    return result1;
                }
                if (nft.FileFromIPFS.Length != 46)
                {
                    result1.result.ErrorCode = 406;
                    result1.result.ErrorMessage =
                        "IPFS Hash is not valid. only 46 characters - ipfs hash v0 supported only";
                    result1.result.ResultState = ResultStates.Error;
                    return result1;
                }
                
                if (!nft.FileFromIPFS.StartsWith("Qm"))
                {
                    result1.result.ErrorCode = 406;
                    result1.result.ErrorMessage =
                        "IPFS Hash is not valid. No v0 CID";
                    result1.result.ResultState = ResultStates.Error;
                    return result1;
                }

                if (project.Storage == "iagon")
                {
                    result1.result.ErrorCode = 406;
                    result1.result.ErrorMessage =
                        "IPFS Hash is not valid. Iagon does not support IPFS Hashes";
                    result1.result.ResultState = ResultStates.Error;
                    return result1;
                }

                // NO AWAIT !!! Just fire and forget
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                IpfsFunctions.PinFileAsync(nft.FileFromIPFS);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                var n = await SaveNft(db,  filename, tokenname,displayname, description,nft.FileFromIPFS, project, nft, mainnftid, metadata, price, isblocked, uploadsource, null);

                result1.ipfshash = nft.FileFromIPFS;
                result1.nftid = n.Id;
                result1.uid = n.Uid;
                await SavePlaceholder(db, n.Id, placeholder);
            }

            return result1;
        }

        private async Task<NMKR.Shared.Model.Nft> SaveNft(EasynftprojectsContext db, string filename, string tokenname, string displayname, string description, string hash, Nftproject project, NftFileV2 nft, int? mainnftid, string metadataoverride, long? price, bool? isblocked, string uploadsource, IagonUploadResultClass iagon)
        {
            NMKR.Shared.Model.Nft n = new()
            {
                Filename = filename,
                Name = tokenname,
                Ipfshash = hash,
                NftprojectId = project.Id,
                State = isblocked==true?"blocked": "free",
                Minted = false,
                Checkpolicyid = false,
                Mimetype = nft.Mimetype,
                Detaildata = description,
                Displayname = displayname,
                Soldcount = 0,
                MainnftId = mainnftid,
                Reservedcount = 0,
                Errorcount = 0,
                Metadataoverride = metadataoverride,
                Policyid = project.Policyid,
                Created = DateTime.Now,
                Filesize = GlobalFunctions.GetFileSize(filename),
                Assetid = GlobalFunctions.GetAssetId(project.Policyid, project.Tokennameprefix, tokenname),
                Uid = Guid.NewGuid().ToString(),
                Price = price,
                Multiplier = project.Multiplier,
                Uploadsource = uploadsource??"API",
                Iagonid = iagon?.Data?.Id,
                Iagonuploadresult = iagon!=null?JsonConvert.SerializeObject(iagon):null,
            };
            await db.Nfts.AddAsync(n);
            await db.SaveChangesAsync();

            await UpdateProjectSettingsIdFromUploadsource(db, project, uploadsource);

            return n;
        }

        private static async Task UpdateProjectSettingsIdFromUploadsource(EasynftprojectsContext db, Nftproject project,
            string uploadsource)
        {
            if (string.IsNullOrEmpty(uploadsource)) return;
            var settings = await (from a in db.Settings
                where a.Uploadsourceforceprice == uploadsource
                select a).AsNoTracking().FirstOrDefaultAsync();
            if (settings == null) return;
            if (project.SettingsId == settings.Id)
                return;

            var p1 = await (from a in db.Nftprojects
                where a.Id == project.Id
                select a).FirstOrDefaultAsync();
            if (p1 == null) return;
            p1.SettingsId = settings.Id;
            await db.SaveChangesAsync();
        }

        private async Task<ApiErrorResultClass> CheckFiles(EasynftprojectsContext db, NftFileV2 nft, string name, string description)
        {
            ApiErrorResultClass result = new() { ErrorCode = 0, ErrorMessage = "", ResultState = ResultStates.Ok };

            if (string.IsNullOrEmpty(nft.FileFromBase64) && string.IsNullOrEmpty(nft.FileFromIPFS) &&
                  string.IsNullOrEmpty(nft.FileFromsUrl))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 62;
                result.ErrorMessage = "No File provided. Send a file either as Base64 or as Link or IPFS Hash (" + name + ")";
                return result;
            }

            if (!string.IsNullOrEmpty(nft.FileFromBase64) && (!string.IsNullOrEmpty(nft.FileFromIPFS) ||
                                                             !string.IsNullOrEmpty(nft.FileFromsUrl)))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 63;
                result.ErrorMessage =
                    "Multiple Files provided. Send a File either as Base64 or as Link or IPFS Hash (" + name + ")";
                return result;
            }

            if (!string.IsNullOrEmpty(nft.FileFromIPFS) && (!string.IsNullOrEmpty(nft.FileFromBase64) ||
                                                            !string.IsNullOrEmpty(nft.FileFromsUrl)))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 64;
                result.ErrorMessage =
                    "Multiple Files provided. Send a File either as Base64 or as Link or IPFS Hash (" + name + ")";
                return result;
            }

            if (!string.IsNullOrEmpty(nft.FileFromsUrl) && (!string.IsNullOrEmpty(nft.FileFromIPFS) ||
                                                            !string.IsNullOrEmpty(nft.FileFromBase64)))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 65;
                result.ErrorMessage =
                    "Multiple Files provided. Send a File either as Base64 or as Link or IPFS Hash (" + name + ")";
                return result;
            }


            if ((!string.IsNullOrEmpty(nft.FileFromBase64) || !string.IsNullOrEmpty(nft.FileFromsUrl)) && !(await FindMimetype(db, nft.Mimetype, name == "PreviewImage")))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 68;
                result.ErrorMessage = "Unsupported MIME Type on " + name;
                return result;
            }

            if (!string.IsNullOrEmpty(nft.FileFromsUrl) && !GlobalFunctions.IsUrlValid(nft.FileFromsUrl))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 69;
                result.ErrorMessage = "URL is not valid";
                return result;
            }


            if (!string.IsNullOrEmpty(description) && description.Length > 255)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 70;
                result.ErrorMessage = "Description is too long (" + name + ")";
                return result;
            }
         
            return result;
        }

        private async Task<bool> FindMimetype(EasynftprojectsContext db, string nftMimetype, bool main)
        {
            var t = await (from a in db.Mimetypes
                     where a.Mimetype1 == nftMimetype && (!main || a.Allowedasmain)
                     select a).FirstOrDefaultAsync();
            return t != null;
        }

        private async Task SavePlaceholder(EasynftprojectsContext db, int nId, MetadataPlaceholderClass[] ph)
        {
            if (ph == null)
                return;
            foreach (var ph1 in ph)
            {
                if (!string.IsNullOrEmpty(ph1.Name))
                    await db.Metadata.AddAsync(new() { NftId = nId, Placeholdername = ph1.Name, Placeholdervalue = ph1.Value });
            }

            await db.SaveChangesAsync();
        }
    }
}
