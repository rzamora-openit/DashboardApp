using System.Collections.Generic;

namespace OpeniT.PowerbiDashboardApp.ViewModels.NotInModels
{
	public class ServiceGroupViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Index { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
		public string Uri { get; set; }

		public List<ServiceViewModel> Services { get; set; }
	}
}
