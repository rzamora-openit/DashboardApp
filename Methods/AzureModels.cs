using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using System.Collections.Generic;
using System.Linq;

namespace OpeniT.PowerbiDashboardApp.Methods
{
	public class AzureModels
	{
		public static List<Group> ParseTransitiveMembers(List<Group> groups)
		{
			foreach (var group in groups)
			{
				GetTransitiveMembers(group, groups);
			}
			return groups;
		}

		public static List<GroupMember> GetTransitiveMembers(Group group, List<Group> groups)
		{
			if (!group.TransitiveMembers.Any())
			{
				foreach (var member in group.Members)
				{
					if (member.Type == "#microsoft.graph.group")
					{
						var memberAsGroup = groups.SingleOrDefault(x => x.Id == member.Id);
						EnsureAddUniqueTransitiveMembers(group, GetTransitiveMembers(memberAsGroup, groups));
					}
					else if (member.Type == "#microsoft.graph.user")
					{
						EnsureAddUniqueTransitiveMember(group, member);
					}
				}
			}
			return group.TransitiveMembers;
		}

		public static void EnsureAddUniqueTransitiveMembers(Group group, List<GroupMember> members)
		{
			foreach (var member in members)
			{
				EnsureAddUniqueTransitiveMember(group, member);
			}
		}

		public static void EnsureAddUniqueTransitiveMember(Group group, GroupMember member)
		{
			if (!group.TransitiveMembers.Any(x => x.Id == member.Id))
			{
				group.TransitiveMembers.Add(member);
			}
		}
	}
}
