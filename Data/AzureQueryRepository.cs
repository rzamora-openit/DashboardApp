using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Data
{
	public class AzureQueryRepository : IAzureQueryRepository
	{
		private readonly IAzureQueryHelper azureQueryHelper;
		private readonly IConfiguration config;

		private readonly string apiVersion;
		private readonly string applicationId;
		private readonly string tenantName;
		private readonly string tenantId;
		private readonly string authString;
		private readonly string clientId;
		private readonly string resourceId;
		private readonly string clientSecret;
		private readonly string graphUri;

		public AzureQueryRepository(IAzureQueryHelper azureQueryHelper,
			IConfiguration config)
		{
			this.azureQueryHelper = azureQueryHelper;
			this.config = config;

			this.apiVersion = config["Microsoft:GraphApiVersion"];
			this.tenantName = config["Microsoft:TenantName"];
			this.tenantId = config["Microsoft:TenantId"];
			this.authString = config["Microsoft:Authority"];
			this.clientId = config["Microsoft:ClientId"];
			this.resourceId = config["Microsoft:ResourceId"];
			this.clientSecret = config["Microsoft:ClientSecret"];
			this.graphUri = config["Microsoft:GraphUri"];
			this.applicationId = config["Microsoft:ApplicationId"];
		}

		private ICollection<QueryOption> EnsureAddFilterForResource(ICollection<QueryOption> options)
		{
			if (options == null)
			{
				options = new List<QueryOption>();
			}

			//$count query is required for 'eq' in $filter query
			if (options.Any(o => o.Key.Equals("$count")))
			{
				var filterOptions = options.First(o => o.Key.Equals("$count"));
				filterOptions.Value = "true";
			}
			else
			{
				options.Add(new QueryOption() { Key = "$count", Value = "true" });
			}

			//ensure Roles listed is only from this application's resourceId
			if (options.Any(o => o.Key.Equals("$filter")))
			{
				var filterOptions = options.First(o => o.Key.Equals("$filter"));
				filterOptions.Value = filterOptions.Value + $" and resourceId eq {resourceId}";
			}
			else
			{
				options.Add(new QueryOption() { Key = "$filter", Value = $"resourceId eq {resourceId}" });
			}

			return options;
		}

		private ICollection<QueryOption> EnsureAddFilterForGroups(ICollection<QueryOption> options)
		{
			if (options == null)
			{
				options = new List<QueryOption>();
			}

			//$count query is required for 'eq' in $filter query
			if (options.Any(o => o.Key.Equals("$count")))
			{
				var filterOptions = options.First(o => o.Key.Equals("$count"));
				filterOptions.Value = "true";
			}
			else
			{
				options.Add(new QueryOption() { Key = "$count", Value = "true" });
			}

			//ensure Roles listed is only from this application's resourceId
			if (options.Any(o => o.Key.Equals("$filter")))
			{
				var filterOptions = options.First(o => o.Key.Equals("$filter"));
				filterOptions.Value = filterOptions.Value + $" and mailEnabled eq true";
			}
			else
			{
				options.Add(new QueryOption() { Key = "$filter", Value = $"mailEnabled eq true" });
			}

			return options;
		}

		private ICollection<QueryOption> EnsureAddExpandTransitiveMembers(ICollection<QueryOption> options)
		{
			if (options == null)
			{
				options = new List<QueryOption>();
			}

			//ensure Roles listed is only from this application's resourceId
			if (options.Any(o => o.Key.Equals("$expand")))
			{
				var filterOptions = options.First(o => o.Key.Equals("$expand"));
				filterOptions.Value = "transitiveMembers";
			}
			else
			{
				options.Add(new QueryOption() { Key = "$expand", Value = $"transitiveMembers" });
			}

			return options;
		}

		#region user
		public async Task<List<User>> GetUsers(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = "users";
			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			var jArray = JObject.Parse(content).Value<JArray>("value");
			var users = jArray.ToObject<List<User>>();
			return users;
		}

		public async Task<User> GetUserByEmail(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"users/{email}";

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			return JsonConvert.DeserializeObject<User>(content);

		}

		public async Task<JObject> GetUserByEmailAsJObject(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"users/{email}";

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			return JObject.Parse(content);
		}

		public async Task<Photo> GetUserPhotoByEmail(string email, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"users/{email}/photo/$value";

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			Photo photo = new Photo();
			photo.Content = await response.Content.ReadAsByteArrayAsync();
			photo.ContentType = response.Content.Headers.ContentType.MediaType;
			return photo;
		}

		public async Task<Photo> GetUserPhotoById(string id, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"users/{id}/photo/$value";

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			Photo photo = new Photo();
			photo.Content = await response.Content.ReadAsByteArrayAsync();
			photo.ContentType = response.Content.Headers.ContentType.MediaType;
			return photo;
		}

		public async Task<User> GetUserManagerByEmail(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"users/{email}/manager";

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			return JsonConvert.DeserializeObject<User>(content);
		}

		public async Task<string> GetUserId(string email, CancellationToken cancellationToken = default(CancellationToken))
		{
			List<QueryOption> options = new List<QueryOption>()
			{
				new QueryOption(){ Key="$count", Value = "true"},
				new QueryOption(){ Key="$select", Value = "id"}
			};
			var user = await this.GetUserByEmail(email, options, cancellationToken);

			return user.Id;
		}

		public async Task<JArray> GetUserGroupsByEmail(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var userId = await GetUserId(email);

			return await GetUserGroupsById(userId, options, cancellationToken);
		}

		public async Task<IEnumerable<string>> GetUserGroupNamesByEmail(string email, CancellationToken cancellationToken = default(CancellationToken))
		{
			var userId = await GetUserId(email);

			ICollection<QueryOption> options = new List<QueryOption>();
			options.Add(new QueryOption() { Key = "$select", Value = "mail" });

			var jArray = await GetUserGroupsById(userId, options, cancellationToken);
			return jArray.Select(x => jArray.Value<string>("mail"));
		}

		public async Task<JArray> GetUserGroupsById(string userId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"users/{userId}/memberOf";

			options = EnsureAddFilterForGroups(options);

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			var jArray = JObject.Parse(content).Value<JArray>("value");
			return jArray;
		}

		public async Task<JArray> GetUserGroupsTransitiveByEmail(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var userId = await GetUserId(email);

			return await GetUserGroupsTransitiveById(userId, options, cancellationToken);
		}

		public async Task<JArray> GetUserGroupsTransitiveById(string userId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"users/{userId}/transitiveMemberOf";

			options = EnsureAddFilterForGroups(options);

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			var jArray = JObject.Parse(content).Value<JArray>("value");
			return jArray;
		}
		#endregion user

		#region group
		public async Task<List<Group>> GetGroups(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"groups/";

			options = EnsureAddFilterForGroups(options);

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			var jArray = JObject.Parse(content).Value<JArray>("value");
			var groups = jArray.ToObject<List<Group>>();
			return groups;
		}

		public async Task<JArray> GetGroupsAsJArray(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"groups/";

			options = EnsureAddFilterForGroups(options);

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				options: options,
				cancellationToken: cancellationToken
			);
			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			var jArray = JObject.Parse(content).Value<JArray>("value");
			return jArray;
		}

		public async Task<List<Group>> GetGroupsPaged(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"groups/";

			var groups = new List<Group>();

			options = EnsureAddFilterForGroups(options);

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				options: options,
				cancellationToken: cancellationToken
			);

			var nextLink = "";
			do
			{
				if (!string.IsNullOrEmpty(nextLink))
				{
					response = await azureQueryHelper.getQuery(
						nextLink,
						cancellationToken: cancellationToken
					);
				}

				if (response == null)
				{
					return groups;
				}

				var content = await response.Content.ReadAsStringAsync(cancellationToken);
				var jObject = JObject.Parse(content);
				var jArray = jObject.Value<JArray>("value");
				var groups_ = jArray.ToObject<List<Group>>();
				if (groups_ == null)
				{
					return groups;
				}
				groups.AddRange(groups_);
				nextLink = jObject.Value<string>("@odata.nextLink");
			}
			while (!string.IsNullOrEmpty(nextLink));

			return groups;
		}

		public async Task<GroupsDelta> GetGroupsDelta(string deltaLink = null, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"groups/delta";

			var groupDelta = new GroupsDelta();

			var response = string.IsNullOrEmpty(deltaLink) ?
								await azureQueryHelper.getQuery(
									apiRoute: apiRoute,
									graphVersion: "v1.0",
									options: options,
									cancellationToken: cancellationToken
								) :
								await azureQueryHelper.getQuery(
									deltaLink,
									cancellationToken: cancellationToken
								);

			var nextLink = "";
			do
			{
				if (!string.IsNullOrEmpty(nextLink))
				{
					response = await azureQueryHelper.getQuery(
						nextLink,
						cancellationToken: cancellationToken
					);
				}

				if (response == null)
				{
					return groupDelta;
				}

				var content = await response.Content.ReadAsStringAsync(cancellationToken);
				var jObject = JObject.Parse(content);
				var jArray = jObject.Value<JArray>("value");
				var groups = jArray.ToObject<List<Group>>();
				groupDelta.Groups.AddRange(groups);

				nextLink = jObject.Value<string>("@odata.nextLink");
				groupDelta.DeltaLink = jObject.Value<string>("@odata.deltaLink");
			}
			while (!string.IsNullOrEmpty(nextLink));

			return groupDelta;
		}

		public async Task<List<User>> GetGroupMembersByEmail(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"groups/{email}/transitiveMembers/microsoft.graph.user";

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			var jArray = JObject.Parse(content).Value<JArray>("value");
			var users = jArray.ToObject<List<User>>();
			return users;
		}

		public async Task<List<User>> GetGroupMembers(string groupId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"groups/{groupId}/transitiveMembers/microsoft.graph.user";

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				options: options,
				cancellationToken: cancellationToken
			);
			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			var jArray = JObject.Parse(content).Value<JArray>("value");
			var users = jArray.ToObject<List<User>>();
			return users;
		}

		public async Task<JArray> GetGroupMembersAsJArray(string groupId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"groups/{groupId}/transitiveMembers/microsoft.graph.user";

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "beta",
				options: options,
				cancellationToken: cancellationToken
			);
			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			var jArray = JObject.Parse(content).Value<JArray>("value");
			return jArray;
		}

		#endregion groups

		#region appRoles
		public async Task<JArray> GetRolesOfEmail(string email, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"users/{email}/appRoleAssignments";

			options = EnsureAddFilterForResource(options);

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "v1.0",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			var jArray = JObject.Parse(content).Value<JArray>("value");
			return jArray;
		}

		public async Task<IEnumerable<string>> GetRoleNamesOfEmail(string email, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"users/{email}/appRoleAssignments";

			ICollection<QueryOption> options = new List<QueryOption>();
			options.Add(new QueryOption() { Key = "$select", Value = "appRoleId" });
			options = EnsureAddFilterForResource(options);

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "v1.0",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			var jArray = JObject.Parse(content).Value<JArray>("value");
			var roleIds = jArray.Select(x => x.Value<string>("appRoleId"));

			var queryOptions = new List<QueryOption>();
			queryOptions.Add(new QueryOption() { Key = "$select", Value = "appRoles" });
			var application = await this.GetAppDetails(queryOptions);

			return application.AppRoles.Where(x => roleIds.Contains(x.Id)).Select(x => x.DisplayName);
		}

		public async Task<JObject> GetFullAppDetails(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"applications/{applicationId}";

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "v1.0",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);

			var jObject = JObject.Parse(content);
			var application = jObject.ToObject<Application>();

			return jObject;
		}

		public async Task<Application> GetAppDetails(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"applications/{applicationId}";

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "v1.0",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);

			var jObject = JObject.Parse(content);
			var application = jObject.ToObject<Application>();

			return application;
		}

		public async Task<bool> AddRole(string name, string description, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"applications/{applicationId}";

			var queryOptions = new List<QueryOption>();
			queryOptions.Add(new QueryOption() { Key = "$select", Value = "appRoles" });
			var application = await this.GetAppDetails(queryOptions);

			if (application.AppRoles.Any(x => x.Value == name))
			{
				return true;
			}

			application.AppRoles.Add(new AppRole()
			{
				Id = Guid.NewGuid().ToString(),
				AllowedMemberTypes = new List<string> { "User" },
				Description = description,
				DisplayName = name,
				IsEnabled = true,
				Origin = "Application",
				Value = name
			});
			var appRoles = application.AppRoles;

			var response = await azureQueryHelper.patchQuery(
				apiRoute: apiRoute,
				content: new { appRoles },
				graphVersion: "v1.0",
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return false;
			}

			return true;
		}

		public async Task<bool> EnalbeRole(string name, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"applications/{applicationId}";

			var queryOptions = new List<QueryOption>();
			queryOptions.Add(new QueryOption() { Key = "$select", Value = "appRoles" });
			var application = await this.GetAppDetails(queryOptions);

			var targetAppRole = application.AppRoles.FirstOrDefault(x => x.Value == name);
			if (targetAppRole == null)
			{
				return false;
			}

			var appRoles = application.AppRoles;
			targetAppRole.IsEnabled = true;

			var response = await azureQueryHelper.patchQuery(
				apiRoute: apiRoute,
				content: new { appRoles },
				graphVersion: "v1.0",
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return false;
			}

			return true;
		}

		public async Task<bool> DisableRole(string name, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"applications/{applicationId}";

			var queryOptions = new List<QueryOption>();
			queryOptions.Add(new QueryOption() { Key = "$select", Value = "appRoles" });
			var application = await this.GetAppDetails(queryOptions);

			var targetAppRole = application.AppRoles.FirstOrDefault(x => x.Value == name);
			if (targetAppRole == null)
			{
				return false;
			}

			var appRoles = application.AppRoles;
			targetAppRole.IsEnabled = false;

			var response = await azureQueryHelper.patchQuery(
				apiRoute: apiRoute,
				content: new { appRoles },
				graphVersion: "v1.0",
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return false;
			}

			return true;
		}

		public async Task<bool> DeleteRole(string name, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"applications/{applicationId}";

			var queryOptions = new List<QueryOption>();
			queryOptions.Add(new QueryOption() { Key = "$select", Value = "appRoles" });
			var application = await this.GetAppDetails(queryOptions);

			var targetAppRole = application.AppRoles.FirstOrDefault(x => x.Value == name);
			if (targetAppRole == null)
			{
				return false;
			}

			var appRoles = application.AppRoles;
			appRoles.Remove(targetAppRole);

			var response = await azureQueryHelper.patchQuery(
				apiRoute: apiRoute,
				content: new { appRoles },
				graphVersion: "v1.0",
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return false;
			}

			return true;
		}

		public async Task<bool> AddToRoleById(string userId, string roleId, CancellationToken cancellationToken = default(CancellationToken))
		{

			var apiRoute = $"users/{userId}/appRoleAssignments";

			var content = new AppRoleAssignmentContent
			{
				PrincipalId = userId,
				ResourceId = resourceId,
				AppRoleId = roleId
			};

			var response = await azureQueryHelper.postQuery(
				apiRoute: apiRoute,
				content: content,
				graphVersion: "v1.0",
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return false;
			}

			return true;
		}

		public async Task<bool> AddToRole(string email, string roleId, CancellationToken cancellationToken = default(CancellationToken))
		{

			var userId = await this.GetUserId(email);
			var apiRoute = $"users/{email}/appRoleAssignments";

			var content = new AppRoleAssignmentContent
			{
				PrincipalId = userId,
				ResourceId = resourceId,
				AppRoleId = roleId
			};

			var response = await azureQueryHelper.postQuery(
				apiRoute: apiRoute,
				content: content,
				graphVersion: "v1.0",
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return false;
			}

			return true;
		}

		public async Task<bool> RemoveFromRoleById(string userId, string roleAssignmentId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"users/{userId}/appRoleAssignments/{roleAssignmentId}";

			var response = await azureQueryHelper.deleteQuery(
				apiRoute: apiRoute,
				graphVersion: "v1.0",
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return false;
			}

			return true;
		}

		public async Task<bool> RemoveFromRole(string email, string roleAssignmentId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var userId = await this.GetUserId(email);

			var apiRoute = $"users/{userId}/appRoleAssignments/{roleAssignmentId}";

			var response = await azureQueryHelper.deleteQuery(
				apiRoute: apiRoute,
				graphVersion: "v1.0",
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return false;
			}

			return true;
		}

		public async Task<List<AppRoleAssignment>> GetAppRoleAssignments(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"servicePrincipals/{resourceId}/appRoleAssignedTo";

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "v1.0",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			var jArray = JObject.Parse(content).Value<JArray>("value");
			var appRoleAssignment = jArray.ToObject<List<AppRoleAssignment>>();
			return appRoleAssignment;
		}

		public async Task<JArray> GetAppRoleAssignmentsAsJObect(ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"servicePrincipals/{resourceId}/appRoleAssignedTo";

			var response = await azureQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "v1.0",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var content = await response.Content.ReadAsStringAsync(cancellationToken);
			return JObject.Parse(content).Value<JArray>("value");
		}

		public async Task<string> GetRoleId(string roleValue, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var application = await this.GetAppDetails();
			return application.AppRoles.Where(x => x.Value == roleValue).FirstOrDefault()?.Id;
		}

		public async Task<string> GetRoleAssignmentId(string roleValue, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var application = await this.GetAppDetails();
			return application.AppRoles.Where(x => x.Value == roleValue).FirstOrDefault()?.Id;
		}

		#endregion appRoles

		#region mail
		public async Task<bool> SendMailAsync(string senderEmail, Email message, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (Site.StaticValues.BlockAllEmail) return false;

			var apiRoute = $"users/{senderEmail}/sendMail";

			var response = await azureQueryHelper.postQuery(
				apiRoute: apiRoute,
				content: new { message },
				graphVersion: "beta",
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return false;
			}

			return true;
		}
		#endregion mail
	}
}
