using Microsoft.AspNetCore.Authorization;

namespace OpeniT.PowerbiDashboardApp.Security.Authorization
{
	public class ServiceAccess : AuthorizeAttribute
	{
		public readonly string feature;

		public ServiceAccess(string feature)
		{
			this.feature = feature;
			this.Policy = $"{nameof(ServiceAccess)}-{feature}";
		}
	}
}
