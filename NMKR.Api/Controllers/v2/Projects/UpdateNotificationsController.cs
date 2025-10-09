using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Api.Controllers.SharedClasses;
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
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class UpdateNotificationsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public UpdateNotificationsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Updates the notifications of a project
        /// </summary>
        /// <remarks>
        /// WIth this Controller you can update the notifications. All old entries will be deleted. If you want to clear the notifications, just send an empty array
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on studio.nmkr.io</param>
        /// <response code="200">The notifications was successfully updated</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">Project not found</response>            
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetNotificationsClass[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{projectuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Projects" }
        )]
        public async Task<IActionResult> Put([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid,
            [FromBody] NotificationsClassV2[] notifications)
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


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var customer = CheckApiAccess.GetCustomer(_redis, db, apikey);
            if (customer == null)
            {
                result.ErrorCode = 124;
                result.ErrorMessage = "Customer not found";
                result.ResultState = ResultStates.Error;
                return StatusCode(401, result);
            }

            var project = await (from a in db.Nftprojects
                                 where a.Uid == projectuid
                                 select a).AsNoTracking().FirstOrDefaultAsync();

            if (project == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 553;
                result.ErrorMessage = "Project UID not found";
                return NotFound(result);
            }

            // Check the notifications
            SaveProjectFunctions spf = new SaveProjectFunctions();
            result = spf.CheckNotifications(db, notifications, result);
            if (result.ResultState == ResultStates.Error)
                return StatusCode(406, result);

            // Delete the old notifications
            string sql = $"delete from notifications where nftproject_id={project.Id}";
            await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, sql);

            // Save the new notifications
            await spf.SaveNotifications(db, notifications, project.Id);

            var res = await spf.GetNotifications(db, projectuid);

            return StatusCode(200,res);
        }

    }
}
