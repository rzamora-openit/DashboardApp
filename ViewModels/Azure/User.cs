using Newtonsoft.Json;

namespace OpeniT.PowerbiDashboardApp.ViewModels.Azure
{
	public class User
	{
		[JsonProperty("@odata.type")]
		public string Type { set; get; }

		public string Id { get; set; }

		public bool AccountEnabled { get; set; }

		public string Mail { get; set; }

		public string CompanyName { get; set; }

		public string DisplayName { get; set; }

		public string Department { get; set; }

		public string GivenName { get; set; }

		public string JobTitle { get; set; }

		public string PhysicalDeliveryOfficeName { get; set; }

		public string Surname { get; set; }

		public string UserPrincipalName { get; set; }

		public Photo Photo { get; set; }
	}
}
