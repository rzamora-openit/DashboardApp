using Microsoft.AspNetCore.Http;
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

namespace OpeniT.PowerbiDashboardApp.Data
{
	public class DataRepository
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
			return await this.context.PowerbiReferences.ToListAsync();
		}

		public async Task<PowerbiReference> GetPowerbiReferenceById(int id)
		{
			return await this.context.PowerbiReferences.FirstOrDefaultAsync(x => x.Id == id);
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
	}
}
