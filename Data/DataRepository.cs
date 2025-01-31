﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OpeniT.PowerbiDashboardApp.Models.Application;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using OpeniT.PowerbiDashboardApp.Models;
using Microsoft.AspNetCore.Identity;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using System.Collections.Generic;
using System.Collections;
using OpeniT.PowerbiDashboardApp.Models.Objects;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;

namespace OpeniT.PowerbiDashboardApp.Data
{
    public class DataRepository : IDataRepository
	{
		private readonly DataContext context;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly IHttpContextAccessor httpContextAccessor;

		public DataRepository(DataContext context,
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			IHttpContextAccessor httpContextAccessor)
		{
			this.context = context;
			this.userManager = userManager;
			this.roleManager = roleManager;
			this.httpContextAccessor = httpContextAccessor;
		}

		public async Task<bool> SaveChangesAsync()
		{
			var userEmail = (httpContextAccessor != null && httpContextAccessor.HttpContext != null
			? httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? httpContextAccessor.HttpContext.User.Identity.Name
			: "Server");

			var entries = this.context.ChangeTracker.Entries();
			foreach (var entry in entries.Where(e =>
				e.Entity.GetType().BaseType == typeof(BaseMonitor)
				&& e.State != EntityState.Detached
				&& e.State != EntityState.Unchanged))
			{
				switch (entry.State)
				{
					case EntityState.Added:
						((BaseMonitor)entry.Entity).AddedDate = DateTime.UtcNow;
						((BaseMonitor)entry.Entity).AddedBy = userEmail;
						break;
					case EntityState.Modified:
						((BaseMonitor)entry.Entity).LastUpdatedDate = DateTime.UtcNow;
						((BaseMonitor)entry.Entity).LastUpdatedBy = userEmail;
						break;
				}
			}

			var results = await this.context.SaveChangesAsync();

			return results > 0;
		}

		#region ApplicationActivity
		public async void AddActivityAsync(ApplicationActivity activity)
		{
			await this.context.Activities.AddAsync(activity);
		}
		#endregion

		#region ApplicationUser
		public async Task<IdentityResult> AddApplicationUserAsAdminAsync(ApplicationUser applicationUser)
		{
			return await this.userManager.CreateAsync(applicationUser);
		}

		public async Task<ApplicationUser> AddApplicationUserInternalAccountAsync(InternalAccount internalAccount)
		{
			var user = new ApplicationUser
			{
				UserName = internalAccount.Email,
				Email = internalAccount.Email,
				InternalAccount = internalAccount
			};

			var result = await this.AddApplicationUserAsAdminAsync(user);
			return result.Succeeded ? user : null;
		}

		public async Task<IdentityResult> UpdateApplicationUser(ApplicationUser applicationUser)
		{
			return await this.userManager.UpdateAsync(applicationUser);
		}

		public async Task<ApplicationUser> GetApplicationUserByEmail(string email)
		{
			return await this.userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
		}

		public async Task<ApplicationUser> GetApplicationUserWithAccountByEmail(string email)
		{
			return await this.userManager.Users.Where(u => u.Email == email)
				.Include(x => x.InternalAccount)
				.FirstOrDefaultAsync();
		}

		public async Task<ApplicationUser> GetApplicationUserWithAccountByUserName(string username)
		{
			return await this.userManager.Users.Where(u => u.NormalizedUserName == username.ToUpper())
				.Include(x => x.InternalAccount)
				.FirstOrDefaultAsync();
		}

		public async Task<ApplicationUser> GetApplicationUserByProviderKey(string providerKey)
		{
			var userLogin = await this.context.UserLogins.FirstOrDefaultAsync(x => x.ProviderKey == providerKey);
			if (userLogin == null) return null;

			return await this.userManager.Users.Where(x => x.Id == userLogin.UserId)
				.Include(x => x.InternalAccount)
				.FirstOrDefaultAsync();
		}

		public async Task<ApplicationUser> GetInternalApplicationUserByEmail(string email)
		{
			return await this.userManager.Users.Where(u => u.Email == email)
				.Include(u => u.InternalAccount)
				.FirstOrDefaultAsync();
		}

