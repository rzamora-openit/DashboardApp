using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Application;
using OpeniT.PowerbiDashboardApp.Models.Files;
using System;
using System.Net;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Controllers.Api.Dashboard
{
	[Authorize(Policy = "Internal")]
	[Route("api/dashboard/[controller]")]
	[ApiController]
	[ValidateAntiForgeryToken]
	public class ImageController : ControllerBase
	{
		private string ControllerName = "api/dashboard/" + nameof(ImageController);

		private readonly IDataRepository dataRepository;
		private readonly IAzureQueryRepository azureQuery;
		private readonly IApplicationLogger logger;

		public ImageController(IDataRepository dataRepository,
			IAzureQueryRepository azureQuery,
			IApplicationLogger logger)
		{
			this.dataRepository = dataRepository;
			this.azureQuery = azureQuery;
			this.logger = logger;
		}


		[IgnoreAntiforgeryToken]
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Get" };

			try
			{
				var email = this.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

				#region ServerSide Validation
				if (!this.ModelState.IsValid)
				{
					await logger.LogInvalidData(activity, "Invalid model");
					return this.BadRequest("Invalid request.");
				}
				#endregion ServerSide Validation

				await logger.LogDataAccess(activity: activity, relevantObject: nameof(Image), reference: $"{email}", log: $"Get User Image for internal account Email [{email}] success");

				try
				{
					var thumbnail = await this.azureQuery.GetUserPhotoByEmail(email);
					return File(thumbnail.Content, thumbnail.ContentType);
				}
				catch
				{
					return await GetFileImageAsync(null, email.Substring(0, 1));
				}
			}
			catch (Exception ex)
			{
				await logger.LogException(activity, ex);
				return this.BadRequest("Error in proccessing the request.");
			}

			await logger.LogUnknown(activity);
			return this.BadRequest("Unknown proccess failure.");
		}

		public async Task<IActionResult> GetFileImageAsync(Image image, string fallbackText = "!")
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Index" };

			try
			{
				var owner = this.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

				if (image?.Blob?.Binaries is not null)
				{
					await logger.LogDataAccess(activity: activity, relevantObject: nameof(Image), reference: $"{image.Id}", log: $"Get Success");

					var mime = Methods.Mime.GetMimeType(image.FileName);
					return File(image.Blob.Binaries, mime);
				}

				using (WebClient webClient = new WebClient())
				{
					await logger.LogDataAccess(activity: activity, relevantObject: nameof(Image), reference: $"{image?.Id}", log: $"Get directed to default");

					byte[] data = webClient.DownloadData("https://dummyimage.com/128/C63734/fff.png&text=" + fallbackText.ToUpper());
					return File(data, "image/png");
				}
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
