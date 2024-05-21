using System.Collections.Generic;

namespace OpeniT.PowerbiDashboardApp.ViewModels.Azure
{
	public class AppRole
	{
		public ICollection<string> AllowedMemberTypes { get; set; }

		public string Description { get; set; }

		public string DisplayName { get; set; }

		public string Id { get; set; }

		public bool IsEnabled { get; set; } = true;

		public string Origin { get; set; } = "Application";

		public string Value { get; set; }
	}
}
