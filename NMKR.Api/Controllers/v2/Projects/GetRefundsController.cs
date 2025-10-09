using System;
using System.Linq;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using Asp.Versioning;

namespace NMKR.Api.Controllers.v2.Projects
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetRefundsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetRefundsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns all Refunds of a project
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project (not the id)</param>
        /// <param Name="fromdate">(Optional) - The refunds starting from this date</param>
        /// <param Name="todate">(Optional) - The refunds up to this date</param>
        /// <param Name="exportOptions">You can receive a CSV File, a Zipped CSV File or a direct JSON Result</param>
        /// <response code="200">Returns the GetRefundsClass</response>
        /// <response code="201">Returns the results as CSV File</response>
        /// <response code="200">Returns the result as ZIP File</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The project was not found in our database or not assiged to your account</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetRefundsClass[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Projects" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid, [FromQuery] DateTime? fromdate = null, [FromQuery] DateTime? todate = null, [FromQuery] TransactionsExportOptions exportOptions = TransactionsExportOptions.Json)
        {

            //string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;




            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = projectuid;

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
                projectuid, apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }




            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);


            var refunds = await (from a in db.Refundlogs
                    .Include(a => a.Nftproject)
                where (a.Nftproject.Uid == projectuid || projectuid == null) &&
                      (fromdate == null || a.Created >= fromdate) && (todate == null || a.Created <= todate)
                                 select new GetRefundsClass()
                {
                   Created = a.Created,
                   State = a.State,
                    NftprojectId = a.NftprojectId,
                    Senderaddress = a.Senderaddress,
                    Receiveraddress = a.Receiveraddress,
                    Refundreason = a.Refundreason,
                    Txhash = a.Txhash,
                    Outgoingtxhash = a.Outgoingtxhash,
                    Lovelace = a.Lovelace??0,
                    Fee = a.Fee??0
                }).AsNoTracking().ToListAsync(); ;


            var path = GeneralConfigurationClass.TempFilePath;
            string filteredfilename = $"transactions_{projectuid}";
            string filename = path + filteredfilename + ".zip";
            string csvfilename = path + filteredfilename + ".csv";
            if (exportOptions == TransactionsExportOptions.Csv || exportOptions == TransactionsExportOptions.Zip)
            {
                System.IO.Directory.CreateDirectory(path);
                GlobalFunctions.DeleteFile(filename);
                GlobalFunctions.DeleteFile(csvfilename);

                GlobalFunctions.DeleteOldFiles(path, 1);

                await using (var writer = new StreamWriter(csvfilename))
                await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(refunds);

                }

                if (exportOptions == TransactionsExportOptions.Csv)
                    return PhysicalFile(csvfilename, "text/csv", csvfilename);
            }

            if (exportOptions == TransactionsExportOptions.Zip)
            {

                using var archive = ZipFile.Open(filename, ZipArchiveMode.Create);
                archive.CreateEntryFromFile(csvfilename, Path.GetFileName(filteredfilename + ".csv"), CompressionLevel.Optimal);
                archive.Dispose();
                return PhysicalFile(filename, "application/zip", csvfilename);
            }


            return Ok(refunds);
        }
    }
}
