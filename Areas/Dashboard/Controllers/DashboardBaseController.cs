using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpeniT.PowerbiDashboardApp.Data;
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

			var dataRepository = (DataRepository)context.HttpContext.RequestServices.GetService(typeof(DataRepository));

			var email = User.FindFirst(ClaimTypes.Email)?.Value;

			var account = await dataRepository.GetInternalApplicationUserByEmail(email);
			this.ViewData["Account"] = account;
		}
	}
}
