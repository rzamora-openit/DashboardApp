using Microsoft.AspNetCore.Identity;

namespace OpeniT.PowerbiDashboardApp.Models.Accounts
{
	public class ApplicationUser : IdentityUser
	{
		public InternalAccount InternalAccount { get; set; }
	}
}
