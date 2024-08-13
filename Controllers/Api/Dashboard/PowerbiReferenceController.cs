using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Data;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using OpeniT.PowerbiDashboardApp.Models.Application;
using OpeniT.PowerbiDashboardApp.Models.Objects;
using OpeniT.PowerbiDashboardApp.Security.Model;
using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OpeniT.PowerbiDashboardApp.Site.Services;

namespace OpeniT.PowerbiDashboardApp.Controllers.Api.Dashboard
{
	[Authorize(Policy = "Internal")]
	[Route("api/dashboard/[controller]")]
	[ApiController]
	[ValidateAntiForgeryToken]
	public class PowerbiReferenceController : ControllerBase
	{
		private string ControllerName = "api/dashboard/" + nameof(PowerbiReferenceController);

		private readonly IDataRepository dataRepository;
		private readonly IApplicationLogger logger;
		private readonly IAccessProfileHelper accessProfileHelper;

		public PowerbiReferenceController(IDataRepository dataRepository,
			IApplicationLogger logger,
			IAccessProfileHelper accessProfileHelper)
		{
			this.dataRepository = dataRepository;
			this.logger = logger;
			this.accessProfileHelper = accessProfileHelper;
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Get" };

			var owner = this.User.Identity.Name;

			try
			{
				IEnumerable<PowerbiReference> references = new List<PowerbiReference>();
				
				var hasPermission = await this.accessProfileHelper.HasPermission(owner, FeatureNames.Dashboard, Security.AccessLevelFlag.Admin);
				if (hasPermission)
				{
					references = await this.dataRepository.GetPowerbiReferences();
				}
				else
				{
					var internalUser = await this.dataRepository.GetInternalAccountByEmail(owner);
					if (internalUser == null)
					{
						var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(InternalAccount), reference: $"{owner}", log: $"Not found");
						return this.BadRequest(log);
					}

					var member = Helpers.AzureStaticStore.GetMemberById(internalUser.ReferenceId);
					var groupIds = member.GroupIds;

					references = await this.dataRepository.GetPowerbiReferencesSharing(internalUser.ReferenceId, groupIds);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), log: $"Get success");
				return this.Ok(references);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity, ex);
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetReference(int id)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Post" };

			var owner = this.User.Identity.Name;

			try
			{
				PowerbiReference reference = null;

				var hasPermission = await this.accessProfileHelper.HasPermission(owner, FeatureNames.Dashboard, Security.AccessLevelFlag.Admin);
				if (hasPermission)
				{
					reference = await this.dataRepository.GetPowerbiReferenceById(id);
				}
				else
				{
					var internalUser = await this.dataRepository.GetInternalAccountByEmail(owner);
					if (internalUser == null) 
					{
						var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(InternalAccount), reference: $"{owner}", log: $"Not found");
						return this.BadRequest(log);
					}

					var member = Helpers.AzureStaticStore.GetMemberById(internalUser.ReferenceId);
					var groupIds = member.GroupIds;

					reference = await this.dataRepository.GetPowerbiReferenceSharingById(id, internalUser.ReferenceId, groupIds);
				}