		public async Task<ApplicationUser> GetInternalUserByReferenceId(string referenceId)
		{
			return await this.context.Users.Where(u => u.InternalAccount != null && u.InternalAccount.ReferenceId == referenceId)
				.Include(x => x.InternalAccount)
				.FirstOrDefaultAsync();
		}
		#endregion

		#region InternalAccount
		public async Task<InternalAccount> GetInternalAccountByEmail(string email)
		{
			return await this.context.InternalAccounts.FirstOrDefaultAsync(x => x.Email == email);
		}
		#endregion

		#region Roles
		public async Task<IdentityResult> AddToRole(string email, string role)
		{
			var user = await this.GetApplicationUserByEmail(email);
			return await AddToRole(user, role); ;
		}

		public async Task<IdentityResult> AddToRole(ApplicationUser user, string role)
		{
			await EnsureRoleExist(role);
			return await userManager.AddToRoleAsync(user, role);
		}

		public async Task<IdentityResult> AddToRoles(ApplicationUser user, IEnumerable<string> roles)
		{
			await EnsureRolesExist(roles);
			return await userManager.AddToRolesAsync(user, roles);
		}

		public async Task<IdentityResult> SetToRole(string email, string role)
		{
			var user = await this.GetApplicationUserByEmail(email);
			return await SetToRole(user, role); ;
		}

		public async Task<IdentityResult> SetToRole(ApplicationUser user, string role)
		{
			var roles = await this.userManager.GetRolesAsync(user);
			await RemoveFromRoles(user, roles);

			return await AddToRole(user, role);
		}

		public async Task<IdentityResult> SetToRoles(string email, IEnumerable<string> roles)
		{
			var user = await this.GetApplicationUserByEmail(email);
			return await SetToRoles(user, roles); ;
		}

		public async Task<IdentityResult> SetToRoles(ApplicationUser user, IEnumerable<string> roles)
		{
			var userRoles = await this.userManager.GetRolesAsync(user);
			await RemoveFromRoles(user, userRoles);

			return await AddToRoles(user, roles);
		}

		public async Task<IdentityResult> RemoveFromRoles(ApplicationUser user, IEnumerable<string> roles)
		{
			return await userManager.RemoveFromRolesAsync(user, roles);
		}

		public async Task EnsureRoleExist(string role)
		{
			var identityRole = await roleManager.FindByNameAsync(role);
			if (identityRole == null)
			{
				await roleManager.CreateAsync(new IdentityRole(role));
			}
		}

		public async Task EnsureRolesExist(IEnumerable<string> roles)
		{
			foreach (var role in roles)
			{
				var identityRole = await roleManager.FindByNameAsync(role);
				if (identityRole == null)
				{
					await roleManager.CreateAsync(new IdentityRole(role));
				}
			}
		}
		#endregion

		#region PowerbiReferences
		public async Task<IEnumerable<PowerbiReference>> GetPowerbiReferences()
		{
			return await this.context.PowerbiReferences
				.Include(x => x.Sharing.UserShares)
				.Include(x => x.Sharing.GroupShares)
				.ToListAsync();
		}

		public async Task<IEnumerable<PowerbiReference>> GetPowerbiReferencesSharing(string azureId, List<string> groupIds)
		{
			return await this.context.PowerbiReferences
				.Include(x => x.Sharing.UserShares)
				.Include(x => x.Sharing.GroupShares)
				.Where(x => x.Sharing.UserShares.Any(s => s.AzureId == azureId) || x.Sharing.GroupShares.Any(s => groupIds.Contains(s.AzureId)))
				.ToListAsync();
		}

		public async Task<PowerbiReference> GetPowerbiReferenceById(int id)
		{
			return await this.context.PowerbiReferences
				.Include(x => x.Sharing.UserShares)
				.Include(x => x.Sharing.GroupShares)
				.FirstOrDefaultAsync(x => x.Id == id);
		}

		public async Task<PowerbiReference> GetPowerbiReferenceSharingById(int id, string azureId, List<string> groupIds)
		{
			return await this.context.PowerbiReferences
				.Include(x => x.Sharing.UserShares)
				.Include(x => x.Sharing.GroupShares)
				.Where(x => x.Sharing.UserShares.Any(s => s.AzureId == azureId) || x.Sharing.GroupShares.Any(s => groupIds.Contains(s.AzureId)))
				.FirstOrDefaultAsync(x => x.Id == id);
		}

