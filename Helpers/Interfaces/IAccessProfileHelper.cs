using OpeniT.PowerbiDashboardApp.Models.Accounts;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Helpers.Interfaces
{
    public interface IAccessProfileHelper
    {
        Task<bool> FetchAndStoreGroupInfo(InternalAccount internalAccount, bool force = false);
        Task<bool> FetchAndStoreInfo(InternalAccount internalAccount, bool force = false);
        Task<bool> FetchAndStoreRoleInfo(InternalAccount internalAccount, bool force = false);
        Task<bool> HasPermission(ApplicationUser user, string featureName);
        Task<bool> HasPermission(string email, string featureName);
		Task<bool> HasPermission(ApplicationUser user, string featureName, Security.AccessLevelFlag flag);
		Task<bool> HasPermission(string email, string featureName, Security.AccessLevelFlag flag);
	}
}