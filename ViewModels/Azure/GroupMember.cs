using Newtonsoft.Json;

namespace OpeniT.PowerbiDashboardApp.ViewModels.Azure
{
	public class GroupMember
	{
		[JsonProperty("@odata.type")]
		public string Type { set; get; }

		public string Id { get; set; }

		public string Mail { get; set; }

		public string DisplayName { get; set; }

		[JsonProperty("@removed")]
		public Status Removed { get; set; }

		public class Status
		{
			public string Reason { get; set; }
		}
	}
}
