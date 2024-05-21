using System.Collections.Generic;

namespace OpeniT.PowerbiDashboardApp.ViewModels.Azure
{
	public class GroupsDelta
	{
		public string DeltaLink { get; set; }

		public List<Group> Groups { get; set; } = new List<Group>();
	}
}
