using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes.Iagon;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Iagon;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using NMKR.Shared.Functions.Metadata;

namespace NMKR.Api.Controllers
{
    [Route("[controller]")]
    [ApiVersion("1")]
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
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="nft">The UploadNft Class as Body Content</param>
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
        [HttpPost("{apikey}/{nftprojectid:int}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Post(string apikey, int nftprojectid, [FromBody] UploadNftClass nft)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            if (Request.Method.Equals("HEAD"))
                return null;



            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            var result = CheckApiAccess.CheckApiKey(db, apikey, remoteIpAddress?.ToString(),
                nftprojectid);
            if (result.ResultState != ResultStates.Ok)
            {
                await db.Database.CloseConnectionAsync();
                return Unauthorized(result);
            }

            await GlobalFunctions.UpdateLastActionProjectAsync(db, nftprojectid,_redis);

            if (nft == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 99;
                result.ErrorMessage = "Invalid JSON Data submitted";
                LogClass.LogMessage(db,"API-CALL: Invalid JSON Data submitted " + nftprojectid);
                await db.Database.CloseConnectionAsync();
                return StatusCode(500, result);
            }
            LogClass.LogMessage(db,"API-CALL: NFT Upload started " + nftprojectid + " - " + nft.AssetName);

            if (nft.PreviewImageNft == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 66;
                result.ErrorMessage = "No Previewimage provided";
                LogClass.LogMessage(db,"API-CALL: No Previewimage provided " + nftprojectid + " - " + nft.AssetName);
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            result = CheckFiles(db, nft.PreviewImageNft, "PreviewImage", nft.Metadata);
            if (result.ResultState != ResultStates.Ok)
            {
                LogClass.LogMessage(db,"API-CALL: Upload Error 406 " + nftprojectid + " - " + nft.AssetName);
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            int i = 0;
            if (nft.Subfiles != null)
            {
                foreach (var file in nft.Subfiles)
                {
                    i++;

                    /*    if (string.IsNullOrEmpty(file.Name))
                                file.Name = nft.AssetName; */

                    result = CheckFiles(db, nft.PreviewImageNft, "Subfile " + i, nft.Metadata);
                    if (result.ResultState != ResultStates.Ok)
                    {
                        LogClass.LogMessage(db,"API-CALL: Upload Error 409 " + nftprojectid + " - " + nft.AssetName);
                        await db.Database.CloseConnectionAsync();
                        return StatusCode(409, result);
                    }


                    if (file.MetadataPlaceholder != null)
                    {
                        foreach (var placeholderClass in file.MetadataPlaceholder)
                        {
                            if (placeholderClass.Value==null)
                                continue;
                            if (placeholderClass.Value.Length <= 63) continue;
                            if (placeholderClass.Value.StartsWith("[") && placeholderClass.Value.EndsWith("]"))
                                continue;
                            result.ErrorCode = 116;
                            result.ErrorMessage = "Metadata placeholder value is too long (max. 63 characters) - " +
                                                  placeholderClass.Value;
                            result.ResultState = ResultStates.Error;
                            LogClass.LogMessage(db,"Metadata placeholder value is too long (max. 63 characters) " +
                                                   nftprojectid + " - " + nft.AssetName);
                            await db.Database.CloseConnectionAsync();
                            return StatusCode(406, result);
                        }
                    }
                }
            }




            if (string.IsNullOrEmpty(nft.AssetName))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 66;
                result.ErrorMessage = "No Assetname provided";
                LogClass.LogMessage(db,"API-CALL: No Assetname provided " + nftprojectid + " - " + nft.AssetName);
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }





            var t = await (from a in db.Nfts
                where a.NftprojectId == nftprojectid
                      && a.Name == nft.AssetName && a.State != "deleted"
                select a).FirstOrDefaultAsync();

            if (t != null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 66;
                result.ErrorMessage = "Assetname already exists";
                LogClass.LogMessage(db,"API-CALL: Assetname already exists " + nftprojectid + " - " + nft.AssetName);
                await db.Database.CloseConnectionAsync();
                return StatusCode(409, result);
            }

            if (!string.IsNullOrEmpty(nft.AssetName) && nft.AssetName.Length > 31)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 71;
                result.ErrorMessage = "Assetname is too long";
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if (!string.IsNullOrEmpty(nft.AssetName) && nft.AssetName.Length < 1)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 72;
                result.ErrorMessage = "Assetname is too short";
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if (!string.IsNullOrEmpty(nft.AssetName) && nft.AssetName!=GlobalFunctions.FilterTokenname(nft.AssetName))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 73;
                result.ErrorMessage = "Assetname contains invalid characters";
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }



            var project = await (from a in db.Nftprojects
                where a.Id == nftprojectid
                select a).FirstOrDefaultAsync();

            if (project == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 99;
                LogClass.LogMessage(db,"API-CALL: Upload - internal error " + nftprojectid + " - " + nft.AssetName);
                result.ErrorMessage = "Internal Error"; // Should never happen
                await db.Database.CloseConnectionAsync();
                return StatusCode(500, result);
            }
            var res = new UploadNftResultClass();

            if (!string.IsNullOrEmpty(nft.Metadata) && !project.Cip68)
            {
                var chk1 = new CheckMetadataForCip25Fields();
                var checkmetadata = chk1.CheckMetadata(nft.Metadata, project.Policyid, "", true, false);
                if (!checkmetadata.IsValid)
                {
                    result.ErrorCode = 115;
                    result.ErrorMessage = checkmetadata.ErrorMessage;
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }
            }

            if (!string.IsNullOrEmpty(nft.MetadataCip68) && project.Cip68)
            {
                if (!GlobalFunctions.IsValidJson(nft.MetadataCip68, out var formatedmetadata))
                {
                    result.ErrorCode = 115;
                    result.ErrorMessage = "Metadata CIP68 is not a valid JSON object";
                    result.ResultState = ResultStates.Error;
                    LogClass.LogMessage(db, "API-CALL: Metadata CIP68 is not valid JSON " + nftprojectid);
                    return StatusCode(406, result);
                }
                await db.SaveChangesAsync();
            }


            if (nft.PreviewImageNft != null && nft.PreviewImageNft.MetadataPlaceholder != null)
            {
                foreach (var placeholderClass in nft.PreviewImageNft.MetadataPlaceholder)
                {
                    if (placeholderClass.Value==null)
                        continue;
                    if (placeholderClass.Value.Length <= 63) continue;
                    if (placeholderClass.Value.StartsWith("[") && placeholderClass.Value.EndsWith("]")) continue;
                    result.ErrorCode = 116;
                    result.ErrorMessage = "Metadata placeholder value is too long (max. 63 characters) - " +
                                          placeholderClass.Value;
                    result.ResultState = ResultStates.Error;
                    LogClass.LogMessage(db,"Metadata placeholder value is too long (max. 63 characters) " +
                                           nftprojectid + " - " + nft.AssetName);
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }
            }



            int mid = 0;
            var strategy = db.Database.CreateExecutionStrategy();
            var error = false;
            await strategy.ExecuteAsync(async () =>
            {
                await using var dbContextTransaction = await db.Database.BeginTransactionAsync();
                string tokenname2 = GlobalFunctions.FilterTokenname(nft.AssetName);
                result = UploadFiles(db, nft.PreviewImageNft, project, tokenname2, "PreviewImage", null,
                    nft.Metadata,
                    out int mainftid, out string ipfshash, out string uid);
                if (result.ResultState != ResultStates.Ok)
                {
                    await dbContextTransaction.RollbackAsync();
                    error = true;
                    //   return StatusCode(406, result);
                }

                if (!error)
                {
                    res.NftId = mainftid;
                    res.NftUid = uid;
                    res.IpfsHashMainnft = ipfshash;
                    res.AssetId =
                        GlobalFunctions.GetAssetId(project.Policyid, project.Tokennameprefix, tokenname2);
                    List<string> subhashes = new();
                    i = 0;
                    if (nft.Subfiles != null)
                    {
                        foreach (var subfile in nft.Subfiles)
                        {
                            i++;
                            result = UploadFiles(db, subfile, project, tokenname2, "Subfile " + i, mainftid,
                                null,
                                out int subid, out string ipfshash2, out string uid2);
                            if (result.ResultState != ResultStates.Ok)
                            {
                                await dbContextTransaction.RollbackAsync();
                                LogClass.LogMessage(db,
                                    "API-CALL: Upload Error 406-2 " + nftprojectid + " - " + nft.AssetName);
                                error = true;
                                break;
                                //  return StatusCode(406, result);
                            }

                            if (!error)
                                subhashes.Add(ipfshash2);
                        }
                    }

                    if (!error)
                    {
                        res.IpfsHashSubfiles = subhashes.ToArray();
                        await dbContextTransaction.CommitAsync();
                        mid = mainftid;
                    }
                }
            });

            if (error)
            {
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            GetMetadataClass gm = new(res.NftId, "", true);
            res.Metadata = (await gm.MetadataResultAsync()).Metadata;
            LogClass.LogMessage(db,"API-CALL: NFT Upload finished " + nftprojectid + " - " + nft.AssetName + " - IPFS: "+(GeneralConfigurationClass.IPFSGateway + res.IpfsHashMainnft));
            await db.Database.CloseConnectionAsync();

            return Ok(res);
        }

        private ApiErrorResultClass UploadFiles(EasynftprojectsContext db, NftFile nft, Nftproject project, string tokenname, string name, int? mainnftid, string metadata, out int nftid, out string ipfshash, out string uid)
        {
            ApiErrorResultClass result = new() { ErrorCode = 0, ErrorMessage = "", ResultState = ResultStates.Ok };

            var path = GeneralConfigurationClass.TempFilePath;
            string filename = GlobalFunctions.GetGuid();

            nftid = 0;
            ipfshash = "";
            uid = null;

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
                    System.IO.File.WriteAllBytes(path + filename, Convert.FromBase64String(nft.FileFromBase64));
                }
                catch
                {
                    result.ErrorMessage = "Image is not a BASE64 Image";
                    result.ResultState = ResultStates.Error;
                    return result;
                }

                var ipfs = IpfsFunctions.AddFile(path + filename);
                Ipfsadd ia = Ipfsadd.FromJson(ipfs);


                IagonUploadResultClass iagon = null;
                if (project.Storage == "iagon")
                {
                    iagon = IagonFunctions.AddFile(path + filename, project.Uid, tokenname);
                }

                System.IO.File.Delete(path+filename);

                var n = SaveNft(db, path + filename, tokenname, ia.Hash, project, nft, mainnftid, metadata, iagon);
                ipfshash = ia.Hash;
                nftid = n.Id;
                uid = n.Uid;
                SavePlaceholder(db, n.Id, nft);
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
                    if (nft.FileFromsUrl.ToLower().StartsWith("http://") ||
                        nft.FileFromsUrl.ToLower().StartsWith("https://"))
                        client.DownloadFile(nft.FileFromsUrl, path + filename);
                    else
                    {
                        result.ErrorCode = 406;
                        result.ErrorMessage = "URL not correct";
                        result.ResultState = ResultStates.Error;
                        return result;
                    }
                }
                catch (Exception e)
                {
                    LogClass.LogMessage(db,"API-CALL: EXCEPTION NFT Upload - Downloading from url: " + nft.FileFromsUrl + " - " + e.Message);
                    result.ErrorCode = 404;
                    result.ErrorMessage = "Error while downloading from URL - " + e.Message;
                    result.ResultState = ResultStates.Error;
                    return result;
                }

                var ipfs = IpfsFunctions.AddFile(path + filename);
                Ipfsadd ia = Ipfsadd.FromJson(ipfs);

                IagonUploadResultClass iagon = null;
                if (project.Storage == "iagon")
                {
                    iagon = IagonFunctions.AddFile(path + filename, project.Uid, tokenname);
                }

                System.IO.File.Delete(path + filename);

                var n = SaveNft(db, path + filename, tokenname, ia.Hash, project, nft, mainnftid, metadata, iagon);
                ipfshash = ia.Hash;
                nftid = n.Id;
                uid = n.Uid;
                SavePlaceholder(db, n.Id, nft);
            }

