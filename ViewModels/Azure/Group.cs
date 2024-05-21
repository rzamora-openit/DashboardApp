using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpeniT.PowerbiDashboardApp.ViewModels.Azure
{
	public class Group
	{
		public string Id { get; set; }

		public string DisplayName { get; set; }

		public string Mail { get; set; }

		public List<GroupMember> TransitiveMembers { get; set; } = new List<GroupMember>();

		public List<GroupMember> Members { get; set; } = new List<GroupMember>();

		[JsonProperty("members@delta")]
		public List<GroupMember> DeltaMembers { get; set; } = new List<GroupMember>();

		[JsonProperty("@removed")]
		public Status Removed { get; set; }

		public class Status
		{
			public string Reason { get; set; }
		}
	}
}
