using System.Collections.Generic;

namespace OpeniT.PowerbiDashboardApp.ViewModels.Azure
{
	public class Application
	{
		public string CreatedDateTime { get; set; }

		public string DisplayName { get; set; }

		public string PublisherDomain { get; set; }

		public ICollection<AppRole> AppRoles { get; set; } = new List<AppRole>();
	}
}
