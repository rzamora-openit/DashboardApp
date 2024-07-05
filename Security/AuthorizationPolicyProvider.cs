using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OpeniT.PowerbiDashboardApp.Security.Authorization;
using OpeniT.PowerbiDashboardApp.Security.Requirements;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Security
{
	internal class AuthorizationPolicyProvider : IAuthorizationPolicyProvider
	{
		public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }
		public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
		{
			// ASP.NET Core only uses one authorization policy provider, so if the custom implementation
			// doesn't handle all policies (including default policies, etc.) it should fall back to an
			// alternate provider.
			//
			// In this sample, a default authorization policy provider (constructed with options from the 
			// dependency injection container) is used if this custom provider isn't able to handle a given
			// policy name.
			//
			// If a custom policy provider is able to handle all expected policy names then, of course, this
			// fallback pattern is unnecessary.
			FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
		}

		public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

		public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

		public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
		{
			string[] policySignature = policyName.Split("-");

			if (policySignature.Length == 2 && policySignature[0].CompareTo(nameof(ServiceAccess)) == 0)
			{
				var policy = new AuthorizationPolicyBuilder();
				policy.AddRequirements(new FeatureAccessRequirement(policySignature[1]));
				return Task.FromResult(policy.Build());
			}

			return FallbackPolicyProvider.GetPolicyAsync(policyName);
		}
	}
}
