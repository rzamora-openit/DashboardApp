using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using OpeniT.PowerbiDashboardApp.Security.Model;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Areas.Dashboard.Controllers
{
	[Authorize]
	public class DashboardBaseController : Controller
	{
		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			await base.OnActionExecutionAsync(context, next);

			var dataRepository = (IDataRepository)context.HttpContext.RequestServices.GetService(typeof(IDataRepository));
			var accessProfileHelper = (IAccessProfileHelper)context.HttpContext.RequestServices.GetService(typeof(IAccessProfileHelper));
			var userManager = (UserManager<ApplicationUser>)context.HttpContext.RequestServices.GetService(typeof(UserManager<ApplicationUser>));

			var email = User.FindFirst(ClaimTypes.Email)?.Value;
			var user = await dataRepository.GetInternalApplicationUserByEmail(email);
			if (user?.InternalAccount != null)
			{
				await accessProfileHelper.FetchAndStoreInfo(user.InternalAccount);
			}

			var account = await dataRepository.GetInternalApplicationUserByEmail(email);
			this.ViewData["Account"] = account;

			var profile = new AccessProfile()
			{
				Id = user?.InternalAccount?.ReferenceId,
				Email = user?.Email,
				IsMaster = await userManager.IsInRoleAsync(user, Site.ConstantValues.ItoolsMaster)
			};
			this.ViewData["Profile"] = profile;
		}
	}
}
