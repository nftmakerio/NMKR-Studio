using System.Collections.Generic;
using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Extensions;

namespace NMKR.Api.Controllers.v2.Misc
{

    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetServerStateController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns a the State of the Server - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetServerStateController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns a the State of the Servers
        /// </summary>
        /// <response code="200">Returns an array of ServerStateClass</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServerStateClass[]))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Misc"}
        )]
        public async Task<IActionResult> Get()
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            var bg=await (from a in db.Backgroundservers
                          where a.Monitorthisserver==true && a.Name!="development"
                          select a).AsNoTracking().ToListAsync();

            List<ServerStateClass> serverStateClasses = new List<ServerStateClass>();
            foreach (var b in bg)
            {
                List<BackgroundTaskEnums> bgtasks = new List<BackgroundTaskEnums>();

                if (b.Checkpaymentaddresses)
                    bgtasks.Add(BackgroundTaskEnums.checkpaymentaddresses);
                if (b.Checkpaymentaddressessolana)
                    bgtasks.Add(BackgroundTaskEnums.checkpaymentaddressessolana);
                if (b.Checkdoublepayments)
                    bgtasks.Add(BackgroundTaskEnums.checkdoublepayments);
                if (b.Checkpolicies)
                    bgtasks.Add(BackgroundTaskEnums.checkpolicies);
                if (b.Checkpoliciessolana)
                    bgtasks.Add(BackgroundTaskEnums.checkpoliciessolana);
                if (b.Executedatabasecommands)
                    bgtasks.Add(BackgroundTaskEnums.executedatabasecommands);
                if (b.Checkforfreepaymentaddresses)
                    bgtasks.Add(BackgroundTaskEnums.checkfreepaymentaddresses);
                if (b.Checkcustomeraddresses)
                    bgtasks.Add(BackgroundTaskEnums.checkcustomeraddresses);
                if (b.Checkcustomeraddressessolana)
                    bgtasks.Add(BackgroundTaskEnums.checkcustomeraddressessolana);
                if (b.Checkforpremintedaddresses)
                    bgtasks.Add(BackgroundTaskEnums.checkpremintedaddresses);
                if (b.Executepayoutrequests)
                    bgtasks.Add(BackgroundTaskEnums.executepayouts);
                if (b.Checkforexpirednfts)
                    bgtasks.Add(BackgroundTaskEnums.checkexpiredpaymentaddresses);
                if (b.Checkforburningendpoints)
                    bgtasks.Add(BackgroundTaskEnums.checkburningaddresses);
                if (b.Checkprojectaddresses)
                    bgtasks.Add(BackgroundTaskEnums.checkpayinaddresses);
                if (b.Checkmintandsend)
                    bgtasks.Add(BackgroundTaskEnums.mintandsend);
                if (b.Checkmintandsendsolana)
                    bgtasks.Add(BackgroundTaskEnums.mintandsendsolana);
                if (b.Checklegacyauctions)
                    bgtasks.Add(BackgroundTaskEnums.checklegacyauctions);
                if (b.Checklegacydirectsales)
                    bgtasks.Add(BackgroundTaskEnums.checklegacydirectsales);
                if (b.Checknotificationqueue)
                    bgtasks.Add(BackgroundTaskEnums.checknotificationqueue);
                if (b.Executesubmissions)
                    bgtasks.Add(BackgroundTaskEnums.executesubmissions);
                if (b.Checkdecentralsubmits)
                    bgtasks.Add(BackgroundTaskEnums.checkdecentralsubmits);
                if (b.Checkroyaltysplitaddresses)
                    bgtasks.Add(BackgroundTaskEnums.checkroyaltysplitaddresses);
                if (b.Checktransactionconfirmations)
                    bgtasks.Add(BackgroundTaskEnums.checktransactionconfirmations);
                if (b.Checkbuyinsmartcontractaddresses)
                    bgtasks.Add(BackgroundTaskEnums.checkbuyinsmartcontractaddresses);
                if (b.Checkcustomerchargeaddresses)
                    bgtasks.Add(BackgroundTaskEnums.checkcustomerchargeaddresses);

                ServerStateClass serverStateClass = new ServerStateClass()
                {
                    ServerId = b.Id,
                    ServerName = b.Name,
                    ServerState = b.State,
                    LastLifeSign = b.Lastlifesign,
                    CardanoSlot= b.Slot,
                    CardanoEpoch = b.Epoch,
                    CardanoSyncprogress = b.Syncprogress,
                    CardanoNodeVersion = b.Nodeversion,
                    ActualTask= string.IsNullOrEmpty(b.Actualtask) ? BackgroundTaskEnums.none :  b.Actualtask.ToEnum<BackgroundTaskEnums>(),
                    ServerVersion = b.Runningversion,
                    BackgroundTasks = bgtasks.ToArray(),
                };
                serverStateClasses.Add(serverStateClass);
            }


            return Ok(serverStateClasses.ToArray());
        }
    }
}
