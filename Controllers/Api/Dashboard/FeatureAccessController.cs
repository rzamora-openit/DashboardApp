using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using OpeniT.PowerbiDashboardApp.Models.Application;
using OpeniT.PowerbiDashboardApp.Security.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Controllers.Api.Dashboard
{
	[Authorize(Policy = "Internal")]
	[Route("api/dashboard/[controller]")]
	[ApiController]
	[ValidateAntiForgeryToken]
	public class FeatureAccessController : ControllerBase
	{
		private string ControllerName = "api/dashboard/" + nameof(FeatureAccessController);

		private readonly IDataRepository dataRepository;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly IFeatureAccessHelper featureAccessHelper;
		private readonly IApplicationLogger logger;

		public FeatureAccessController(IDataRepository dataRepository,
						UserManager<ApplicationUser> userManager,
						IFeatureAccessHelper featureAccessHelper,
						IApplicationLogger logger)
		{
			this.dataRepository = dataRepository;
			this.userManager = userManager;
			this.featureAccessHelper = featureAccessHelper;
			this.logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Get" };

			try
			{
				var owner = this.User.Identity.Name;

				var user = await dataRepository.GetInternalApplicationUserByEmail(owner);
				if (user == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(ApplicationUser), reference: $"{owner}", log: $"Not Found");
					return this.BadRequest(log);
				}

				var profile = new AccessProfile()
				{
					Id = user.InternalAccount?.ReferenceId,
					Email = user.Email,
					IsMaster = await userManager.IsInRoleAsync(user, Site.ConstantValues.ItoolsMaster)
				};

				var results = await this.dataRepository.GetFeatureAccessesAsync();
				var services = Site.Services.GenerateServices(profile, Security.AccessLevelFlag.Admin);

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(FeatureAccess), log: $"Get Success");
				return this.Ok(results
					.Where(x => services.Any(y => y.Name == x.FeatureName))
					.OrderBy(x => Site.Services.GenerateService(x.FeatureName).Index)
				);
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: "Failed to get feature access");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpPost("access/{id}")]
		public async Task<IActionResult> AddAccess(int id, Access vm)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"AddAccess/{id}" };

			try
			{
				var owner = this.User.Identity.Name;
				var action = "";

				var featureAccess = await this.dataRepository.GetFeatureAccessById(id);
				if (featureAccess == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(FeatureAccess), reference: $"{id}", log: $"Not found");
					return this.BadRequest(log);
				}

				var user = await dataRepository.GetInternalApplicationUserByEmail(owner);
				if (user == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(ApplicationUser), reference: $"{owner}", log: $"Not Found");
					return this.BadRequest(log);

				}

				var profile = new AccessProfile()
				{
					Id = user.InternalAccount?.ReferenceId,
					Email = user.Email,
					IsMaster = await userManager.IsInRoleAsync(user, Site.ConstantValues.ItoolsMaster)
				};

				if (Security.AccessEvaluator.AssertAccessLevel(featureAccess.Accesses, profile) < Security.AccessLevelFlag.Admin)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(AccessProfile), reference: $"{profile.Email}", log: $"Insufficient Access level");
					return this.BadRequest(log);
				}

				var access = featureAccess.Accesses.FirstOrDefault(x => x.Type == vm.Type && x.AzureId == vm.AzureId);
				if (access != null)
				{
					access.UpdatedDate = DateTime.Now;
					access.UpdatedByEmail = owner;
					access.Level = vm.Level;

					this.dataRepository.UpdateAccess(access);

					var resultSuccess = await this.dataRepository.SaveChangesAsync();
					if (!resultSuccess)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Access), reference: $"{access.Id}", log: $"Failed saving to update access");
						return this.BadRequest($"Update Access failed");
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(Access), reference: $"{access.Id}", log: $"Update Access Success");
					await featureAccessHelper.AddFeatureAccess(featureAccess.FeatureName, access);
					action = "Updated";
				}
				else
				{
					access = vm;
					access.CreatedDate = DateTime.Now;
					access.CreatedByEmail = owner;
					featureAccess.Accesses.Add(access);
					this.dataRepository.AddAccess(access);
					this.dataRepository.UpdateFeatureAccess(featureAccess);

					var resultSuccess = await this.dataRepository.SaveChangesAsync();
					if (!resultSuccess)
					{
						var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Access), log: $"Failed saving to add access");
						return this.BadRequest($"Add Access failed");
					}

					await logger.LogDataAccess(activity: activity, relevantObject: nameof(Access), reference: $"{access.Id}", log: $"Add Access Success");
					await featureAccessHelper.AddFeatureAccess(featureAccess.FeatureName, access);
					action = "Added";
				}

				return this.Ok(new { featureAccess, action });
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to add access for feature access with an id of {id}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpDelete("access/{id}/{accessId}")]
		public async Task<IActionResult> DeleteAccess(int id, int accessId)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"DeleteAccess/{id}/{accessId}" };

			try
			{
				var owner = this.User.Identity.Name;

				var featureAccess = await this.dataRepository.GetFeatureAccessById(id);
				if (featureAccess == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(FeatureAccess), reference: $"{id}", log: $"Not found");
					return this.BadRequest(log);
				}

				var access = featureAccess.Accesses.FirstOrDefault(x => x.Id == accessId);
				if (access == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Access), reference: $"{id}", log: $"Not found");
					return this.BadRequest(log);
				}

				var user = await dataRepository.GetInternalApplicationUserByEmail(owner);
				if (user == null)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(ApplicationUser), reference: $"{owner}", log: $"Not Found");
					return this.BadRequest(log);
				}

				var profile = new AccessProfile()
				{
					Id = user.InternalAccount?.ReferenceId,
					Email = user.Email,
					IsMaster = await userManager.IsInRoleAsync(user, Site.ConstantValues.ItoolsMaster)
				};

				if (Security.AccessEvaluator.AssertAccessLevel(featureAccess.Accesses, profile) < Security.AccessLevelFlag.Admin)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(AccessProfile), reference: $"{profile.Email}", log: $"Insufficient Access level");
					return this.BadRequest(log);
				}

				this.dataRepository.RemoveAccess(access);

				var resultSuccess = await this.dataRepository.SaveChangesAsync();
				if (!resultSuccess)
				{
					var log = await logger.LogFailure(activity: activity, relevantObject: nameof(Access), reference: $"{access.Id}", log: $"Failed saving to delete access");
					return this.BadRequest($"Delete Access failed");
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(Access), reference: $"{access.Id}", log: $"Delete Success");
				await featureAccessHelper.RemoveFeatureAccess(access);

				return this.Ok();
			}
			catch (Exception ex)
			{
				var log = await logger.LogException(activity: activity, exception: ex, log: $"Failed to delete access from feature access with an id of {id}/{accessId}");
				return this.BadRequest(log);
			}

			var logUnknown = await logger.LogUnknown(activity);
			return this.BadRequest(logUnknown);
		}

		[HttpGet("{featureName}")]
		public async Task<IActionResult> GetFeatureAccess(string featureName)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetFeatureAccess" };

			try
			{
				var owner = this.User.Identity.Name;

				var featureAccess = await this.dataRepository.GetFeatureAccessByFeatureName(featureName);

				var user = await dataRepository.GetInternalApplicationUserByEmail(owner);
				var profile = new AccessProfile()
				{
					Id = user?.InternalAccount?.ReferenceId,
					Email = user?.Email,
					IsMaster = await userManager.IsInRoleAsync(user, Site.ConstantValues.ItoolsMaster)
				};

				var userAccess = Security.AccessEvaluator.AssertAccessLevel(featureAccess.Accesses, profile);

				return this.Ok(userAccess);
			}
			catch (Exception ex)
			{
				await logger.LogException(activity, ex);
				return this.BadRequest("Error in proccessing the request.");
			}

			await logger.LogUnknown(activity);
			return this.BadRequest("Unknown proccess failure.");
		}
	}
}
