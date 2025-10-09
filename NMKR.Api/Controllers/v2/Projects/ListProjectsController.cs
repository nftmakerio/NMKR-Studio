using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace NMKR.Api.Controllers.v2.Projects
{
    /// <summary>
    /// The Order options for the project list
    /// </summary>
    public enum ProjectSortOptions
    {
        /// <summary>
        /// Orders the list by the creation date ascending
        /// </summary>
        created,
        /// <summary>
        /// Orders the list by the creation date descending
        /// </summary>
        created_desc,
        /// <summary>
        /// Orders the list by the project name ascending
        /// </summary>
        name,
        /// <summary>
        /// Orders the list by the project name descending
        /// </summary>
        name_desc,
        /// <summary>
        /// Orders the list by the free tokens ascending
        /// </summary>
        freetokens,
        /// <summary>
        /// Orders the list by the free tokens descending
        /// </summary>
        freetokens_desc,
        /// <summary>
        /// Ordes the list by the sold tokens ascending
        /// </summary>
        soldtokens,
        /// <summary>
        /// Ordes the list by the sold tokens descending
        /// </summary>
        soldtokens_desc,
        /// <summary>
        /// Orders the list by the reserved tokens ascending
        /// </summary>
        reservedtokens,
        /// <summary>
        /// Orders the list by the reserved tokens descending
        /// </summary>
        reservedtokens_desc,
    }


    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
   
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
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <response code="200">Returns the NftProjectsDetails Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
        /// <response code="404">The apikey or the projects where not found</response>
        /// <response code="406">The provided informations are not valid for this request</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<NftProjectsDetails>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]

        [HttpGet]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Projects" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, [FromQuery] ProjectSortOptions optionalSortOrder = ProjectSortOptions.created)
        {
          //  string apikey = Request.Headers["authorization"];
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
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<NftProjectsDetails>>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }

            // This function can not be used with token only
            if (!string.IsNullOrEmpty(apikey) && apikey.StartsWith("token"))
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
            var result = CheckCachedAccess.CheckApikeyOrToken (_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            string hash = HashClass.GetHash(SHA256.Create(), apikey);

            var t = await (from a in db.Apikeys
                    .Include(a => a.Apikeyaccesses)
                where a.Apikeyhash == hash
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (t == null)
            {
                await db.Database.CloseConnectionAsync();
                result.ErrorCode = 20;
                result.ErrorMessage = "Apikey not found";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return StatusCode(404, result);
            }

            var projects = await (from a in db.Nftprojects
                    .Include(a => a.Customerwallet)
                    .AsSplitQuery()
                    .Include(a => a.Usdcwallet)
                    .AsSplitQuery()
                where a.CustomerId == t.CustomerId && a.State != "deleted"
                select a).ToListAsync();

            List<NftProjectsDetails> proj1 = new();
            foreach (var p in projects)
            {
                proj1.Add(new(db, p));
            }

            proj1 = SortProjects(proj1, optionalSortOrder, 0,100000);


            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, proj1, apiparameter);
            return Ok(proj1);
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

        [HttpGet("{count:int}/{page:int}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Projects" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, int count, int page, [FromQuery]ProjectSortOptions optionalSortOrder = ProjectSortOptions.created)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

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
            if (!string.IsNullOrEmpty(apikey) && apikey.StartsWith("token"))
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


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            string hash = HashClass.GetHash(SHA256.Create(), apikey);

            var t = await (from a in db.Apikeys
                    .Include(a => a.Apikeyaccesses)
                where a.Apikeyhash == hash
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (t == null)
            {
                await db.Database.CloseConnectionAsync();
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
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }

            if (count == 0)
            {
                result.ErrorCode = 20;
                result.ErrorMessage = "Count must be at least 1";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }
            if (count > 50)
            {
                result.ErrorCode = 20;
                result.ErrorMessage = "Count can not exceed 50";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }


            var projects =await  (from a in db.Nftprojects
                    .Include(a => a.Customerwallet)
                    .AsSplitQuery()
                    .Include(a => a.Usdcwallet)
                    .AsSplitQuery()
                where a.CustomerId == t.CustomerId && a.State != "deleted"
                select a).ToListAsync();//     .Skip((page - 1) * count).Take(count).ToListAsync();


            List<NftProjectsDetails> proj1 = new();
            foreach (var p in projects)
            {
                proj1.Add(new(db, p));
            }

            proj1 = SortProjects(proj1, optionalSortOrder, page, count);

            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, proj1, apiparameter);
            return Ok(proj1);
        }

        private List<NftProjectsDetails> SortProjects(List<NftProjectsDetails> proj1, ProjectSortOptions sort, int page, int count)
        {
            switch (sort)
            {
                case ProjectSortOptions.created:
                    proj1 = proj1.OrderBy(a => a.Created).Skip((page - 1) * count).Take(count).ToList();
                    break;
                case ProjectSortOptions.created_desc:
                    proj1 = proj1.OrderByDescending(a => a.Created).Skip((page - 1) * count).Take(count).ToList();
                    break;
                case ProjectSortOptions.name:
                    proj1 = proj1.OrderBy(a => a.Projectname).Skip((page - 1) * count).Take(count).ToList();
                    break;
                case ProjectSortOptions.name_desc:
                    proj1 = proj1.OrderByDescending(a => a.Projectname).Skip((page - 1) * count).Take(count).ToList();
                    break;
                case ProjectSortOptions.freetokens:
                    proj1 = proj1.OrderBy(a => a.Free).Skip((page - 1) * count).Take(count).ToList();
                    break;
                case ProjectSortOptions.freetokens_desc:
                    proj1 = proj1.OrderByDescending(a => a.Free).Skip((page - 1) * count).Take(count).ToList();
                    break;
                case ProjectSortOptions.soldtokens:
                    proj1 = proj1.OrderBy(a => a.Sold).Skip((page - 1) * count).Take(count).ToList();
                    break;
                case ProjectSortOptions.soldtokens_desc:
                    proj1 = proj1.OrderByDescending(a => a.Sold).Skip((page - 1) * count).Take(count).ToList();
                    break;
                case ProjectSortOptions.reservedtokens:
                    proj1 = proj1.OrderBy(a => a.Reserved).Skip((page - 1) * count).Take(count).ToList();
                    break;
                case ProjectSortOptions.reservedtokens_desc:
                    proj1 = proj1.OrderByDescending(a => a.Reserved).Skip((page - 1) * count).Take(count).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sort), sort, null);
            }

            return proj1;
        }
    }
}