using System;

namespace OpeniT.PowerbiDashboardApp.Models.Application
{
	public class Access
	{
		public int Id { get; set; }

		public string Type { get; set; }

		//Mail for Groups, Name for Roles
		public string Reference { get; set; }

		public string AzureId { get; set; }

		public Security.AccessLevelFlag Level { get; set; }

		public bool Limiter { get; set; }

		public string CreatedByEmail { get; set; }

		public DateTime CreatedDate { get; set; }

		public string UpdatedByEmail { get; set; }

		public DateTime UpdatedDate { get; set; }
	}
}
