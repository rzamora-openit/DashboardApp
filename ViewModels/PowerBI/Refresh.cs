using System;

namespace OpeniT.PowerbiDashboardApp.ViewModels.PowerBI
{
	public class Refresh
	{
		public string RefreshType { get; set; }

		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }

		public string Status { get; set; }

		public string ServiceExceptionJson { get; set; }

		public string RequestId { get; set; }
	}
}
