using BWBugTracker.Extensions;
using BWBugTracker.Models;
using BWBugTracker.Models.Enums;
using BWBugTracker.Models.ViewModels;
using BWBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BWBugTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBTCompanyService _companyService;
        private readonly IBTProjectService _projectService;

        public HomeController(ILogger<HomeController> logger, IBTCompanyService companyService, IBTProjectService projectService)
        {
            _logger = logger;
            _companyService = companyService;
            _projectService = projectService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> PortoIndex()
        {
            DashboardViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();

            model.Company = await _companyService.GetCompanyInfoAsync(companyId);
            model.Projects = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                   .Where(p => p.Archived == false)
                                                   .ToList();

            model.Tickets = model.Projects
                                 .SelectMany(p => p.Tickets)
                                 .Where(t => t.Archived == false)
                                 .ToList();

            model.Members = model.Company.Members.ToList();

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Landing()
        {
            return View();
        }
        
        public IActionResult LandingTest()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();

            model.Company = await _companyService.GetCompanyInfoAsync(companyId);
            model.Projects = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId)).Where(p => p.Archived == false).ToList();
            model.Tickets = model.Projects.SelectMany(p => p.Tickets).Where(t => t.Archived == false).ToList();
            model.Members = model.Company.Members.ToList();

            return View(model);
        }

		[HttpPost]
		public async Task<JsonResult> ProjectTicketsChart()
		{
			int companyId = User.Identity.GetCompanyId();

			List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

			List<object> chartData = new();
			chartData.Add(new object[] { "ProjectName", "TicketCount" });

			foreach (Project prj in projects)
			{
				chartData.Add(new object[] { prj.Name, prj.Tickets.Count() });
			}

			return Json(chartData);
		}

		[HttpPost]
		public async Task<JsonResult> ProjectPriorityChart()
		{
			int companyId = User.Identity.GetCompanyId();

			List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

			List<object> chartData = new();
			chartData.Add(new object[] { "Priority", "Count" });


			foreach (string priority in Enum.GetNames(typeof(BTProjectPriorities)))
			{
				int priorityCount = (await _projectService.GetAllProjectsByPriorityAsync(companyId, priority)).Count();
				chartData.Add(new object[] { priority, priorityCount });
			}

			return Json(chartData);
		}
	}
}
