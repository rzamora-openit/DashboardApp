using OpeniT.PowerbiDashboardApp.Models.Application;
using OpeniT.PowerbiDashboardApp.Security.Model;
using System.Collections.Generic;
using System.Linq;

namespace OpeniT.PowerbiDashboardApp.Security
{
	public class AccessEvaluator
	{
		public static AccessLevelFlag AssertAccessLevel(IEnumerable<Access> accesses, AccessProfile accessProfile)
		{
			if (accesses == null || accessProfile == null)
			{
				return AccessLevelFlag.None;
			}

			if (accessProfile.IsMaster)
			{
				return AccessLevelFlag.Master;
			}

			var member = Helpers.AzureStaticStore.GetMemberById(accessProfile.Id);

			var groupIds = member.GroupIds;
			var rolesIds = member.RoleIds;

			var granted = accesses
					.Where(x => !x.Limiter)
					.Where(x =>
						(x.Type == "All") ||
						(x.Type == "User" && x.AzureId == accessProfile.Id) ||
						(x.Type == "Group" && groupIds.Contains(x.AzureId)) ||
						(x.Type == "Role" && rolesIds.Contains(x.AzureId))
					);

			if (!granted.Any())
			{
				return AccessLevelFlag.None;
			}
			var highestGrant = granted.Max(x => x.Level);

			var limited = accesses
					.Where(x => x.Limiter)
					.Where(x =>
						(x.Type == "All") ||
						(x.Type == "User" && x.AzureId == accessProfile.Id) ||
						(x.Type == "Group" && groupIds.Contains(x.AzureId)) ||
						(x.Type == "Role" && rolesIds.Contains(x.AzureId))
					);

			if (!limited.Any())
			{
				return highestGrant;
			}
			var lowestLimit = limited.Min(x => x.Level);

			var highestAccess = highestGrant < lowestLimit ? highestGrant : lowestLimit;
			return highestAccess;
		}
	}
}
