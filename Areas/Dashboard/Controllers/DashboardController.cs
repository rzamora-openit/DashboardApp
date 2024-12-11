using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Application;
using OpeniT.PowerbiDashboardApp.Security.Authorization;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Areas.Dashboard.Controllers
{
	[Area("Dashboard")]
	[ServiceAccess(Site.Services.FeatureNames.Dashboard)]
	public class DashboardController : DashboardBaseController
	{
		private string ControllerName = nameof(DashboardController);

		private readonly IApplicationLogger logger;
		private readonly IDataRepository dataRepository;

		public DashboardController(IApplicationLogger logger,
			IDataRepository dataRepository)
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
