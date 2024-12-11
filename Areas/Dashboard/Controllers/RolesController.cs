using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Application;
using OpeniT.PowerbiDashboardApp.Security.Authorization;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Areas.Dashboard.Controllers
{
	[Area("Dashboard")]
	[ServiceAccess(Site.Services.FeatureNames.Roles)]
	public class RolesController : DashboardBaseController
	{
		private string ControllerName = nameof(RolesController);

		private readonly IApplicationLogger logger;

		public RolesController(IApplicationLogger logger)
		{
			this.logger = logger;
		}

		public async Task<IActionResult> Index()
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Index" };

			await logger.LogNavigation(activity: activity, log: "Content Access");
			this.ViewData[ControllerName] = true;
			return View();
		}
	}
}
