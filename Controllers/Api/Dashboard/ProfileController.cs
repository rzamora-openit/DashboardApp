﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using OpeniT.PowerbiDashboardApp.Models.Application;
using OpeniT.PowerbiDashboardApp.Models.Objects;
using static OpeniT.PowerbiDashboardApp.Site.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace OpeniT.PowerbiDashboardApp.Controllers.Api.Dashboard
{
	[Authorize(Policy = "Internal")]
	[Route("api/dashboard/[controller]")]
	[ApiController]
	[ValidateAntiForgeryToken]
	public class ProfileController : ControllerBase
	{
		private string ControllerName = "api/dashboard/" + nameof(ProfileController);

		private readonly IDataRepository dataRepository;
		private readonly IApplicationLogger logger;
		private readonly IAccessProfileHelper accessProfileHelper;

		public ProfileController(IDataRepository dataRepository,
			IApplicationLogger logger,
			IAccessProfileHelper accessProfileHelper)
		{
			this.dataRepository = dataRepository;
			this.logger = logger;
			this.accessProfileHelper = accessProfileHelper;
		}

		[HttpGet("write-permission")]
		public async Task<IActionResult> CheckWritePermission()
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"CheckWritePermission" };

			var owner = this.User.Identity.Name;

			try
			{
				var hasPermission = await this.accessProfileHelper.HasPermission(owner, FeatureNames.Dashboard, Security.AccessLevelFlag.Write);
				
				await logger.LogDataAccess(activity: activity, log: $"Get success");
				return this.Ok(hasPermission);
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
