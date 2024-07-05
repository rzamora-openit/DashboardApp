using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Application;
using System;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Helpers
{
	public class ApplicationLogger : IApplicationLogger
	{
		private readonly IDataRepository dataRepository;
		private readonly IHttpContextAccessor httpContextAccessor;

		public ApplicationLogger(IDataRepository dataRepository,
			IHttpContextAccessor httpContextAccessor)
		{
			this.dataRepository = dataRepository;
			this.httpContextAccessor = httpContextAccessor;
		}

		public string GetUserAgent()
		{
			return httpContextAccessor.HttpContext.Request.Headers.ContainsKey("User-Agent") ? httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString() : null;
		}

		public async Task<string> Log(string log)
		{
			try
			{
				var activity = PopulateActivityInfo(new ApplicationActivity() { LogType = Site.ConstantValues.LogMessage, Log = log, DateExcuted = DateTime.Now });
				this.dataRepository.AddActivityAsync(activity);
				await this.dataRepository.SaveChangesAsync();

				Console.WriteLine(log);
			}
			catch (Exception e)
			{
				Console.WriteLine("Logging Failed : " + e.Message);
			}
			return "";
		}

		public async Task<string> Log(ApplicationActivity activity)
		{
			try
			{
				var activity_ = PopulateActivityInfo(activity);
				this.dataRepository.AddActivityAsync(activity_);
				await this.dataRepository.SaveChangesAsync();

				if (activity_.LogType == Site.ConstantValues.LogError ||
					activity_.LogType == Site.ConstantValues.LogFailure ||
					activity_.LogType == Site.ConstantValues.LogInvalidData ||
					activity_.LogType == Site.ConstantValues.LogInvalidAccess)
					Console.WriteLine($"{activity_.Id} :: {activity_.IdentityName} :: {activity_.IpAddress} :: {activity_.Method} :: {activity_.Path} :: {activity_.LogType} : {activity_.Status} - {activity_.Log} for {activity_.RelevantObject} [ {activity_.RelevantObjectReference} ] ");
			}
			catch (Exception e)
			{
				Console.WriteLine("Logging Failed : " + e.Message);
			}
			return "";
		}

		public async Task<string> LogNavigation(ApplicationActivity activity, string log = "")
		{
			activity.LogType = Site.ConstantValues.LogNavigation;
			activity.Log = log;
			activity.Status = "Success";
			await this.Log(activity);
			return "";
		}

		public async Task<string> LogDataAccess(ApplicationActivity activity, string relevantObject = "", string reference = "", string log = "", bool success = true)
		{
			activity.LogType = Site.ConstantValues.LogDataAccess;
			activity.Log = log;
			activity.RelevantObject = relevantObject;
			activity.RelevantObjectReference = reference;
			activity.Status = success ? "Success" : "Failed";
			await this.Log(activity);
			return "";
		}

		public async Task<string> LogJsonSerialized<T>(ApplicationActivity activity, T referenceObject, string relevantObject = "", string log = "", bool success = true)
			where T : class
		{
			activity.LogType = Site.ConstantValues.LogJsonSerialized;
			activity.Log = log;
			activity.RelevantObject = relevantObject;
			activity.RelevantObjectReference = JsonConvert.SerializeObject(referenceObject, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
			activity.Status = success ? "Success" : "Failed";
			await this.Log(activity);
			return "";
		}

		public async Task<string> LogFailure(ApplicationActivity activity, string relevantObject = "", string reference = "", string log = "")
		{
			activity.LogType = Site.ConstantValues.LogFailure;
			activity.Log = log;
			activity.RelevantObject = relevantObject;
			activity.RelevantObjectReference = reference;
			activity.Status = "Failed";
			await this.Log(activity);

			return "Failed processing the request";
		}

		public async Task<string> LogError(ApplicationActivity activity, string relevantObject = "", string reference = "", string log = "")
		{
			activity.LogType = Site.ConstantValues.LogError;
			activity.Log = log;
			activity.RelevantObject = relevantObject;
			activity.RelevantObjectReference = reference;
			activity.Status = "Failed";
			await this.Log(activity);

			return "Failed processing the request";
		}

		public async Task<string> LogInvalidData(ApplicationActivity activity, string log)
		{
			activity.LogType = Site.ConstantValues.LogInvalidData;
			activity.Log = log;
			activity.Status = "Failed";
			await this.Log(activity);
			return "";
		}

		public async Task<string> LogInvalidAccess(ApplicationActivity activity, string relevantObject = "", string reference = "", string log = "")
		{
			activity.LogType = Site.ConstantValues.LogInvalidAccess;
			activity.Log = log;
			activity.RelevantObject = relevantObject;
			activity.RelevantObjectReference = reference;
			activity.Status = "Failed";
			await this.Log(activity);
			return "";
		}

		public async Task<string> LogException(ApplicationActivity activity, Exception exception, string log = null)
		{
			activity.LogType = Site.ConstantValues.LogException;
			activity.Log = (log != null ? log + ": " : "") + exception.Message.ToString();
			activity.Status = "Failed";
			await this.Log(activity);

			return "Error in processing or the request";
		}

		public async Task<string> LogUnknown(ApplicationActivity activity)
		{
			activity.LogType = Site.ConstantValues.LogUnknown;
			activity.Log = "Unknown process failure";
			activity.Status = "Failed";
			await this.Log(activity);

			return "Unknown process failure";
		}

		public async Task<string> LogServerActivity(ApplicationActivity activity, string log = "", bool success = true)
		{
			activity.LogType = Site.ConstantValues.LogUnknown;
			activity.Log = log;
			activity.Status = success ? "Success" : "Failed";
			await this.Log(activity);

			return "Server Activity";
		}

		public ApplicationActivity PopulateActivityInfo(ApplicationActivity activity)
		{
			//activity.SiteVersion = Site.SiteVersion.Version;
			activity.DateExcuted = DateTime.Now;

			if (httpContextAccessor != null && httpContextAccessor.HttpContext != null)
			{
				activity.IpAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
				activity.UserAgent = httpContextAccessor.HttpContext.Request.Headers.ContainsKey("User-Agent") ?
								httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString() : "";
				activity.Method = httpContextAccessor.HttpContext.Request.Method;
				activity.Path = httpContextAccessor.HttpContext.Request.Path;
				//For those who are curious if Referer is misspelled, it is not. Although Referrer is the correct english spelling of the word, they made the misspelling in the HTTP specification
				//https://english.stackexchange.com/questions/42630/referer-or-referrer/42636
				activity.Referrer = httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Referer") ?
								httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString() : "";

				if (httpContextAccessor.HttpContext.User != null)
				{

					activity.IdentityName = httpContextAccessor.HttpContext.User.Identity?.Name;
				}
				else
				{
					activity.IdentityName = "Context without User";
				}
			}
			else
			{
				activity.IdentityName = "Server";
			}

			return new ApplicationActivity()
			{
				SiteVersion = activity.SiteVersion,
				DateExcuted = activity.DateExcuted,
				IdentityName = activity.IdentityName,

				IpAddress = activity.IpAddress,
				UserAgent = activity.UserAgent,
				Method = activity.Method,
				Path = activity.Path,
				Referrer = activity.Referrer,

				LogType = activity.LogType,
				Controller = activity.Controller,
				Action = activity.Action,
				RelevantObject = activity.RelevantObject,
				RelevantObjectReference = activity.RelevantObjectReference,
				Status = activity.Status,
				Log = activity.Log
			};
		}
	}
}
