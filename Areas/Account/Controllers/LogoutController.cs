using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using OpeniT.PowerbiDashboardApp.Models.Application;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Areas.Account.Controllers
{
	[Area("Account")]
	[AllowAnonymous]
	public class LogoutController : Controller
	{
		private string ControllerName = nameof(LogoutController);

		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly IApplicationLogger logger;

		public LogoutController(SignInManager<ApplicationUser> signInManager,
			IApplicationLogger logger)
		{
			this.signInManager = signInManager;
			this.logger = logger;
		}

		[Route("[controller]")]
		[Route("[area]/[controller]")]
		public async Task<IActionResult> Index()
		{
			var owner = this.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"{ControllerName}.Index" };
			activity.LogType = Site.ConstantValues.LogNavigation;
			activity.Log = $"Logout: [{owner}]";

			await this.signInManager.SignOutAsync();
			await logger.Log(activity);
			return LocalRedirect("/");
		}
	}
}
