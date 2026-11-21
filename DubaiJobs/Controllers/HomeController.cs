
using DubaiJobs.Models;
using DubaiJobs.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DubaiJobs.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult dashboard()
        {
            return View();
        }

        public IActionResult ApplyJob()
        {
            return View();
        }

        // Browse Jobs - Public access
        [AllowAnonymous]
        public async Task<IActionResult> BrowsJobs()
        {
            // Retrieve ALL jobs from database
            var jobs = await _context.PostJobs
                .AsNoTracking()
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();

            // Debug information
            _logger.LogInformation(
                "Browse Jobs: {JobCount} jobs retrieved from database",
                jobs.Count);

            ViewBag.TotalJobs = jobs.Count;

            return View(jobs);
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ??
                            HttpContext.TraceIdentifier
            });
        }
    }
}
