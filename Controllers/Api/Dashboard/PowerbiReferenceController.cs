using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Data;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Application;
using OpeniT.PowerbiDashboardApp.Models.Objects;
using System;
using System.Threading.Tasks;

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

		public PowerbiReferenceController(IDataRepository dataRepository,
			IApplicationLogger logger)
		{
			this.dataRepository = dataRepository;
			this.logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Get" };

			try
			{
				var results = await this.dataRepository.GetPowerbiReferences();

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), log: $"Get success");
				return this.Ok(results);
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

			try
			{
				var result = await this.dataRepository.GetPowerbiReferenceById(id);
				if (result == null)
				{
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{id}", log: $"Not found");
					return this.BadRequest(log);
				}

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{id}", log: $"Get Success");
				return this.Ok(result);
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

			try
			{
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
					var log = await logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), log: $"Save failed");
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

			try
			{
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

			try
			{
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
	}
}
