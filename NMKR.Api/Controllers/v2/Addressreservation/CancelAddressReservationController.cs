using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using NMKR.Shared.NotificationClasses;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Addressreservation
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class CancelAddressReservationController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IBus _bus;

        public CancelAddressReservationController(IConnectionMultiplexer redis, IBus bus)
        {
            _redis = redis;
            _bus = bus;
        }

        /// <summary>
        /// Cancels a address reservation (project uid)
        /// </summary>
        /// <remarks>
        /// When you call this API, the reservation of all nfts dedicated to this address will released to free state. This function can be called, when a user closes his browser or when he hit on a "Cancel Reservation" Button
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project (not the id)</param>
        /// <param Name="paymentaddress">The address which has to be canceled</param>
        /// <response code="200">Cancellation was successful</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">Address not found</response>
        /// <response code="406">Address is not in active state - eg. already paid or already released to free</response>            
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}/{paymentaddress}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Address reservation (sale)" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid, string paymentaddress)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");
            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = (HttpContext.Connection.RemoteIpAddress) ?? IPAddress.None;

            string apifunction = this.GetType().Name;
            string apiparameter = projectuid + paymentaddress;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));

            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                projectuid, apikey, remoteIpAddress?.ToString() ?? string.Empty);
            if (result.ResultState == ResultStates.Ok)
                return await Cancel(result, new(apikey, apifunction, apiparameter), projectuid, paymentaddress,
                    remoteIpAddress.ToString());


            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
            return Unauthorized(result);

        }

        internal async Task<IActionResult> Cancel(ApiErrorResultClass result, CachedApiCallValues cachedApiCallValues, string projectuid, string paymentaddress, string remoteIpAddress)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var project = await (from a in db.Nftprojects
                where a.Uid == projectuid
                select a).FirstOrDefaultAsync();

            if (project == null)
            {
                LogClass.LogMessage(db,
                    "API-CALL from " + remoteIpAddress +
                    ": Cancelreservation - ERROR: Project not found" +
                    projectuid, paymentaddress);
                result.ErrorCode = 117;
                result.ErrorMessage = "Project not found";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 404, result);
                await db.Database.CloseConnectionAsync();
                return StatusCode(404, result);
            }


            await GlobalFunctions.UpdateLastActionProjectAsync(db, project.Id,_redis);

            var nftaddress = await (from a in db.Nftaddresses where a.Address == paymentaddress && a.NftprojectId == project.Id select a).FirstOrDefaultAsync();

            if (nftaddress == null)
            {
                LogClass.LogMessage(db,
                    "API-CALL from " + remoteIpAddress +
                    ": Cancelreservation - ERROR: Address not found" +
                    project.Id, paymentaddress);
                result.ErrorCode = 113;
                result.ErrorMessage = "Address not found";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 404, result);
                await db.Database.CloseConnectionAsync();
                return StatusCode(404, result);
            }

            switch (nftaddress.State)
            {
                case "paid":
                    LogClass.LogMessage(db,
                        "API-CALL from " + remoteIpAddress + ": Cancelreservation - ERROR: Address already paid " +
                        project.Id,paymentaddress);
                    result.ErrorCode = 114;
                    result.ErrorMessage = "Address is already paid - Cancelreservation not possible";
                    result.ResultState = ResultStates.Error;
                    CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 406, result);
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                case "expired":
                    LogClass.LogMessage(db,
                        "API-CALL from " + remoteIpAddress + ": Cancelreservation - ERROR: Address already expired " +
                        project.Id,paymentaddress);
                    result.ErrorCode = 115;
                    result.ErrorMessage = "Address is already expired - Cancelreservation not possible";
                    result.ResultState = ResultStates.Error;
                    CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 406, result);
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                case "rejected":
                    LogClass.LogMessage(db,
                        "API-CALL from " + remoteIpAddress + ": Cancelreservation - ERROR: Address already rejected " +
                        project.Id, paymentaddress);
                    result.ErrorCode = 115;
                    result.ErrorMessage = "Address is already rejected - Cancelreservation not possible";
                    result.ResultState = ResultStates.Error;
                    CheckCachedAccess.SetCachedResult(_redis, cachedApiCallValues, 406, result);
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
            }

            if (nftaddress.State != "active")
            {
                LogClass.LogMessage(db,
                    "API-CALL from " + remoteIpAddress +
                    ": Cancelreservation - ERROR: Address not in active state " +
                    project.Id + " - State: " + nftaddress.State);
                result.ErrorCode = 116;
                result.ErrorMessage = "Address is not in active state - Cancelreservation not possible - State = " +
                                      nftaddress.State;
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis,cachedApiCallValues, 406, result);
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }
            LogClass.LogMessage(db,
                "API-CALL from " + remoteIpAddress +
                ": Cancelreservation " + nftaddress.Address,
                project.Id + " - State: " + nftaddress.State);


            await GlobalFunctions.ReleaseNftAsync(db,_redis, nftaddress.Id);

            // Send Notifications via NotificationServer
            await _bus.Publish(new RmqTransactionClass { AddressId = nftaddress.Id, ProjectId = nftaddress.NftprojectId, EventType = NotificationEventTypes.transactioncanceled, TransactionId = null});


            nftaddress.State = "expired";
            nftaddress.Reservationtoken = null;
            await db.SaveChangesAsync();


            await db.Database.CloseConnectionAsync();

            return Ok(result);
        }
    }
}

