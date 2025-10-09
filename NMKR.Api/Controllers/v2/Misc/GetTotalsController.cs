using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Newtonsoft.Json;
using System.Collections.Generic;
using Asp.Versioning;

namespace NMKR.Api.Controllers.v2.Misc
{
    [ApiExplorerSettings(IgnoreApi = true)]

    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetTotalsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

     
        public GetTotalsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Getstatisticsview[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{month:int}/{year:int}")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, int month, int year)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = month.ToString() + year.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<Getstatisticsview[]>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            if (year < 2022 || year > 2050)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 379;
                result.ErrorMessage = "Year must be between 2022 and 2050";
                return StatusCode(406, result);
            }

            if (month < 1 || month > 12)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 379;
                result.ErrorMessage = "Month must be between 1 and 12";
                return StatusCode(406, result);
            }

            var stats = await LoadStatistics(month, year);
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, stats, apiparameter);

            return Ok(stats);
        }



        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Getprojectstatisticsview[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{month:int}/{year:int}/{userid:int}")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, int month, int year, int userid)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = month.ToString() + year.ToString() + userid.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<Getprojectstatisticsview[]>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            if (year < 2022 || year > 2050)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 379;
                result.ErrorMessage = "Year must be between 2022 and 2050";
                return StatusCode(406, result);
            }

            if (month < 1 || month > 12)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 379;
                result.ErrorMessage = "Month must be between 1 and 12";
                return StatusCode(406, result);
            }

            var stats = await LoadStatisticsProjects(month, year, userid);
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, stats, apiparameter);

            return Ok(stats);
        }

        private async Task<Getprojectstatisticsview[]> LoadStatisticsProjects(int month, int year, int userid)
        {

            await using EasynftprojectsContext db = new(GlobalFunctions.optionsBuilder.Options);
            DateTime dt = new DateTime(year, month, 1);

            // We need the View to call a procedure - unfortunately we can not use entity framework with the from raw alone - without view
            var _stats2 = db.Getprojectstatisticsviews.FromSqlRaw("Call GetProjectStatistics(@fromdate, @todate)",
                new MySqlParameter("fromdate", dt.Date),
                new MySqlParameter("todate", dt.AddMonths(1).Date)
            ).AsEnumerable().OrderByDescending(a => a.Totaltransactions).ToList();

            var stats3 = _stats2.Where(x => x.CustomerId == userid).ToArray();

            return stats3;
        }

        private async Task<Getstatisticsview[]> LoadStatistics(int month, int year)
        {
            List<Getstatisticsview> _stats = new();

           DateTime dt=new DateTime(year,month,1);

            await using EasynftprojectsContext db = new(GlobalFunctions.optionsBuilder.Options);
            _stats = db.Getstatisticsviews.FromSqlRaw("Call GetStatistics(@fromdate, @todate)",
                new MySqlParameter("fromdate", dt.Date),
                new MySqlParameter("todate", dt.AddMonths(1).Date)
            ).ToList();

            return _stats.ToArray();
        }

    }
}
