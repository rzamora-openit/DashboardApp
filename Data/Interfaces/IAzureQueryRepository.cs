using Newtonsoft.Json.Linq;
using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Data.Interfaces
{
    public interface IAzureQueryRepository
    {
        Task<bool> AddRole(string name, string description, CancellationToken cancellationToken = default);
        Task<bool> AddToRole(string email, string roleId, CancellationToken cancellationToken = default);
        Task<bool> AddToRoleById(string userId, string roleId, CancellationToken cancellationToken = default);
        Task<bool> DeleteRole(string name, CancellationToken cancellationToken = default);
        Task<bool> DisableRole(string name, CancellationToken cancellationToken = default);
        Task<bool> EnalbeRole(string name, CancellationToken cancellationToken = default);
        Task<Application> GetAppDetails(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<List<AppRoleAssignment>> GetAppRoleAssignments(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<JArray> GetAppRoleAssignmentsAsJObect(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<JObject> GetFullAppDetails(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<List<User>> GetGroupMembers(string groupId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<JArray> GetGroupMembersAsJArray(string groupId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<List<User>> GetGroupMembersByEmail(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<List<Group>> GetGroups(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<JArray> GetGroupsAsJArray(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<GroupsDelta> GetGroupsDelta(string deltaLink = null, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<List<Group>> GetGroupsPaged(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<string> GetRoleAssignmentId(string roleValue, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<string> GetRoleId(string roleValue, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetRoleNamesOfEmail(string email, CancellationToken cancellationToken = default);
        Task<JArray> GetRolesOfEmail(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<User> GetUserByEmail(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<JObject> GetUserByEmailAsJObject(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetUserGroupNamesByEmail(string email, CancellationToken cancellationToken = default);
        Task<JArray> GetUserGroupsByEmail(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<JArray> GetUserGroupsById(string userId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<JArray> GetUserGroupsTransitiveByEmail(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<JArray> GetUserGroupsTransitiveById(string userId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<string> GetUserId(string email, CancellationToken cancellationToken = default);
        Task<User> GetUserManagerByEmail(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<Photo> GetUserPhotoByEmail(string email, CancellationToken cancellationToken = default);
        Task<Photo> GetUserPhotoById(string id, CancellationToken cancellationToken = default);
        Task<List<User>> GetUsers(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<bool> RemoveFromRole(string email, string roleAssignmentId, CancellationToken cancellationToken = default);
        Task<bool> RemoveFromRoleById(string userId, string roleAssignmentId, CancellationToken cancellationToken = default);
        Task<bool> SendMailAsync(string senderEmail, Email message, CancellationToken cancellationToken = default);
    }
}