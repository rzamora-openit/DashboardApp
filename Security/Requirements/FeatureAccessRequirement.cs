using Microsoft.AspNetCore.Authorization;

namespace OpeniT.PowerbiDashboardApp.Security.Requirements
{
	public class FeatureAccessRequirement : IAuthorizationRequirement
	{
		public readonly string feature;

		public FeatureAccessRequirement(string feature)
		{
			this.feature = feature;
		}
	}
}
