using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Data;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using OpeniT.PowerbiDashboardApp.Models.Application;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace OpeniT.PowerbiDashboardApp.Controllers.Web
{
	[AllowAnonymous]
	public class NotFoundController : Controller
	{
		private string ControllerName = nameof(NotFoundController);

		private readonly ApplicationLogger logger;
		private readonly DataRepository dataRepository;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly SignInManager<ApplicationUser> signInManager;

		public NotFoundController(ApplicationLogger logger,
			DataRepository dataRepository,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager)
		{
			this.logger = logger;
			this.dataRepository = dataRepository;
			this.userManager = userManager;
			this.signInManager = signInManager;
		}

		[AllowAnonymous]
		public async Task<IActionResult> Index(string returnUrl)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Index" };
			try
			{
				if (this.User.Identity.IsAuthenticated)
				{
					var identityName = this.User.Identity.Name;
					var claimsEmail = this.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
					var claimsProvider = this.User.FindFirst(System.Security.Claims.ClaimTypes.AuthenticationMethod)?.Value;
					var claimsRoles = string.Join(", ", this.User.Claims.Where(x => x.Type == System.Security.Claims.ClaimTypes.Role).Select(x => x.Value));

					var applicationUser = await this.dataRepository.GetApplicationUserWithAccountByUserName(identityName);

					var log = $"Failed attempt to access [{returnUrl}] by authenticated User: ";
					log += $" identityname [{identityName}],";
					log += $" claimsEmail [{claimsEmail}],";
					log += $" claimsProvider [{claimsProvider}],";
					log += $" claimsRoles [{claimsRoles}],";
					log += $" applicationUserId [{applicationUser.Id}],";

					await logger.LogInvalidAccess(activity, log);
				}
				else
				{
					await logger.LogInvalidAccess(activity, $"Failed attempt to access [{returnUrl}] by unauthenticated User");
				}
			}
			catch (Exception ex)
			{
				await logger.LogException(activity, ex);
			}

			await logger.LogNavigation(activity: activity, log: "Content Access");
			return View();
		}
	}
}
