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
    public class GetCustomerTransactionsController : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;

        public GetCustomerTransactionsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns all Transactions of a customer
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The id of your customer</param>
        /// <param Name="fromdate">(Optional) - The transactions starting from this date</param>
        /// <param Name="todate">(Optional) - The transactions up to this date</param>
        /// <param Name="exportOptions">You can receive a CSV File, a Zipped CSV File or a direct JSON Result</param>
        /// <response code="200">Returns the GetTransactionsClass</response>
        /// <response code="201">Returns the results as CSV File</response>
        /// <response code="200">Returns the result as ZIP File</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The project was not found in our database or not assiged to your account</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetTransactionsClass[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{customerid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Customer" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, int customerid, [FromQuery] DateTime? fromdate = null, [FromQuery] DateTime? todate = null, [FromQuery] TransactionsExportOptions exportOptions = TransactionsExportOptions.Json)
        {

          //  string apikey = Request.Headers["authorization"];
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
                result.ErrorMessage = "Customer id not correct";
                result.ResultState = ResultStates.Error;
                return StatusCode(401, result);
            }



            var transactions = await (from a in db.Transactions
                    .Include(a => a.TransactionsAdditionalpayouts).AsSplitQuery()
                    .Include(a => a.TransactionNfts)
                    .ThenInclude(a => a.Nft)
                    .ThenInclude(a => a.Nftproject).AsSplitQuery()
                    .Include(a => a.Nftproject).AsSplitQuery()
                                      where (a.CustomerId == customerid) &&
                                            (fromdate == null || a.Created >= fromdate) && (todate == null || a.Created <= todate) &&
                                            a.Transactiontype != nameof(TransactionTypes.paidfailedtransactiontocustomeraddress) &&
                                            a.Transactiontype != nameof(TransactionTypes.doublepaymentsendbacktobuyer)
                                            && a.Transactiontype != nameof(TransactionTypes.mintfromnftmakeraddress)
                                      orderby a.Id descending
                                      select new GetTransactionsClass
                                      {
                                          Created = a.Created,
                                          State = a.State,
                                          NftprojectId = a.NftprojectId ?? 0,
                                          NftProjectUid = a.Nftproject.Uid,
                                          Projectname=a.Nftproject.Projectname,
                                          Ada = a.Ada,
                                          Fee = a.Fee ?? 0,
                                          Mintingcostsada = a.Mintingcostsada ?? 0,
                                          Projectada = a.Projectada ?? 0,
                                          Projectincomingtxhash = a.Projectincomingtxhash,
                                          Receiveraddress = a.Receiveraddress,
                                          Senderaddress = a.Senderaddress,
                                          Transactionid = a.Transactionid,
                                          Transactiontype = a.Transactiontype,
                                          Projectaddress = a.Projectaddress,
                                          Eurorate = a.Eurorate ?? 0,
                                          Nftcount = a.TransactionNfts.Count(),
                                          Tokencount = a.TransactionNfts.Sum(x => x.Tokencount),
                                          Originatoraddress = a.Originatoraddress,
                                          Stakereward = a.Stakereward ?? 0,
                                          Stakeaddress = a.Stakeaddress,
                                          AdditionalPayoutWallets = a.TransactionsAdditionalpayouts.Sum(x => x.Lovelace),
                                          Confirmed = a.Confirmed,
                                          Priceintokensquantity = a.Priceintokensquantity ?? 0,
                                          Priceintokenspolicyid = a.Priceintokenspolicyid,
                                          Priceintokenstokennamehex = a.Priceintokenstokennamehex,
                                          Priceintokensmultiplier = a.Priceintokensmultiplier ?? 1,
                                          Nmkrcosts = a.Nmkrcosts,
                                          Discount = a.Discount ?? 0,
                                          CustomerProperty = a.Customerproperty,
                                          Coin = a.Coin,
                                          Blockchain = a.Coin == Coin.ADA.ToString() ? Blockchain.Cardano : Blockchain.Solana,
                                          TransactionNfts = a.TransactionNfts.Select(x => new GetTransactionNftsClass
                                          {
                                              AssetName = ((x.Nft.Nftproject.Tokennameprefix ?? "") + x.Nft.Name).ToHex(),
                                              Fingerprint = x.Nft.Fingerprint,
                                              TokenCount = x.Tokencount,
                                              Multiplier = x.Multiplier,
                                              TxHashSolanaTransaction = x.Txhash,
                                              Confirmed = x.Confirmed ?? true,
                                          }).ToArray()
                                      }).AsNoTracking().ToListAsync();


            var path = GeneralConfigurationClass.TempFilePath;
            string filteredfilename = $"transactions_{customerid}";
            string filteredcsvfilename = $"transactions_nfts_{customerid}";
            string filename = path + filteredfilename + ".zip";
            string csvfilename = path + filteredfilename + ".csv";
            string csvnftsfilename = path + filteredfilename + "_nfts.csv";

            if (exportOptions == TransactionsExportOptions.Csv || exportOptions == TransactionsExportOptions.Zip)
            {
                System.IO.Directory.CreateDirectory(path);
                GlobalFunctions.DeleteFile(filename);
                GlobalFunctions.DeleteFile(csvfilename);

                GlobalFunctions.DeleteOldFiles(path, 1);

                await using (var writer = new StreamWriter(csvfilename))
                await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(transactions);

                }
                await using (var writer = new StreamWriter(csvnftsfilename))
                await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<GetTransactionNftsCsvExportClass>();
                    await csv.NextRecordAsync();
                    foreach (var transaction in transactions)
                    {
                        foreach (var nft in transaction.TransactionNfts)
                        {
                            csv.WriteRecord(new GetTransactionNftsCsvExportClass(nft, transaction.Transactionid));
                            await csv.NextRecordAsync();
                        }

                    }
                }
                if (exportOptions == TransactionsExportOptions.Csv)
                    return PhysicalFile(csvfilename, "text/csv", csvfilename);
            }

            if (exportOptions == TransactionsExportOptions.Zip)
            {

                using var archive = ZipFile.Open(filename, ZipArchiveMode.Create);
                archive.CreateEntryFromFile(csvfilename, Path.GetFileName(filteredfilename + ".csv"), CompressionLevel.Optimal);
                archive.CreateEntryFromFile(csvnftsfilename, Path.GetFileName(filteredcsvfilename + ".csv"), CompressionLevel.Optimal);
                archive.Dispose();
                return PhysicalFile(filename, "application/zip", csvfilename);
            }


            return Ok(transactions);
        }
    }
}
