using Microsoft.AspNetCore.Identity;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using OpeniT.PowerbiDashboardApp.Security;
using OpeniT.PowerbiDashboardApp.Security.Model;
using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Helpers
{
    public class AccessProfileHelper : IAccessProfileHelper
	{
		private readonly IAzureQueryRepository azureQuery;
		private readonly IDataRepository dataRepository;
		private readonly UserManager<ApplicationUser> userManager;

		public AccessProfileHelper(IAzureQueryRepository azureQuery, IDataRepository dataRepository, UserManager<ApplicationUser> userManager)
		{
			this.azureQuery = azureQuery;
			this.dataRepository = dataRepository;
			this.userManager = userManager;
		}

		public async Task<bool> FetchAndStoreInfo(InternalAccount internalAccount, bool force = false)
		{
			var ret = true;
			ret &= await FetchAndStoreGroupInfo(internalAccount, force);
			ret &= await FetchAndStoreRoleInfo(internalAccount, force);
			return ret;
		}

		public async Task<bool> FetchAndStoreGroupInfo(InternalAccount internalAccount, bool force = false)
		{
			try
			{
				if (!AzureStaticStore.Members.ContainsKey(internalAccount.ReferenceId))
				{
					AzureStaticStore.Members.Add(
						internalAccount.ReferenceId,
						new AzureStaticStore.Member()
						{
							Id = internalAccount.ReferenceId,
							DisplayName = internalAccount.DisplayName,
							Mail = internalAccount.Email,
							IsSyncing = false,
							RoleIds = new List<string>()
						}
					);
				}

				var member = AzureStaticStore.Members[internalAccount.ReferenceId];

				if (force ||
					(!member.IsSyncing &&
					(DateTime.Now - member.GroupLastSync).TotalMinutes >= Site.StaticValues.AzureGroupInfoSyncInterval))
				{

					member.IsSyncing = true;

					var queryOptions = new List<QueryOption>();
					queryOptions.Add(new QueryOption() { Key = "$select", Value = "odata.type,id" });

					var jArray = await this.azureQuery.GetUserGroupsTransitiveByEmail(internalAccount.Email, queryOptions);
					var groups = jArray.ToObject<List<Group>>();

					member.GroupIds = groups.Select(x => x.Id).ToList();

					member.GroupLastSync = DateTime.Now;
					member.IsSyncing = false;
				}

				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public async Task<bool> FetchAndStoreRoleInfo(InternalAccount internalAccount, bool force = false)
		{
			try
			{
				if (!AzureStaticStore.Members.ContainsKey(internalAccount.ReferenceId))
				{
					AzureStaticStore.Members.Add(
						internalAccount.ReferenceId,
						new AzureStaticStore.Member()
						{
							Id = internalAccount.ReferenceId,
							DisplayName = internalAccount.DisplayName,
							Mail = internalAccount.Email,
							IsSyncing = false,
							GroupIds = new List<string>(),
							RoleIds = new List<string>()
						}
					);
				}

				var member = AzureStaticStore.Members[internalAccount.ReferenceId];

				if (force ||
					(!member.IsSyncing &&
					(DateTime.Now - member.RoleLastSync).TotalMinutes >= Site.StaticValues.AzureGroupInfoSyncInterval))
				{

					member.IsSyncing = true;

					var queryOptions = new List<QueryOption>();
					queryOptions.Add(new QueryOption() { Key = "$select", Value = "appRoleId" });

					var jArray = await this.azureQuery.GetRolesOfEmail(internalAccount.Email, queryOptions);
					var appRoleAssignments = jArray.ToObject<List<AppRoleAssignment>>();

					member.RoleIds = appRoleAssignments.Select(x => x.AppRoleId).ToList();

					member.RoleLastSync = DateTime.Now;
					member.IsSyncing = false;
				}

				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public async Task<bool> HasPermission(ApplicationUser user, string featureName)
		{
			var profile = new AccessProfile()
			{
				Id = user?.InternalAccount?.ReferenceId,
				Email = user?.Email,
				IsMaster = await userManager.IsInRoleAsync(user, Site.ConstantValues.ItoolsMaster)
			};

			var hasAccess = AccessEvaluator
				.AssertAccessLevel(FeatureAccessHelper
					.GetFeatureAccess(featureName), profile) > Security.AccessLevelFlag.None;
			return hasAccess;
		}

		public async Task<bool> HasPermission(string email, string featureName)
		{
			var user = await dataRepository.GetInternalApplicationUserByEmail(email);
			return await this.HasPermission(user, featureName);
		}

		public async Task<bool> HasPermission(ApplicationUser user, string featureName, Security.AccessLevelFlag flag)
		{
			var profile = new AccessProfile()
			{
				Id = user?.InternalAccount?.ReferenceId,
				Email = user?.Email,
				IsMaster = await userManager.IsInRoleAsync(user, Site.ConstantValues.ItoolsMaster)
			};

			var hasAccess = AccessEvaluator
				.AssertAccessLevel(FeatureAccessHelper
					.GetFeatureAccess(featureName), profile) > flag;
			return hasAccess;
		}

		public async Task<bool> HasPermission(string email, string featureName, Security.AccessLevelFlag flag)
		{
			var user = await dataRepository.GetInternalApplicationUserByEmail(email);
			return await this.HasPermission(user, featureName, flag);
		}
	}
}
