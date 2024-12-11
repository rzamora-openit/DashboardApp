using System.Collections.Generic;

namespace OpeniT.PowerbiDashboardApp.Models.Application
{
	public class FeatureAccess
	{
		public int Id { get; set; }

		public string FeatureName { get; set; }

		public ICollection<Access> Accesses { get; set; }
	}
}
