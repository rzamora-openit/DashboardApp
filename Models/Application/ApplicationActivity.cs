using System;

namespace OpeniT.PowerbiDashboardApp.Models.Application
{
	public class ApplicationActivity
	{
		public int Id { get; set; }

		public string SiteVersion { get; set; }

		public DateTime DateExcuted { get; set; }

		public string IdentityName { get; set; }


		public string IpAddress { get; set; }

		public string UserAgent { get; set; }

		public string Method { get; set; }

		public string Path { get; set; }

		public string Referrer { get; set; }

		public string LogType { get; set; }

		public string Controller { get; set; }

		public string Action { get; set; }

		public string RelevantObject { get; set; }

		public string RelevantObjectReference { get; set; }

		public string Status { get; set; }

		public string Log { get; set; }
	}
}
