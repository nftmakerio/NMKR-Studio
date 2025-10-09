using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NMKR.BackgroundService.Pages
{
    public class IndexModel : PageModel
    {
        private readonly EasynftprojectsContext _db = new(GlobalFunctions.optionsBuilder.Options);
        public List<Backgroundtasklogview> log = new();
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

       
        public async Task OnGetAsync()
        {
            log = await (from s in _db.Backgroundtasklogviews
                         select s).ToListAsync();
        }
    }
}
