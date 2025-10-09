using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.PricelistFunctions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Misc
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetPublicMintsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns a the State of the Server - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetPublicMintsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns a list of current or upcoming mints
        /// </summary>
        /// <response code="200">Returns an array of PublicMintsClass</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PublicMintsClass[]))]
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

            var projects = await (from a in db.Nftprojects
                    .Include(a => a.Customer)
                    .Include(a => a.Pricelists).AsSplitQuery()
                where a.State == "active" && a.Publishmintto3rdpartywebsites && a.Projecttype == "nft-project"
                      && (a.Paymentgatewaysalestart == null || a.Paymentgatewaysalestart.Value <= DateTime.Now.AddDays(5)) &&
                      a.Created > DateTime.Now.AddDays(-120)
                      orderby a.Paymentgatewaysalestart
                select a).AsNoTracking().ToListAsync();

            List<PublicMintsClass> publicMintsClasses = new List<PublicMintsClass>();
            foreach (var project in projects)
            {
                publicMintsClasses.Add(await GetProjectInfo(db,project));
            }

            return Ok(publicMintsClasses.ToArray());
        }

        private async Task<PublicMintsClass> GetProjectInfo(EasynftprojectsContext db, Nftproject project)
        {
            List<Blockchain> bc = new List<Blockchain>();
            if (project.Enabledcoins.Contains(Coin.SOL.ToString()))
                bc.Add(Blockchain.Solana);
            if (project.Enabledcoins.Contains(Coin.ADA.ToString()))
                bc.Add(Blockchain.Cardano);


            PublicMintsClass res = new PublicMintsClass()
            {
                ProjectName = project.Projectname,
                ProjectDescription = project.Description,
                ProjectImage = project.Projectlogo,
                ProjectUrl = project.Projecturl,
                ProjectCreated = (DateTime) project.Created,
                Blockchains = bc.ToArray(),
                MintStart = (DateTime) project.Paymentgatewaysalestart,
                MintState = GetProjectState(project),
                CreaterName = GetCreatorName(project),
                Pricelist = await GetPricelist(db,project),
                TotalNfts = project.Total1,
                ReservedNfts = project.Reserved1,
                SoldNfts = project.Sold1
            };

            return res;
        }

        private async Task<PricelistClass[]> GetPricelist(EasynftprojectsContext db, Nftproject project)
        {
            var pl1 = await GetPricelistClass.GetPriceList(db, project, _redis);
            return pl1.ToArray();
        }

        private string GetCreatorName(Nftproject project)
        {
           if (!string.IsNullOrEmpty(project.Customer.Company))
               return project.Customer.Company;
           return project.Customer.Firstname + " " + project.Customer.Lastname;
        }

        private PublicMintState GetProjectState(Nftproject project)
        {
            if (project.Paymentgatewaysalestart != null && project.Paymentgatewaysalestart.Value > DateTime.Now)
                return PublicMintState.Upcoming;

            if (project.Free1 == 0)
                return PublicMintState.SoldOut;

            if (project.Policyexpire!=null && project.Policyexpire < DateTime.Now)
                return PublicMintState.Ended;

            return PublicMintState.Active;
        }
    }
}
