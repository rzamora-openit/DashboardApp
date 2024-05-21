using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;

namespace OpeniT.PowerbiDashboardApp.Security
{
	public class Assertions
	{
		public static readonly Func<AuthorizationHandlerContext, bool> IsInternal = context => context.User.FindAll(ClaimTypes.Role).Any() && !context.User.IsInRole(Site.ConstantValues.ExternalUser);
	}
}
