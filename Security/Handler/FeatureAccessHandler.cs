using Microsoft.AspNetCore.Authorization;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Security.Model;
using OpeniT.PowerbiDashboardApp.Security.Requirements;
using OpeniT.PowerbiDashboardApp.Site;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Security.Handler
{
	public class FeatureAccessHandler : AuthorizationHandler<FeatureAccessRequirement>
	{
		private readonly IDataRepository dataRepository;

		public FeatureAccessHandler(IDataRepository dataRepository)
		{
			this.dataRepository = dataRepository;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FeatureAccessRequirement requirement)
		{
			if (Site.StaticValues.EnableGlobalInternalAccess)
			{
				if (context.User.Claims.Where(x => x.Type == ClaimTypes.Role).Any() &&
					!context.User.IsInRole(Site.ConstantValues.ExternalUser))
				{
					context.Succeed(requirement);
				}
			}
			else
			{
				var email = context.User.FindFirstValue(ClaimTypes.Email) ?? context.User.Identity.Name;
				var member = AzureStaticStore.GetMemberByEmail(email);
				var roles = context.User.FindAll(ClaimTypes.Role).Select(x => x.Value);

				var userTask = dataRepository.GetInternalApplicationUserByEmail(email);
				userTask.Wait();
				var user = userTask.Result;

				var accessProfile = new AccessProfile()
				{
					Id = user?.InternalAccount?.ReferenceId,
					Email = user?.Email,
					IsMaster = roles.Contains(Site.ConstantValues.ItoolsMaster)
				};

				if (Services.ExemptServiceFeatureList.Any(f => f == requirement.feature))
				{
					context.Succeed(requirement);
					return Task.CompletedTask;
				}

				if (AccessEvaluator.AssertAccessLevel(FeatureAccessHelper.GetFeatureAccess(requirement.feature), accessProfile) > Security.AccessLevelFlag.None)
				{
					context.Succeed(requirement);
					return Task.CompletedTask;
				}
			}

			return Task.CompletedTask;
		}
	}
}
