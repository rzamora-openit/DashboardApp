using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Data;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Models.Application;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Areas.Account.Controllers
{
	[Area("Account")]
	[AllowAnonymous]
	public class AccessDenied : Controller
	{
		private string ControllerName = nameof(AccessDenied);

		private readonly ApplicationLogger logger;
		private readonly DataRepository dataRepository;

		public AccessDenied(ApplicationLogger logger,
			DataRepository dataRepository)
		{
			this.logger = logger;
			this.dataRepository = dataRepository;
		}

		[Route("[controller]")]
		[Route("[area]/[controller]")]
		public async Task<IActionResult> Index(string returnUrl)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = "Index" };

			await logger.LogInvalidAccess(activity: activity, log: $"Rerouted Invalid Access from Url [{returnUrl}]");
			return View();
		}
	}
}