				if (reference == null)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{id}", log: $"Not found");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{id}", log: $"Get Success");
				return this.Ok(reference);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity, ex);
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] PowerbiReference powerbiReference)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Post" };

			var owner = this.User.Identity.Name;

			try
			{
				var hasPermission = await this.accessProfileHelper.HasPermission(owner, FeatureNames.Dashboard, Security.AccessLevelFlag.Write);
				if (!hasPermission)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), log: $"Insufficient access");
					return this.BadRequest("Insufficient access");
				}

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				this.dataRepository.AddPowerbiReference(powerbiReference);

				var result = await this.dataRepository.SaveChangesAsync();
				if (!result)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(PowerbiReference), log: $"Save failed");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), log: $"Get Success");
				return this.Ok(powerbiReference);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity, ex);
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Put(int id, [FromBody] PowerbiReference vm)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Put/{id}" };

			var owner = this.User.Identity.Name;

			try
			{
				var hasPermission = await this.accessProfileHelper.HasPermission(owner, FeatureNames.Dashboard, Security.AccessLevelFlag.Write);
				if (!hasPermission)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), log: $"Insufficient access");
					return this.BadRequest("Insufficient access");
				}

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var powerbiReference = await this.dataRepository.GetPowerbiReferenceById(id);
				if (powerbiReference == null)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{id}", log: $"Not found");
					return this.BadRequest(log);
				}

				powerbiReference.Name = vm.Name;
				powerbiReference.WorkGroupId = vm.WorkGroupId;
				powerbiReference.DataSetId = vm.DataSetId;
				powerbiReference.ReportId = vm.ReportId;

				this.dataRepository.UpdatePowerbiReference(powerbiReference);

				var result = await this.dataRepository.SaveChangesAsync();
				if (!result)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Update failed");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Update success");
				return this.Ok(powerbiReference);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity, ex);
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id) 
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Delete/{id}" };

			var owner = this.User.Identity.Name;

			try
			{
				var hasPermission = await this.accessProfileHelper.HasPermission(owner, FeatureNames.Dashboard, Security.AccessLevelFlag.Write);
				if (!hasPermission)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), log: $"Insufficient access");
					return this.BadRequest("Insufficient access");
				}

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var powerbiReference = await this.dataRepository.GetPowerbiReferenceById(id);
				if (powerbiReference == null)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{id}", log: $"Not found");
					return this.BadRequest(log);
				}

				this.dataRepository.UpdatePowerbiReference(powerbiReference);

				var result = await this.dataRepository.SaveChangesAsync();
				if (!result)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Delete failed");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Delete success");
				return this.Ok();
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity, ex);
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);

		}

		[HttpPut("share/{powerbiId}")]
		public async Task<IActionResult> Share([FromBody] List<UserShare> shareTos, int powerbiId)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Share/{powerbiId}" };

			var owner = this.User.Identity.Name;

			try
			{
				var hasPermission = await this.accessProfileHelper.HasPermission(owner, FeatureNames.Dashboard, Security.AccessLevelFlag.Write);
				if (!hasPermission)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), log: $"Insufficient access");
					return this.BadRequest("Insufficient access");
				}

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var powerbiReference = await this.dataRepository.GetPowerbiReferenceById(powerbiId);
				if (powerbiReference == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiId}", log: $"Not found");
					return this.BadRequest(log);
				}

				if (powerbiReference.Sharing?.UserShares == null)
				{
					powerbiReference.Sharing = new Sharing()
					{
						UserShares = shareTos
					};
				}
				else
				{
					var alreadyExists = powerbiReference.Sharing.UserShares.Any(u => shareTos.Any(s => s.Email == u.Email));
					if (alreadyExists)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Share failed to {string.Join(',', shareTos.Select(s => s.Email))}. An email is already added.");
						return this.BadRequest("An email is already added");
					}

					powerbiReference.Sharing.UserShares = powerbiReference.Sharing.UserShares.Concat(shareTos).ToList();
				}

				this.dataRepository.UpdatePowerbiReference(powerbiReference);

				var result = await this.dataRepository.SaveChangesAsync();
				if (!result)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Share failed to {string.Join(',', shareTos.Select(s => s.Email))}");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Share success to {string.Join(',', shareTos.Select(s => s.Email))}");
				return this.Ok(powerbiReference);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity, ex);
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpPut("remove-share/{powerbiId}")]
		public async Task<IActionResult> RemoveShare([FromBody] List<UserShare> userShares, int powerbiId)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"RemoveShare/{powerbiId}" };

			var owner = this.User.Identity.Name;

			try
			{
				var hasPermission = await this.accessProfileHelper.HasPermission(owner, FeatureNames.Dashboard, Security.AccessLevelFlag.Write);
				if (!hasPermission)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), log: $"Insufficient access");
					return this.BadRequest("Insufficient access");
				}

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var powerbiReference = await this.dataRepository.GetPowerbiReferenceById(powerbiId);
				if (powerbiReference == null)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiId}", log: $"Not found");
					return this.BadRequest(log);
				}

				if (powerbiReference.Sharing?.UserShares == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Sharing), reference: $"{powerbiId}", log: $"Not found");
					return this.BadRequest(log);
				}

				powerbiReference.Sharing.UserShares = powerbiReference.Sharing.UserShares.Where(u => !userShares.Any(s => u.Email == s.Email)).ToList();

				this.dataRepository.UpdatePowerbiReference(powerbiReference);

				var result = await this.dataRepository.SaveChangesAsync();
				if (!result)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Share failed to {string.Join(',', userShares.Select(s => s.Email))}");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Share success to {string.Join(',', userShares.Select(s => s.Email))}");
				return this.Ok(powerbiReference);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity, ex);
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpPut("group/share/{powerbiId}")]
		public async Task<IActionResult> GroupShare([FromBody] List<GroupShare> shareTos, int powerbiId)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GroupShare/{powerbiId}" };

			var owner = this.User.Identity.Name;

			try
			{
				var hasPermission = await this.accessProfileHelper.HasPermission(owner, FeatureNames.Dashboard, Security.AccessLevelFlag.Write);
				if (!hasPermission)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), log: $"Insufficient access");
					return this.BadRequest("Insufficient access");
				}

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var powerbiReference = await this.dataRepository.GetPowerbiReferenceById(powerbiId);
				if (powerbiReference == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiId}", log: $"Not found");
					return this.BadRequest(log);
				}

				if (powerbiReference.Sharing?.GroupShares == null)
				{
					powerbiReference.Sharing = new Sharing()
					{
						GroupShares = shareTos
					};
				}
				else
				{
					var alreadyExists = powerbiReference.Sharing.GroupShares.Any(u => shareTos.Any(s => s.Email == u.Email));
					if (alreadyExists)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Share failed to {string.Join(',', shareTos.Select(s => s.Email))}. An email is already added.");
						return this.BadRequest("An email is already added");
					}

					powerbiReference.Sharing.GroupShares = powerbiReference.Sharing.GroupShares.Concat(shareTos).ToList();
				}

				this.dataRepository.UpdatePowerbiReference(powerbiReference);

				var result = await this.dataRepository.SaveChangesAsync();
				if (!result)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Share failed to {string.Join(',', shareTos.Select(s => s.Email))}");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Share success to {string.Join(',', shareTos.Select(s => s.Email))}");
				return this.Ok(powerbiReference);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity, ex);
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpPut("group/remove-share/{powerbiId}")]
		public async Task<IActionResult> GroupRemoveShare([FromBody] List<GroupShare> groupShares, int powerbiId)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"RemoveShare/{powerbiId}" };

			var owner = this.User.Identity.Name;

			try
			{
				var hasPermission = await this.accessProfileHelper.HasPermission(owner, FeatureNames.Dashboard, Security.AccessLevelFlag.Write);
				if (!hasPermission)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), log: $"Insufficient access");
					return this.BadRequest("Insufficient access");
				}

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					var log = await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest(log);
				}
				#endregion ServerSide Validation

				var powerbiReference = await this.dataRepository.GetPowerbiReferenceById(powerbiId);
				if (powerbiReference == null)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiId}", log: $"Not found");
					return this.BadRequest(log);
				}

				if (powerbiReference.Sharing?.GroupShares == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Sharing), reference: $"{powerbiId}", log: $"Not found");
					return this.BadRequest(log);
				}

				powerbiReference.Sharing.GroupShares = powerbiReference.Sharing.GroupShares.Where(u => !groupShares.Any(s => u.Email == s.Email)).ToList();

				this.dataRepository.UpdatePowerbiReference(powerbiReference);

				var result = await this.dataRepository.SaveChangesAsync();
				if (!result)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Share failed to {string.Join(',', groupShares.Select(s => s.Email))}");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{powerbiReference.Id}", log: $"Share success to {string.Join(',', groupShares.Select(s => s.Email))}");
				return this.Ok(powerbiReference);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity, ex);
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}
	}
}