		public async Task<PowerbiReference> GetPowerbiReferenceByGroupIdDatasetId(string groupId, string datasetId)
		{
			return await this.context.PowerbiReferences.FirstOrDefaultAsync(x => x.WorkGroupId == groupId && x.DataSetId == datasetId);
		}

		public async Task<PowerbiReference> GetPowerbiReferenceByGroupIdReportId(string groupId, string reportId)
		{
			return await this.context.PowerbiReferences.FirstOrDefaultAsync(x => x.WorkGroupId == groupId && x.ReportId == reportId);
		}

		public async void AddPowerbiReference(PowerbiReference powerbiReference)
		{
			await this.context.PowerbiReferences.AddAsync(powerbiReference);
		}

		public void UpdatePowerbiReference(PowerbiReference powerbiReference)
		{
			this.context.PowerbiReferences.Update(powerbiReference);
		}

		public void RemovePowerbiReference(PowerbiReference powerbiReference)
		{
			this.context.PowerbiReferences.Remove(powerbiReference);
		}
		#endregion

		#region FeatureAccess		
		public async void AddFeatureAccess(FeatureAccess featureAccess)
		{
			await this.context.FeatureAccesses.AddAsync(featureAccess);
		}
		public void UpdateFeatureAccess(FeatureAccess featureAccess)
		{
			this.context.FeatureAccesses.Update(featureAccess);
		}
		public void RemoveFeatureAccess(FeatureAccess featureAccess)
		{
			this.context.FeatureAccesses.Remove(featureAccess);
		}
		public async Task<IEnumerable<FeatureAccess>> GetFeatureAccessesAsync()
		{
			return await this.context.FeatureAccesses
				.Include(x => x.Accesses)
				.ToListAsync();
		}
		public async Task<FeatureAccess> GetFeatureAccessById(int Id)
		{
			return await this.context.FeatureAccesses.Where(x => x.Id == Id)
				.Include(x => x.Accesses)
				.FirstOrDefaultAsync();
		}
		public async Task<FeatureAccess> GetFeatureAccessByFeatureName(string featureName)
		{
			return await this.context.FeatureAccesses.Where(x => x.FeatureName == featureName)
				.Include(x => x.Accesses)
				.FirstOrDefaultAsync();
		}
		#endregion FeatureAccess

		#region Access		
		public async void AddAccess(Access Access)
		{
			await this.context.Accesses.AddAsync(Access);
		}
		public void UpdateAccess(Access Access)
		{
			this.context.Accesses.Update(Access);
		}
		public void RemoveAccess(Access Access)
		{
			this.context.Accesses.Remove(Access);
		}
		public async Task<IEnumerable<Access>> GetAccessesAsync()
		{
			return await this.context.Accesses.ToListAsync();
		}
		public async Task<IEnumerable<Access>> GetAccessesByPriviledgeInfoAsync(string email = null, IEnumerable<string> groups = null, IEnumerable<string> role = null)
		{
			var q = this.context.Accesses.AsQueryable();
			if (string.IsNullOrEmpty(email))
			{
				q.Where(x => (x.Type == Security.AccessTypes.User && x.Reference == email));
			}
			if (groups != null && groups.Any())
			{
				q.Where(x => (x.Type == Security.AccessTypes.Group && groups.Contains(x.Reference)));
			}
			if (role != null && role.Any())
			{
				q.Where(x => (x.Type == Security.AccessTypes.Role && role.Contains(x.Reference)));
			}
			return await q.ToListAsync();
		}
		public async Task<Access> GetAccessById(int Id)
		{
			return await this.context.Accesses.FirstOrDefaultAsync(x => x.Id == Id);
		}
		public async Task<IEnumerable<string>> GetDistinctGroupIds()
		{
			return await this.context.Accesses
				.Where(x => x.Type == Security.AccessTypes.Group)
				.Select(x => x.AzureId)
				.Distinct()
				.ToListAsync();
		}

		public async Task<IEnumerable<string>> GetDistinctRoleIds()
		{
			return await this.context.Accesses
				.Where(x => x.Type == Security.AccessTypes.Role)
				.Select(x => x.AzureId)
				.Distinct()
				.ToListAsync();
		}

		#endregion Access
	}
}
