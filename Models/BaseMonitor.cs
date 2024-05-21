using System;

namespace OpeniT.PowerbiDashboardApp.Models
{
	public class BaseMonitor
	{
		public int Id { get; set; }

		public string AddedBy { get; set; }
		public DateTime AddedDate { get; set; }

		public string LastUpdatedBy { get; set; }
		public DateTime? LastUpdatedDate { get; set; }
	}
}
