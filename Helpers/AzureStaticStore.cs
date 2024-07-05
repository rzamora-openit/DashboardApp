using System.Collections.Generic;
using System;
using System.Linq;

namespace OpeniT.PowerbiDashboardApp.Helpers
{
	public class AzureStaticStore
	{
		public static SortedList<string, Member> Members = new SortedList<string, Member>();

		public static List<string> GetGroupsById(string id)
		{

			return !string.IsNullOrEmpty(id) && Members.ContainsKey(id) ? Members[id].GroupIds : new List<string>();
		}

		public static List<string> GetRolesById(string id)
		{
			return !string.IsNullOrEmpty(id) && Members.ContainsKey(id) ? Members[id].RoleIds : new List<string>();
		}

		public static Member GetMemberById(string id)
		{
			return !string.IsNullOrEmpty(id) && Members.ContainsKey(id) ? Members[id] : new Member();
		}

		public static Member GetMemberByEmail(string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				return null;
			}

			return Members.Values.FirstOrDefault(x => x.Mail == email);
		}

		#region store classes
		public class Member
		{
			public string Id
			{
				get;
				set;
			}

			public string DisplayName { get; set; }

			public string Mail { get; set; }

			public DateTime GroupLastSync { get; set; }
			public DateTime RoleLastSync { get; set; }

			public List<string> GroupIds = new List<string>();

			public List<string> RoleIds = new List<string>();

			public bool IsSyncing { get; set; } = false;
		}
		#endregion store classes
	}
}
