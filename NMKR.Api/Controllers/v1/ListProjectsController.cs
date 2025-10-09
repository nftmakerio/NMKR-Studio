using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class ListProjectsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public ListProjectsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Lists all your projects
        /// </summary>
        /// <remarks>
        /// You will receive a list with all of your projects
        ///
        /// IMPORTANT:
        /// This function uses an internal cache. All results will be cached for 10 seconds. You do not need to call this function more than once in 10 seconds, because the results will be the same.
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <response code="200">Returns the NftProjectsDetails Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
        /// <response code="404">The apikey or the projects where not found</response>
        /// <response code="406">The provided informations are not valid for this request</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<NftProjectsDetails>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]

        [HttpGet("{apikey}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey)
        {
            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = "";

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<NftProjectsDetails>>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }

            // This function can not be used with token only
            if (apikey.StartsWith("token"))
            {
                var result1 = new ApiErrorResultClass
                {
                    ErrorCode = 52,
                    ErrorMessage = "Apikey not found",
                    ResultState = ResultStates.Error
                };
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result1, apiparameter);
                return StatusCode(404, result1);
            }

            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {
                string hash = HashClass.GetHash(SHA256.Create(), apikey);

                var t = (from a in db.Apikeys
                        .Include(a => a.Apikeyaccesses)
                         where a.Apikeyhash == hash
                         select a).AsNoTracking().FirstOrDefault();

                if (t == null)
                {
                    db.Database.CloseConnection();
                    result.ErrorCode = 20;
                    result.ErrorMessage = "Apikey not found";
                    result.ResultState = ResultStates.Error;
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                    return StatusCode(404, result);
                }

                var projects = (from a in db.Nftprojects
                        .Include(a => a.Customerwallet)
                        .AsSplitQuery()
                        .Include(a => a.Usdcwallet)
                        .AsSplitQuery()
                                where a.CustomerId == t.CustomerId && a.State != "deleted"
                                select a).ToList();

                List<NftProjectsDetails> proj1 = new();
                foreach (var p in projects)
                {
                    proj1.Add(new(db, p));
                }

                db.Database.CloseConnection();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, proj1, apiparameter);
                return Ok(proj1);
            }
        }

        /// <summary>
        /// Lists all your projects with pagination
        /// </summary>
        /// <remarks>
        /// You will receive a list with all of your projects
        ///
        /// IMPORTANT:
        /// This function uses an internal cache. All results will be cached for 10 seconds. You do not need to call this function more than once in 10 seconds, because the results will be the same.
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="count">How many projects do you want th get on one page´. Min. 1, Max 50</param>
        /// <param Name="apikey">The Pagenumber. It starts with 1</param>
        /// <response code="200">Returns the NftProjectsDetails Class</response>
        /// <response code="404">The apikey or the projects where not found</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
        /// <response code="406">The provided informations are not valid for this request</response>    
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<NftProjectsDetails>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]

        [HttpGet("{apikey}/{count:int}/{page:int}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey, int count, int page)
        {
            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = count.ToString()+"-"+page.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<NftProjectsDetails>>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }

            // This function can not be used with token only
            if (apikey.StartsWith("token"))
            {
                var result1 = new ApiErrorResultClass
                {
                    ErrorCode = 52,
                    ErrorMessage = "Apikey not found",
                    ResultState = ResultStates.Error
                };
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result1, apiparameter);
                return StatusCode(404, result1);
            }

            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                 "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {
                string hash = HashClass.GetHash(SHA256.Create(), apikey);

                var t = (from a in db.Apikeys
                        .Include(a => a.Apikeyaccesses)
                         where a.Apikeyhash == hash
                         select a).AsNoTracking().FirstOrDefault();

                if (t == null)
                {
                    db.Database.CloseConnection();
                    result.ErrorCode = 20;
                    result.ErrorMessage = "Apikey not found";
                    result.ResultState = ResultStates.Error;
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                    return StatusCode(404, result);
                }

                if (page == 0)
                {
                    result.ErrorCode = 20;
                    result.ErrorMessage = "Pagenumber must start with 1";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }

                if (count == 0)
                {
                    result.ErrorCode = 20;
                    result.ErrorMessage = "Count must be at least 1";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }
                if (count > 50)
                {
                    result.ErrorCode = 20;
                    result.ErrorMessage = "Count can not exceed 50";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }


                var projects = (from a in db.Nftprojects
                        .Include(a => a.Customerwallet)
                        .AsSplitQuery()
                        .Include(a => a.Usdcwallet)
                        .AsSplitQuery()
                                where a.CustomerId == t.CustomerId && a.State != "deleted"
                                select a).Skip((page - 1) * count).Take(count).ToList();

                List<NftProjectsDetails> proj1 = new();
                foreach (var p in projects)
                {
                    proj1.Add(new(db, p));
                }

                db.Database.CloseConnection();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, proj1, apiparameter);
                return Ok(proj1);
            }
        }


        /// <summary>
        /// Lists all your projects with pagination
        /// </summary>
        /// <remarks>
        /// You will receive a list with all of your projects
        ///
        /// IMPORTANT:
        /// This function uses an internal cache. All results will be cached for 10 seconds. You do not need to call this function more than once in 10 seconds, because the results will be the same.
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="customerid">The customerid you want to receive the projects</param>
        /// <param Name="count">How many projects do you want th get on one page´. Min. 1, Max 50</param>
        /// <param Name="apikey">The Pagenumber. It starts with 1</param>
        /// <response code="200">Returns the NftProjectsDetails Class</response>
        /// <response code="404">The apikey or the projects where not found</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
        /// <response code="406">The provided informations are not valid for this request</response>    
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<NftProjectsDetails>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]

        [HttpGet("{apikey}/{customerid:int}/{count:int}/{page:int}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey, int customerid, int count, int page)
        {
            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = customerid+"-"+count.ToString() + "-" + page.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<NftProjectsDetails>>(cachedResult.ResultString));
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


            using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            if (!apikey.StartsWith("token"))
            {
                string hash = HashClass.GetHash(SHA256.Create(), apikey);

                var t = (from a in db.Apikeys
                        .Include(a => a.Apikeyaccesses)
                    where a.Apikeyhash == hash
                    select a).AsNoTracking().FirstOrDefault();

                if (t == null)
                {
                    db.Database.CloseConnection();
                    result.ErrorCode = 20;
                    result.ErrorMessage = "Apikey not found";
                    result.ResultState = ResultStates.Error;
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }

                if (t.CustomerId != customerid)
                {
                    db.Database.CloseConnection();
                    result.ErrorCode = 56;
                    result.ErrorMessage = "Apikey does not match customerid";
                    result.ResultState = ResultStates.Error;
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }
            }

            if (page == 0)
            {
                result.ErrorCode = 20;
                result.ErrorMessage = "Pagenumber must start with 1";
                result.ResultState = ResultStates.Error;
                db.Database.CloseConnection();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }

            if (count == 0)
            {
                result.ErrorCode = 20;
                result.ErrorMessage = "Count must be at least 1";
                result.ResultState = ResultStates.Error;
                db.Database.CloseConnection();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }
            if (count > 50)
            {
                result.ErrorCode = 20;
                result.ErrorMessage = "Count can not exceed 50";
                result.ResultState = ResultStates.Error;
                db.Database.CloseConnection();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }


            var projects = (from a in db.Nftprojects
                    .Include(a => a.Customerwallet)
                    .AsSplitQuery()
                    .Include(a => a.Usdcwallet)
                    .AsSplitQuery()
                where a.CustomerId == customerid && a.State != "deleted"
                select a).Skip((page - 1) * count).Take(count).ToList();

            List<NftProjectsDetails> proj1 = new();
            foreach (var p in projects)
            {
                proj1.Add(new(db, p));
            }

            db.Database.CloseConnection();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, proj1, apiparameter);
            return Ok(proj1);
        }

    }
}