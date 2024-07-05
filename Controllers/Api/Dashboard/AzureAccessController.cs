using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Application;
using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Controllers.Api.Dashboard
{
	[Authorize(Policy = "Internal")]
	[Route("api/dashboard/[controller]")]
	[ApiController]
	[ValidateAntiForgeryToken]
	public class AzureAccessController : ControllerBase
	{
		private string ControllerName = "api/dashboard/" + nameof(AzureAccessController);

		private readonly IDataRepository dataRepository;
		private readonly IAzureQueryRepository azureQuery;
		private readonly IAccessProfileHelper accessProfileHelper;
		private readonly IApplicationLogger logger;

		public AzureAccessController(IDataRepository dataRepository,
						IAzureQueryRepository azureQuery,
						IAccessProfileHelper accessProfileHelper,
						IApplicationLogger logger)
		{
			this.dataRepository = dataRepository;
			this.azureQuery = azureQuery;
			this.accessProfileHelper = accessProfileHelper;
			this.logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] string search, [FromQuery] int top = 999)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Get/{search}/{top}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var queryOptions = new List<QueryOption>();
				queryOptions.Add(new QueryOption() { Key = "$count", Value = "true" });
				if (!string.IsNullOrEmpty(search))
				{
					queryOptions.Add(new QueryOption() { Key = "$search", Value = $"\"displayName:{search}\" OR \"mail:{search}\"" });
				}
				queryOptions.Add(new QueryOption() { Key = "$filter", Value = "accountEnabled eq true and endsWith(mail,'openit.com')" });
				queryOptions.Add(new QueryOption() { Key = "$select", Value = "id,accountEnabled,mail,companyName,displayName,department,givenName,jobTitle,physicalDeliveryOfficeName,surname,userPrincipalName" });
				queryOptions.Add(new QueryOption() { Key = "$top", Value = $"{top}" });

				var result = await azureQuery.GetUsers(queryOptions);
				if (result == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(ViewModels.Azure.User), reference: $"{search}", log: $"Not found");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(ViewModels.Azure.User), reference: $"{search}", log: $"Get Success");
				return this.Ok(result);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get users by searching {search}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("{email}")]
		public async Task<IActionResult> GetByEmail(string email, [FromQuery] string mode = "")
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetByEmail/{email}/{mode}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				if (mode.ToLower().Equals("full"))
				{
					var result = await this.azureQuery.GetUserByEmailAsJObject(email);
					if (result == null)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(ViewModels.Azure.User), reference: $"{email}", log: $"Not found");
						return this.BadRequest(log);
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(ViewModels.Azure.User), reference: $"{email}", log: $"Get Success");
					return this.Ok(result);
				}
				else
				{
					var result = await this.azureQuery.GetUserByEmail(email);
					if (result == null)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(ViewModels.Azure.User), reference: $"{email}", log: $"Not found");
						return this.BadRequest(log);
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(ViewModels.Azure.User), reference: $"{email}", log: $"Get Success");
					return this.Ok(result);
				}
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get user with an email of {email}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("me")]
		public async Task<IActionResult> GetMe()
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetMe" };
			var owner = this.User.Identity.Name;

			try
			{
				var result = await this.azureQuery.GetUserByEmail(owner);
				if (result == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(ViewModels.Azure.User), reference: $"{owner}", log: $"Not found");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(ViewModels.Azure.User), reference: $"{owner}", log: $"Get Success");
				return this.Ok(result);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get me with an email of {owner}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("{email}/photo")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> GetUserPhoto(string email)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetUserPhoto/{email}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var result = await this.azureQuery.GetUserPhotoByEmail(email);
				if (result == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(ViewModels.Azure.Photo), reference: $"{email}", log: $"Not found");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(ViewModels.Azure.Photo), reference: $"{email}", log: $"Get Success");
				return File(result.Content, result.ContentType);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get user photo with an email of {email}");
				return this.Redirect("/images/ImgPlaceholderBlank.png");
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("{email}/manager")]
		public async Task<IActionResult> GetUserManager(string email)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetUserManager/{email}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var result = await this.azureQuery.GetUserManagerByEmail(email);
				if (result == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(ViewModels.Azure.User), reference: $"{email}", log: $"Not found");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(ViewModels.Azure.User), reference: $"{email}", log: $"Get Success");
				return this.Ok(result);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get user manager with an email of {email}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("ref/{id}/photo")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> GetUserPhotoById(string id)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetUserPhotoById/{id}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var result = await this.azureQuery.GetUserPhotoById(id);
				if (result == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(ViewModels.Azure.Photo), reference: $"{id}", log: $"Not found");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(ViewModels.Azure.Photo), reference: $"{id}", log: $"Get Success");
				return File(result.Content, result.ContentType);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get user photo with an id of {id}");
				return this.Redirect("/images/ImgPlaceholderBlank.png");
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("{email}/groups")]
		public async Task<IActionResult> GetUserGroups(string email)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetUserGroups/{email}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var queryOptions = new List<QueryOption>();
				queryOptions.Add(new QueryOption() { Key = "$select", Value = "odata.type,id,displayName,mail,description,members" });

				var results = await this.azureQuery.GetUserGroupsByEmail(email, queryOptions);
				if (results == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Group), reference: $"{email}", log: $"Not found");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(Group), reference: $"{email}", log: $"Get Success");
				return this.Ok(results);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get user groups with an email of {email}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("{email}/transitiveGroups")]
		public async Task<IActionResult> GetUserGroupsTransitive(string email)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetUserGroupsTransitive/{email}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var queryOptions = new List<QueryOption>();
				queryOptions.Add(new QueryOption() { Key = "$select", Value = "odata.type,id,displayName,mail,description,members" });

				var results = await this.azureQuery.GetUserGroupsTransitiveByEmail(email, queryOptions);
				if (results == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: "Groups", reference: $"{email}", log: $"Not found");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: "Groups", reference: $"{email}", log: $"Get Success");
				return this.Ok(results);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get user group transitive with an email of {email}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("{email}/roles")]
		public async Task<IActionResult> GetRolesByEmail(string email)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetRolesByEmail/{email}" };

			try
			{
				var owner = this.User.Identity.Name;
				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var results = await this.azureQuery.GetRolesOfEmail(email);
				if (results == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: "Roles", reference: $"{email}", log: $"Not Found");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: "Roles", reference: $"{email}", log: $"Get Success");
				return this.Ok(results);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get user roles with an email of {email}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpPost("{userId}/roles/{roleId}")]
		public async Task<IActionResult> AddToRolesById(string userId, string roleId)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"AddToRolesById/{userId}/{roleId}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleId))
				{
					var log = await logger.LogInvalidData(activity: activity, log: $"invalid user[{userId}] or role[{roleId}] Id");
					return this.BadRequest(log);
				}

				var results = await this.azureQuery.AddToRoleById(userId, roleId);
				if (!results)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(AppRoleAssignmentContent), reference: $"{userId}/{roleId}", log: $"Add Role Failed");
					return this.BadRequest(log);
				}

				await Task.Delay(5000);
				var internalAccount = await this.dataRepository.GetInternalAccountByEmail(AzureStaticStore.GetMemberById(userId).Mail);
				await accessProfileHelper.FetchAndStoreInfo(internalAccount, true);

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(AppRoleAssignmentContent), reference: $"{userId}/{roleId}", log: $"Add Role Success");
				return this.Ok(results);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to add role {roleId} to user {userId}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpDelete("{userId}/appRoleAssignments/{roleAssignnmentId}")]
		public async Task<IActionResult> RemoveFromAssignmentByEmail(string userId, string roleAssignnmentId)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"RemoveFromAssignmentByEmail/{userId}/{roleAssignnmentId}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var results = await this.azureQuery.RemoveFromRoleById(userId, roleAssignnmentId);
				if (!results)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(AppRoleAssignmentContent), reference: $"{userId}/{roleAssignnmentId}", log: $"Remove Role Failed");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(AppRoleAssignmentContent), reference: $"{userId}/{roleAssignnmentId}", log: $"Remove Role Success");
				return this.Ok(results);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to remove role {roleAssignnmentId} to user {userId}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("search/{search}")]
		public async Task<IActionResult> GetSearch(string search = "", [FromQuery] int count = 10)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetSearch/{search}/{count}" };

			try
			{
				var owner = this.User.Identity.Name;

				var queryOptions = new List<QueryOption>();
				queryOptions.Add(new QueryOption() { Key = "$count", Value = "true" });
				queryOptions.Add(new QueryOption() { Key = "$search", Value = $"\"displayName:{search}\" OR \"mail:{search}\"" });
				queryOptions.Add(new QueryOption() { Key = "$filter", Value = "accountEnabled eq true and endsWith(mail,'openit.com')" });
				queryOptions.Add(new QueryOption() { Key = "$select", Value = "accountEnabled,mail,companyName,displayName,department,givenName,jobTitle,physicalDeliveryOfficeName,surname,userPrincipalName" });
				queryOptions.Add(new QueryOption() { Key = "$top", Value = $"{count}" });

				var results = await this.azureQuery.GetUsers(queryOptions);

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(ViewModels.Azure.User), reference: $"{search}", log: $"Get Success");
				return this.Ok(results);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get users by searching {search}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("groups")]
		public async Task<IActionResult> GetGroups([FromQuery] string search, [FromQuery] int top = 999, [FromQuery] string mode = "")
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetGroups/{search}/{top}/{mode}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				if (mode.ToLower().Equals("full"))
				{
					var queryOptions = new List<QueryOption>();
					queryOptions.Add(new QueryOption() { Key = "$count", Value = "true" });
					if (!string.IsNullOrEmpty(search))
					{
						queryOptions.Add(new QueryOption() { Key = "$filter", Value = $"proxyAddresses/any(x:startswith(x,'smtp:{search}')) or startswith(mail,'{search}') or startswith(displayName,'{search}')" });
					}
					queryOptions.Add(new QueryOption() { Key = "$top", Value = $"{top}" });

					var results = await this.azureQuery.GetGroupsAsJArray(queryOptions);
					if (results == null)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Group), reference: $"{search}", log: $"Not found");
						return this.BadRequest(log);
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(Group), reference: $"{search}", log: $"Get Success");
					return this.Ok(results);
				}
				else
				{
					var queryOptions = new List<QueryOption>();
					queryOptions.Add(new QueryOption() { Key = "$count", Value = "true" });
					if (!string.IsNullOrEmpty(search))
					{
						queryOptions.Add(new QueryOption() { Key = "$filter", Value = $"proxyAddresses/any(x:startswith(x,'smtp:{search}')) or startswith(mail,'{search}') or startswith(displayName,'{search}')" });
					}
					queryOptions.Add(new QueryOption() { Key = "$select", Value = $"id,displayName,mail" });
					queryOptions.Add(new QueryOption() { Key = "$top", Value = $"{top}" });

					var results = await this.azureQuery.GetGroupsAsJArray(queryOptions);
					if (results == null)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Group), reference: $"{search}", log: $"Not found");
						return this.BadRequest(log);
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(Group), reference: $"{search}", log: $"Get Success");
					return this.Ok(results);
				}
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get groups by searching {search}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("groups/extendmembers")]
		[Authorize(Roles = "iToolsMaster")]
		public async Task<IActionResult> GetGroupsAndMembersIds([FromQuery] string search, [FromQuery] int top = 999)
		{

			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetGroupsAndMembersIds/{search}/{top}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var queryOptions = new List<QueryOption>();
				queryOptions.Add(new QueryOption() { Key = "$count", Value = "true" });
				queryOptions.Add(new QueryOption() { Key = "$top", Value = $"{top}" });
				queryOptions.Add(new QueryOption() { Key = "$select", Value = $"id,displayName,mail" });
				queryOptions.Add(new QueryOption() { Key = "$expand", Value = $"members($select=odata.type,id,displayName,mail)" });

				var results = await this.azureQuery.GetGroupsPaged(queryOptions);
				Methods.AzureModels.ParseTransitiveMembers(results);
				if (results == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Group), reference: $"{search}", log: $"Not found");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(Group), reference: $"{search}", log: $"Get Success");
				return this.Ok(results);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get groups and members by seraching {search}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("groups/{groupId}/members")]
		public async Task<IActionResult> GetGroupsMembers(string groupId, [FromQuery] string mode = "", [FromQuery] int top = 999)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetGroupsMembers/{groupId}/{mode}/{top}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var queryOptions = new List<QueryOption>();
				queryOptions.Add(new QueryOption() { Key = "$count", Value = "true" });
				queryOptions.Add(new QueryOption() { Key = "$top", Value = $"{top}" });

				if (mode.ToLower().Equals("full"))
				{
					var results = await this.azureQuery.GetGroupMembersAsJArray(groupId, queryOptions);
					if (results == null)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(GroupMember), reference: $"{groupId}", log: $"Not found");
						return this.BadRequest(log);
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(GroupMember), reference: $"{groupId}", log: $"Get Success");
					return this.Ok(results);
				}
				else
				{
					var results = await this.azureQuery.GetGroupMembers(groupId, queryOptions);
					if (results == null)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(GroupMember), reference: $"{groupId}", log: $"Not found");
						return this.BadRequest(log);
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(GroupMember), reference: $"{groupId}", log: $"Get Success");
					return this.Ok(results);
				}
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get group members with an id of {groupId}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("groups/delta")]
		[Authorize(Roles = "iToolsMaster")]
		public async Task<IActionResult> GetGroupsDelta([FromQuery] string deltaLink = null)
		{

			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetGroupsDelta/{deltaLink}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation


				if (string.IsNullOrEmpty(deltaLink))
				{
					var queryOptions = new List<QueryOption>();
					queryOptions.Add(new QueryOption() { Key = "$select", Value = "displayName,mail,description,members" });

					var results = await this.azureQuery.GetGroupsDelta(options: queryOptions);
					if (results == null)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Group), reference: $"", log: $"Not found");
						return this.BadRequest(log);
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(Group), reference: $"", log: $"Get Success");
					return this.Ok(results);
				}
				else
				{
					var results = await this.azureQuery.GetGroupsDelta(deltaLink);
					if (results == null)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Group), reference: $"", log: $"Not found");
						return this.BadRequest(log);
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(Group), reference: $"", log: $"Get Success");
					return this.Ok(results);
				}
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get group delta");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("app")]
		public async Task<IActionResult> GetAppDetails([FromQuery] string mode = "")
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetAppDetails/{mode}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				if (mode.ToLower().Equals("full"))
				{
					var result = await this.azureQuery.GetFullAppDetails();
					if (result == null)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Application), reference: $"", log: $"Failed to get Application information");
						return this.BadRequest(log);
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(Application), reference: $"", log: $"Get Success");
					return this.Ok(result);
				}
				else
				{
					var queryOptions = new List<QueryOption>();
					queryOptions.Add(new QueryOption() { Key = "$select", Value = "createdDateTime,displayName,publisherDomain,appRoles" });

					var result = await this.azureQuery.GetAppDetails(queryOptions);
					if (result == null)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Application), reference: $"", log: $"Failed to get Application information");
						return this.BadRequest(log);
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(Application), reference: $"", log: $"Get Success");
					return this.Ok(result);
				}
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: "Failed to get applications");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("roles")]
		public async Task<IActionResult> GetAppRolesDetails()
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetAppRolesDetails" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var queryOptions = new List<QueryOption>();
				queryOptions.Add(new QueryOption() { Key = "$select", Value = "appRoles" });

				var result = await this.azureQuery.GetAppDetails(queryOptions);
				if (result == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Application), reference: $"", log: $"Failed to get Application information");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(Application), reference: $"", log: $"Get Success");
				return this.Ok(result.AppRoles
					.Where(x => x.Id != "be1b2351-7f90-479d-b08e-fa4dee7ecbaa") //hides itools master
					.OrderBy(x => x.Value));
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: "Failed to get application roles");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("getRoleId/{value}")]
		public async Task<IActionResult> GetRoleId(string value)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetRoleId/{value}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var result = await this.azureQuery.GetRoleId(value);
				if (result == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: "Application", reference: $"", log: $"Failed to get Application information");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: "Application", reference: $"", log: $"Get Success");
				return this.Ok(result);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to get application role with a value of {value}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("roles/assignments")]
		public async Task<IActionResult> GetAppRolesAssignments([FromQuery] string mode = "")
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetAppRolesAssignments/{mode}" };

			try
			{
				var owner = this.User.Identity.Name;
				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				if (mode.ToLower().Equals("full"))
				{
					var results = await this.azureQuery.GetAppRoleAssignmentsAsJObect();
					if (results == null)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(AppRole), reference: $"", log: $"Not Found");
						return this.BadRequest(log);
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(AppRole), reference: $"", log: $"Get Success");
					return this.Ok(results);
				}
				else
				{
					var results = await this.azureQuery.GetAppRoleAssignments();
					if (results == null)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(AppRole), reference: $"", log: $"Not Found");
						return this.BadRequest(log);
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(AppRole), reference: $"", log: $"Get Success");
					//hides itools master
					return this.Ok(results.Where(x => x.AppRoleId != "be1b2351-7f90-479d-b08e-fa4dee7ecbaa"));
				}
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: "Failed to get application role assignments");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpPost("roles/{name}/{description}")]
		public async Task<IActionResult> AddAppRoles(string name, string description)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"AddAppRoles/{name}/{description}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var result = await this.azureQuery.AddRole(name, description);
				if (!result)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Application), reference: $"", log: $"Failed to add Application Role");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(Application), reference: $"", log: $"Save Success");
				return this.Ok(result);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to add application role with a name of {name}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpPut("roles/enable/{name}")]
		public async Task<IActionResult> EnableAppRoles(string name)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"EnableAppRoles/{name}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var result = await this.azureQuery.EnalbeRole(name);
				if (!result)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Application), reference: $"{name}", log: $"Failed to enable Application Role");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(Application), reference: $"{name}", log: $"Enable Success");
				return this.Ok(result);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to enable application role with a name of {name}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpPut("roles/disable/{name}")]
		public async Task<IActionResult> DisableAppRoles(string name)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"DisableAppRoles/{name}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var result = await this.azureQuery.DisableRole(name);
				if (!result)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Application), reference: $"{name}", log: $"Failed to disable Application Role");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(Application), reference: $"{name}", log: $"Disable Success");
				return this.Ok(result);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to disable application role with a name of {name}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpDelete("roles/delete/{name}")]
		public async Task<IActionResult> DeleteAppRoles(string name)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"DeleteAppRoles/{name}" };

			try
			{
				var owner = this.User.Identity.Name;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var result = await this.azureQuery.DeleteRole(name);
				if (!result)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Application), reference: $"{name}", log: $"Failed to delete Application Role");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(Application), reference: $"{name}", log: $"Delete Success");
				return this.Ok(result);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to delete application role with a name of {name}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}
	}
}
