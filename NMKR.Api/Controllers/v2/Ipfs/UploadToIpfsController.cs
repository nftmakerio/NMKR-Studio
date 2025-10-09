using System;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Ipfs
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class UploadToIpfsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public UploadToIpfsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Upload a File pin it to IPFS
        /// </summary>
        /// <remarks>
        /// With this API you can upload a file to IPFS. You can upload the file as BASE64 Content or as URL Link.
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="customerid">The customerid on NMKR Studio</param>
        /// <param Name="nft">The UploadToIpfs Class as Body Content</param>
        /// <response code="200">Returns the UploadToIpfsResult Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">No Image Content was provided. Send a file either as Base64 or as Link or IPFS Hash</response>            
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="409">There is a conflict with the provided images. Send a file either as Base64 or as Link or IPFS Hash</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{customerid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "IPFS" }
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, int customerid, [FromBody] UploadToIpfsClass nft)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = "";

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


            if (nft == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 99;
                result.ErrorMessage = "Invalid JSON Data submitted";
                LogClass.LogMessage(db, "API-CALL: Invalid JSON Data submitted ");
                return StatusCode(500, result);
            }

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


            result = await CheckFiles(db, nft);
            if (result.ResultState != ResultStates.Ok)
            {
                LogClass.LogMessage(db, "API-CALL: Upload Error 406 ");

                return StatusCode(406, result);
            }

            var result1 = await UploadFiles(db, nft,customerid);
            if (result1.result.ResultState != ResultStates.Ok)
            {
                LogClass.LogMessage(db, "API-CALL: Upload Error 406 ");

                return StatusCode(406, result1.result);
            }




            return Ok(result1.ipfshash);
        }

        private class UploadToIpfsResultClass
        {
            public ApiErrorResultClass result { get; init; }
            public string ipfshash { get; set; }
        }

        private async Task<UploadToIpfsResultClass> UploadFiles(EasynftprojectsContext db, UploadToIpfsClass nft, int customerid)
        {

            UploadToIpfsResultClass result1 = new()
            { result = new() { ErrorCode = 0, ErrorMessage = "", ResultState = ResultStates.Ok }, ipfshash = "", };

            var path = GeneralConfigurationClass.TempFilePath;
            string filename = GlobalFunctions.GetGuid();


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
                long filesize = GlobalFunctions.GetFileSize(path + filename);
                await SaveNft(db, ia.Hash,filesize, nft, customerid);

                System.IO.File.Delete(path + filename);
                result1.ipfshash = ia.Hash;
            }

            if (!string.IsNullOrEmpty(nft.FileFromsUrl))
            {
                try
                {
                    LogClass.LogMessage(db, "API-CALL: NFT Upload - Downloading from url: " + nft.FileFromsUrl);

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
                    LogClass.LogMessage(db, "API-CALL: EXCEPTION NFT Upload - Downloading from url: " + nft.FileFromsUrl + " - " + e.Message);
                    result1.result.ErrorCode = 404;
                    result1.result.ErrorMessage = "Error while downloading from URL - " + e.Message;
                    result1.result.ResultState = ResultStates.Error;
                    return result1;
                }

                var ipfs = await IpfsFunctions.AddFileAsync(path + filename);
                Ipfsadd ia = Ipfsadd.FromJson(ipfs);
                System.IO.File.Delete(path + filename);
                long filesize=GlobalFunctions.GetFileSize(path + filename);

                await SaveNft(db, ia.Hash, filesize, nft, customerid);
                result1.ipfshash = ia.Hash;
            }

            return result1;
        }

        private async Task SaveNft(EasynftprojectsContext db, string iaHash, long filesize, UploadToIpfsClass nft, int customerid)
        {
            var checkfornft=await(from a in db.Ipfsuploads
                                  where a.Ipfshash == iaHash && a.CustomerId == customerid
                                  select a).AsNoTracking().FirstOrDefaultAsync();

            if (checkfornft != null)
                return;

            Ipfsupload iu = new Ipfsupload()
            {
                Created = DateTime.Now,
                CustomerId = customerid,
                Ipfshash = iaHash,
                Name = nft.Name ?? "",
                Mimetype = nft.Mimetype,
                Filesize = filesize
            };
            await db.Ipfsuploads.AddAsync(iu);
            await db.SaveChangesAsync();
        }

        private async Task<ApiErrorResultClass> CheckFiles(EasynftprojectsContext db, UploadToIpfsClass nft)
        {
            ApiErrorResultClass result = new() { ErrorCode = 0, ErrorMessage = "", ResultState = ResultStates.Ok };

            if (string.IsNullOrEmpty(nft.FileFromBase64) &&
                  string.IsNullOrEmpty(nft.FileFromsUrl))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 62;
                result.ErrorMessage = "No File provided. Send a file either as Base64 or as Link";
                return result;
            }

            if (!string.IsNullOrEmpty(nft.FileFromBase64) && !string.IsNullOrEmpty(nft.FileFromsUrl))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 63;
                result.ErrorMessage =
                    "Multiple Files provided. Send a File either as Base64 or as Link ";
                return result;
            }

            if (!string.IsNullOrEmpty(nft.FileFromsUrl) && !GlobalFunctions.IsUrlValid(nft.FileFromsUrl))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 69;
                result.ErrorMessage = "URL is not valid";
                return result;
            }
            if  (!await FindMimetype(db, nft.Mimetype))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 68;
                result.ErrorMessage = "Unsupported MIME Type";
                return result;
            }

            return result;
        }
        private async Task<bool> FindMimetype(EasynftprojectsContext db, string nftMimetype)
        {
            var t = await (from a in db.Mimetypes
                where a.Mimetype1 == nftMimetype
                select a).FirstOrDefaultAsync();
            return t != null;
        }

    }
}
