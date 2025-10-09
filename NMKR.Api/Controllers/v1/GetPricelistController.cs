using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using NMKR.Shared.Functions.PricelistFunctions;

namespace NMKR.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class GetPricelistController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetPricelistController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns the actual valid pricelist for this project (project id)
        /// </summary>
        /// <remarks>
        /// You will get the predefined prices for one or more nf
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <response code="200">Returns an array of the PricelistClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PricelistClass>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}/{nftprojectid:int}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey, int nftprojectid)
        {
            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = nftprojectid.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<PricelistClass>>(cachedResult.ResultString));
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

            var project = (from a in db.Nftprojects
                where a.Id == nftprojectid
                           select a).FirstOrDefault();

            if (project == null)
            {
                result.ErrorCode = 50;
                result.ErrorMessage = "Projectid not known";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            var pl1 = GetPricelistClass.GetPriceList(db, project, _redis);

            db.Database.CloseConnection();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, pl1, apiparameter);
            return Ok(pl1);
        }

        private Tokens[] GetAdditionalTokens(Pricelist pricelist)
        {
            if (pricelist.Priceintoken != null && pricelist.Priceintoken != 0)
            {
                return new Tokens[1]
                {
                    new()
                    {
                        AssetNameInHex = pricelist.Assetnamehex??pricelist.Tokenassetid.ToHex(),
                        AssetName = pricelist.Tokenassetid,
                        CountToken = (long) pricelist.Priceintoken/(pricelist.Tokenmultiplier??1),
                        PolicyId = pricelist.Tokenpolicyid,
                        TotalCount=(long) pricelist.Priceintoken,
                        Multiplier=pricelist.Tokenmultiplier??1,
                        Decimals=GlobalFunctions.GetDecimalsFromMultiplier(pricelist.Tokenmultiplier),
                    }
                };
            }

            return new Tokens[] { };
        }


        /// <summary>
        /// Returns the actual valid pricelist for this project (project uid)
        /// </summary>
        /// <remarks>
        /// You will get the predefined prices for one or more nf
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project (not the id)</param>
        /// <response code="200">Returns an array of the PricelistClass</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PricelistClass>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}/{projectuid}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey, string projectuid)
        {
            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = projectuid.ToString();

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<PricelistClass>>(cachedResult.ResultString));
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
            var rates = GlobalFunctions.GetNewRates(_redis,Coin.ADA);

            double eur = 0;
            double usd = 0;
            double jpy = 0;
            DateTime effdate = DateTime.Now;

            if (rates != null)
            {
                eur = rates.EurRate;
                usd = (float) rates.UsdRate;
                jpy = (float)rates.JpyRate;
                effdate = rates.EffectiveDate;
            }
            var project = (from a in db.Nftprojects
                where a.Uid == projectuid
                select a).FirstOrDefault();

            if (project == null)
            {
                result.ErrorCode = 50;
                result.ErrorMessage = "Projectuid not known";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return StatusCode(406, result);
            }

            var pl = (from a in db.Pricelists
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Settings)
                where a.NftprojectId == project.Id && (a.Validfrom == null || a.Validfrom <= DateTime.Now) &&
                      (a.Validto == null || a.Validto >= DateTime.Now) && a.State == "active"
                select a).ToList();

            List<PricelistClass> pl1 = new();

            foreach (var pricelist in pl)
            {
                long price =  GlobalFunctions.GetPriceInEntities(_redis, pricelist);
                if (price == 0)
                    price = 2000000;

                var model = new PaybuttonClass(project);
                model.Pricelist = pricelist;
                long sendback =
                    GlobalFunctions.CalculateSendbackToUser(db,_redis, pricelist.Countnftortoken, pricelist.NftprojectId);

                pl1.Add(new()
                {
                    CountNft = pricelist.Countnftortoken,
                    PriceInLovelace = price,
                    PriceInEur = (float)Math.Round((eur * price / 1000000), 2),
                    PriceInUsd = (float)Math.Round((usd * price / 1000000), 2),
                    PriceInJpy = (float)Math.Round((jpy * price / 1000000), 2),
                    Effectivedate = effdate,
                    PaymentGatewayLinkForRandomNftSale=model.PaywindowLink,
                    Currency = pricelist.Currency,
                    SendBackCentralPaymentInLovelace =sendback,
                    PriceInLovelaceCentralPayments = pricelist.Nftproject.Enabledecentralpayments ? price + sendback : price,
                    AdditionalPriceInTokens = GetAdditionalTokens(pricelist),
                });
            }

            db.Database.CloseConnection();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, pl1, apiparameter);
            return Ok(pl1);
        }
    }
}
