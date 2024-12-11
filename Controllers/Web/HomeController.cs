using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Areas.Dashboard.Controllers;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using OpeniT.PowerbiDashboardApp.Models.Application;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Controllers.Web
{
	public class HomeController : DashboardBaseController
	{
		private string ControllerName = nameof(HomeController);
		private readonly IApplicationLogger logger;
		private readonly SignInManager<ApplicationUser> signInManager;

		public HomeController(IApplicationLogger logger,
			SignInManager<ApplicationUser> signInManager)
		{
			this.logger = logger;
			this.signInManager = signInManager;
		}

		public async Task<IActionResult> Index()
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"{ControllerName}.Index" };

			await logger.LogNavigation(activity: activity, log: "Content Access");
			this.ViewData[ControllerName] = true;
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
