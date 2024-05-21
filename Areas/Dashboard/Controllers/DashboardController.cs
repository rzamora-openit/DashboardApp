using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Data;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Models.Application;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Areas.Dashboard.Controllers
{
	[Area("Dashboard")]
	public class DashboardController : DashboardBaseController
	{
		private string ControllerName = nameof(DashboardController);

		private readonly ApplicationLogger logger;
		private readonly DataRepository dataRepository;

		public DashboardController(ApplicationLogger logger,
			DataRepository dataRepository)
		{
			this.logger = logger;
			this.dataRepository = dataRepository;
		}

		public async Task<IActionResult> Index()
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"{ControllerName}.Index" };

			await logger.LogNavigation(activity: activity, log: "Content Access");
			this.ViewData[ControllerName] = true;
			return View();
		}
	}
}