            if (!string.IsNullOrEmpty(nft.FileFromIPFS))
            {
                bool checkIpfs = nft.FileFromIPFS.All(Char.IsLetterOrDigit);
                if (!checkIpfs)
                {
                    result.ErrorCode = 406;
                    result.ErrorMessage =
                        "IPFS Hash is not valid. Only the Hash - no ipfs:// or the gateway address";
                    result.ResultState = ResultStates.Error;
                    return result;
                }

                if (nft.FileFromIPFS.Length != 46)
                {
                    result.ErrorCode = 406;
                    result.ErrorMessage =
                        "IPFS Hash is not valid. only 46 characters - ipfs hash v0 supported only";
                    result.ResultState = ResultStates.Error;
                    return result;
                }

                if (!nft.FileFromIPFS.StartsWith("Qm"))
                {
                    result.ErrorCode = 406;
                    result.ErrorMessage =
                        "IPFS Hash is not valid. No v0 CID";
                    result.ResultState = ResultStates.Error;
                    return result;
                }

                if (project.Storage == "iagon")
                {
                    result.ErrorCode = 406;
                    result.ErrorMessage =
                        "IPFS Hash is not valid. Iagon does not support IPFS Hashes";
                    result.ResultState = ResultStates.Error;
                    return result;
                }

                // NO AWAIT !!! Just fire and forget
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                IpfsFunctions.PinFileAsync(nft.FileFromIPFS);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                var n = SaveNft(db,  filename, tokenname, nft.FileFromIPFS, project, nft, mainnftid, metadata,null);
                ipfshash = nft.FileFromIPFS;
                nftid = n.Id;
                uid = n.Uid;
                SavePlaceholder(db, n.Id, nft);
            }

