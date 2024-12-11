using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Application;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Areas.Account.Controllers
{
	[Area("Account")]
	[AllowAnonymous]
	public class AccessDenied : Controller
	{
		private string ControllerName = nameof(AccessDenied);

		private readonly IApplicationLogger logger;
		private readonly IDataRepository dataRepository;

		public AccessDenied(IApplicationLogger logger,
			IDataRepository dataRepository)
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
