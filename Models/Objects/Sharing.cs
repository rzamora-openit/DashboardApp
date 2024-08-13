using System.Collections;
using System.Collections.Generic;

namespace OpeniT.PowerbiDashboardApp.Models.Objects
{
	public class Sharing
	{
        public int Id { get; set; }
		public ICollection<UserShare> UserShares { get; set; } = new List<UserShare>();
		public ICollection<GroupShare> GroupShares { get; set; } = new List<GroupShare>();
	}
}