            return result;
        }

        private Nft SaveNft(EasynftprojectsContext db, string filename, string tokenname, string hash, Nftproject project, NftFile nft, int? mainnftid, string metadataoverride, IagonUploadResultClass iagon)
        {
            Nft n = new()
            {
                Filename = filename,
                Name = tokenname,
                Ipfshash = hash,
                NftprojectId = project.Id,
                State = "free",
                Minted = false,
                Checkpolicyid = false,
                Mimetype = nft.Mimetype,
                Detaildata = nft.Description,
                Displayname = nft.Displayname,
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
                Multiplier = project.Multiplier,
                Iagonid = iagon?.Data?.Id,
                Iagonuploadresult = iagon != null ? JsonConvert.SerializeObject(iagon) : null,
            };
             db.Nfts.Add(n);
             db.SaveChanges();
            return n;
        }

        private ApiErrorResultClass CheckFiles(EasynftprojectsContext db, NftFile nft, string name, string metadata)
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


            if ((!string.IsNullOrEmpty(nft.FileFromBase64) || !string.IsNullOrEmpty(nft.FileFromsUrl)) && !FindMimetype(db, nft.Mimetype, name == "PreviewImage"))
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


            if (!string.IsNullOrEmpty(nft.Description) && nft.Description.Length > 63)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 70;
                result.ErrorMessage = "Description is too long (" + name + ")";
                return result;
            }
            if (!string.IsNullOrEmpty(nft.Displayname) && nft.Displayname.Length > 63)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 70;
                result.ErrorMessage = "Displayname is too long (" + name + ")";
                return result;
            }


            if (nft.MetadataPlaceholder != null)
            {
                if (nft.MetadataPlaceholder.Any() && !string.IsNullOrEmpty(metadata))
                {
                    result.ResultState = ResultStates.Error;
                    result.ErrorCode = 74;
                    result.ErrorMessage = "Please submit either metadata or placeholder values. But not both (" + name +
                                          ")";
                    return result;
                }
            }


            return result;
        }

        private bool FindMimetype(EasynftprojectsContext db, string nftMimetype, bool main)
        {
            var t =  (from a in db.Mimetypes
                     where a.Mimetype1 == nftMimetype && (!main || a.Allowedasmain)
                     select a).FirstOrDefault();
            return t != null;
        }

        private void SavePlaceholder(EasynftprojectsContext db, int nId, NftFile nft)
        {
            if (nft.MetadataPlaceholder == null)
                return;
            foreach (var ph in nft.MetadataPlaceholder)
            {
                if (!string.IsNullOrEmpty(ph.Name))
                    db.Metadata.Add(new() {NftId = nId, Placeholdername = ph.Name, Placeholdervalue = ph.Value});
            }

            db.SaveChanges();
        }
    }
}
