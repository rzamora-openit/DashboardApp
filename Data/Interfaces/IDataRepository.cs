using Microsoft.AspNetCore.Identity;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using OpeniT.PowerbiDashboardApp.Models.Application;
using OpeniT.PowerbiDashboardApp.Models.Objects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Data.Interfaces
{
    public interface IDataRepository
    {
        void AddActivityAsync(ApplicationActivity activity);
        Task<IdentityResult> AddApplicationUserAsAdminAsync(ApplicationUser applicationUser);
        Task<ApplicationUser> AddApplicationUserInternalAccountAsync(InternalAccount internalAccount);
        void AddPowerbiReference(PowerbiReference powerbiReference);
        Task<IdentityResult> AddToRole(ApplicationUser user, string role);
        Task<IdentityResult> AddToRole(string email, string role);
        Task<IdentityResult> AddToRoles(ApplicationUser user, IEnumerable<string> roles);
        Task EnsureRoleExist(string role);
        Task EnsureRolesExist(IEnumerable<string> roles);
        Task<ApplicationUser> GetApplicationUserByEmail(string email);
        Task<ApplicationUser> GetApplicationUserByProviderKey(string providerKey);
        Task<ApplicationUser> GetApplicationUserWithAccountByEmail(string email);
        Task<ApplicationUser> GetApplicationUserWithAccountByUserName(string username);
        Task<InternalAccount> GetInternalAccountByEmail(string email);
        Task<ApplicationUser> GetInternalApplicationUserByEmail(string email);
        Task<ApplicationUser> GetInternalUserByReferenceId(string referenceId);
        Task<PowerbiReference> GetPowerbiReferenceByGroupIdDatasetId(string groupId, string datasetId);
        Task<PowerbiReference> GetPowerbiReferenceByGroupIdReportId(string groupId, string reportId);
        Task<PowerbiReference> GetPowerbiReferenceById(int id);
        Task<IEnumerable<PowerbiReference>> GetPowerbiReferences();
        Task<IdentityResult> RemoveFromRoles(ApplicationUser user, IEnumerable<string> roles);
        void RemovePowerbiReference(PowerbiReference powerbiReference);
        Task<bool> SaveChangesAsync();
        Task<IdentityResult> SetToRole(ApplicationUser user, string role);
        Task<IdentityResult> SetToRole(string email, string role);
        Task<IdentityResult> SetToRoles(ApplicationUser user, IEnumerable<string> roles);
        Task<IdentityResult> SetToRoles(string email, IEnumerable<string> roles);
        Task<IdentityResult> UpdateApplicationUser(ApplicationUser applicationUser);
        void UpdatePowerbiReference(PowerbiReference powerbiReference);

		#region FeatureAccess
		void AddFeatureAccess(FeatureAccess featureAccess);

		void UpdateFeatureAccess(FeatureAccess featureAccess);

		void RemoveFeatureAccess(FeatureAccess featureAccess);

		Task<IEnumerable<FeatureAccess>> GetFeatureAccessesAsync();

		Task<FeatureAccess> GetFeatureAccessById(int Id);

		Task<FeatureAccess> GetFeatureAccessByFeatureName(string featureName);
		#endregion

		#region Access
		void AddAccess(Access Access);

		void UpdateAccess(Access Access);

		void RemoveAccess(Access Access);

		Task<IEnumerable<Access>> GetAccessesAsync();

		Task<IEnumerable<Access>> GetAccessesByPriviledgeInfoAsync(string email = null, IEnumerable<string> groups = null, IEnumerable<string> role = null);

		Task<Access> GetAccessById(int Id);

		Task<IEnumerable<string>> GetDistinctGroupIds();

		Task<IEnumerable<string>> GetDistinctRoleIds();
		#endregion
	}
}