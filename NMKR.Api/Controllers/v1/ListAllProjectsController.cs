using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Asp.Versioning;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Api.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]

    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class ListAllProjectsController : ControllerBase
    {
        [HttpGet]
        [MapToApiVersion("1")]
        public IActionResult Get()
        {
            using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {
                var projects = (from a in db.Nftprojects
                    where a.State == "active"
                    select new {a.Id,a.Projectname, a.Description, a.Projecturl}).ToList();
                db.Database.CloseConnection();
                return Ok(projects);
            }
        }
    }
}



