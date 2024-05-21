using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System;
using Microsoft.PowerBI.Api;
using Microsoft.Rest;
using System.Linq;
using OpeniT.PowerbiDashboardApp.ViewModels.PowerBI;
using Microsoft.PowerBI.Api.Models;

namespace OpeniT.PowerbiDashboardApp.Helpers
{
	public class PowerBIEmbedHelper
	{
		private readonly string apiVersion;
		private readonly string tenantName;
		private readonly string tenantId;
		private readonly string authUri;
		private readonly string clientId;
		private readonly string resourceId;
		private readonly string clientSecret;
		private readonly string apiUri;
		private readonly string resourceUri;
		private readonly string authenticationMode;
		private readonly string[] scope;

		//private readonly string urlPowerBiServiceApiRoot = "https://api.powerbi.com";

		public PowerBIEmbedHelper(IConfiguration config)
		{
			this.tenantName = config["Microsoft:TenantName"];
			this.resourceId = config["Microsoft:ResourceId"];


			this.apiVersion = config["PowerBI:ApiVersion"];
			this.apiUri = config["PowerBI:ApiUri"];
			this.resourceUri = config["PowerBI:ResourceUri"];

			this.tenantId = config["AzureAd:TenantId"];
			this.clientId = config["AzureAd:ClientId"];
			this.clientSecret = config["AzureAd:ClientSecret"];
			this.authenticationMode = config["AzureAd:AuthenticationMode"];
			this.authUri = config["AzureAd:AuthorityUri"];
			this.scope = config.GetSection("AzureAd:Scope").Get<string[]>();
		}

		/// <summary>
		/// Get Power BI client
		/// </summary>
		/// <returns>Power BI client object</returns>
		public PowerBIClient GetPowerBIClient()
		{
			var tokenCredentials = new TokenCredentials(GetAccessToken(), "Bearer");
			return new PowerBIClient(new Uri(apiUri), tokenCredentials);
		}

		/// <summary>
		/// Generates and returns Access token
		/// </summary>
		/// <returns>AAD token</returns>
		public string GetAccessToken()
		{
			AuthenticationResult authenticationResult = null;
			if (authenticationMode.Equals("masteruser", StringComparison.InvariantCultureIgnoreCase))
			{
				// Create a public client to authorize the app with the AAD app
				IPublicClientApplication clientApp = PublicClientApplicationBuilder.Create(clientId).WithAuthority(authUri).Build();
				var userAccounts = clientApp.GetAccountsAsync().Result;
				try
				{
					// Retrieve Access token from cache if available
					authenticationResult = clientApp.AcquireTokenSilent(scope, userAccounts.FirstOrDefault()).ExecuteAsync().Result;
				}
				catch (MsalUiRequiredException)
				{
					SecureString password = new SecureString();
					foreach (var key in Site.RunTimeStaticValues.ApplicantPortalPassword)
					{
						password.AppendChar(key);
					}
					authenticationResult = clientApp.AcquireTokenByUsernamePassword(scope, Site.RunTimeStaticValues.ApplicantPortalUsername, password).ExecuteAsync().Result;
				}
			}

			// Service Principal auth is the recommended by Microsoft to achieve App Owns Data Power BI embedding
			else if (authenticationMode.Equals("serviceprincipal", StringComparison.InvariantCultureIgnoreCase))
			{
				// For app only authentication, we need the specific tenant id in the authority url
				var tenantSpecificUrl = authUri.Replace("organizations", tenantId);

				// Create a confidential client to authorize the app with the AAD app
				IConfidentialClientApplication clientApp = ConfidentialClientApplicationBuilder
																				.Create(clientId)
																				.WithClientSecret(clientSecret)
																				.WithAuthority(tenantSpecificUrl)
																				.Build();
				// Make a client call if Access token is not available in cache
				authenticationResult = clientApp.AcquireTokenForClient(scope).ExecuteAsync().Result;
			}

			return authenticationResult.AccessToken;
		}

		/// <summary>
		/// Get embed params for a report
		/// </summary>
		/// <returns>Wrapper object containing Embed token, Embed URL, Report Id, and Report name for single report</returns>
		public EmbedParams GetEmbedParams(Guid workspaceId, Guid reportId, [Optional] Guid additionalDatasetId)
		{
			PowerBIClient pbiClient = this.GetPowerBIClient();

			// Get report info
			var pbiReport = pbiClient.Reports.GetReportInGroup(workspaceId, reportId);

			//  Check if dataset is present for the corresponding report
			//  If isRDLReport is true then it is a RDL Report 
			var isRDLReport = String.IsNullOrEmpty(pbiReport.DatasetId);

			EmbedToken embedToken;

			// Generate embed token for RDL report if dataset is not present
			if (isRDLReport)
			{
				// Get Embed token for RDL Report
				embedToken = GetEmbedTokenForRDLReport(workspaceId, reportId);
			}
			else
			{
				// Create list of datasets
				var datasetIds = new List<Guid>();

				// Add dataset associated to the report
				datasetIds.Add(Guid.Parse(pbiReport.DatasetId));

				// Append additional dataset to the list to achieve dynamic binding later
				if (additionalDatasetId != Guid.Empty)
				{
					datasetIds.Add(additionalDatasetId);
				}

				// Get Embed token multiple resources
				embedToken = GetEmbedToken(reportId, datasetIds, workspaceId);
			}

			// Add report data for embedding
			var embedReports = new List<EmbedReport>() {
				new EmbedReport
				{
					ReportId = pbiReport.Id, ReportName = pbiReport.Name, EmbedUrl = pbiReport.EmbedUrl
				}
			};

			// Capture embed params
			var embedParams = new EmbedParams
			{
				EmbedReport = embedReports,
				Type = "Report",
				EmbedToken = embedToken
			};

			return embedParams;
		}

		/// <summary>
		/// Get embed params for multiple reports for a single workspace
		/// </summary>
		/// <returns>Wrapper object containing Embed token, Embed URL, Report Id, and Report name for multiple reports</returns>
		/// <remarks>This function is not supported for RDL Report</remakrs>
		public EmbedParams GetEmbedParams(Guid workspaceId, IList<Guid> reportIds, [Optional] IList<Guid> additionalDatasetIds)
		{
			// Note: This method is an example and is not consumed in this sample app

			PowerBIClient pbiClient = this.GetPowerBIClient();

			// Create mapping for reports and Embed URLs
			var embedReports = new List<EmbedReport>();

			// Create list of datasets
			var datasetIds = new List<Guid>();

			// Get datasets and Embed URLs for all the reports
			foreach (var reportId in reportIds)
			{
				// Get report info
				var pbiReport = pbiClient.Reports.GetReportInGroup(workspaceId, reportId);

				datasetIds.Add(Guid.Parse(pbiReport.DatasetId));

				// Add report data for embedding
				embedReports.Add(new EmbedReport { ReportId = pbiReport.Id, ReportName = pbiReport.Name, EmbedUrl = pbiReport.EmbedUrl });
			}

			// Append to existing list of datasets to achieve dynamic binding later
			if (additionalDatasetIds != null)
			{
				datasetIds.AddRange(additionalDatasetIds);
			}

			// Get Embed token multiple resources
			var embedToken = GetEmbedToken(reportIds, datasetIds, workspaceId);

			// Capture embed params
			var embedParams = new EmbedParams
			{
				EmbedReport = embedReports,
				Type = "Report",
				EmbedToken = embedToken
			};

			return embedParams;
		}

		/// <summary>
		/// Get Embed token for single report, multiple datasets, and an optional target workspace
		/// </summary>
		/// <returns>Embed token</returns>
		/// <remarks>This function is not supported for RDL Report</remakrs>
		public EmbedToken GetEmbedToken(Guid reportId, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId)
		{
			PowerBIClient pbiClient = this.GetPowerBIClient();

			// Create a request for getting Embed token 
			// This method works only with new Power BI V2 workspace experience
			var tokenRequest = new GenerateTokenRequestV2(

				reports: new List<GenerateTokenRequestV2Report>() { new GenerateTokenRequestV2Report(reportId) },

				datasets: datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList(),

				targetWorkspaces: targetWorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId) } : null
			);

			// Generate Embed token
			var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

			return embedToken;
		}

		/// <summary>
		/// Get Embed token for multiple reports, datasets, and an optional target workspace
		/// </summary>
		/// <returns>Embed token</returns>
		/// <remarks>This function is not supported for RDL Report</remakrs>
		public EmbedToken GetEmbedToken(IList<Guid> reportIds, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId)
		{
			// Note: This method is an example and is not consumed in this sample app

			PowerBIClient pbiClient = this.GetPowerBIClient();

			// Convert report Ids to required types
			var reports = reportIds.Select(reportId => new GenerateTokenRequestV2Report(reportId)).ToList();

			// Convert dataset Ids to required types
			var datasets = datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList();

			// Create a request for getting Embed token 
			// This method works only with new Power BI V2 workspace experience
			var tokenRequest = new GenerateTokenRequestV2(

				datasets: datasets,

				reports: reports,

				targetWorkspaces: targetWorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId) } : null
			);

			// Generate Embed token
			var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

			return embedToken;
		}

		/// <summary>
		/// Get Embed token for multiple reports, datasets, and optional target workspaces
		/// </summary>
		/// <returns>Embed token</returns>
		/// <remarks>This function is not supported for RDL Report</remakrs>
		public EmbedToken GetEmbedToken(IList<Guid> reportIds, IList<Guid> datasetIds, [Optional] IList<Guid> targetWorkspaceIds)
		{
			// Note: This method is an example and is not consumed in this sample app

			PowerBIClient pbiClient = this.GetPowerBIClient();

			// Convert report Ids to required types
			var reports = reportIds.Select(reportId => new GenerateTokenRequestV2Report(reportId)).ToList();

			// Convert dataset Ids to required types
			var datasets = datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList();

			// Convert target workspace Ids to required types
			IList<GenerateTokenRequestV2TargetWorkspace> targetWorkspaces = null;
			if (targetWorkspaceIds != null)
			{
				targetWorkspaces = targetWorkspaceIds.Select(targetWorkspaceId => new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId)).ToList();
			}

			// Create a request for getting Embed token 
			// This method works only with new Power BI V2 workspace experience
			var tokenRequest = new GenerateTokenRequestV2(

				datasets: datasets,

				reports: reports,

				targetWorkspaces: targetWorkspaceIds != null ? targetWorkspaces : null
			);

			// Generate Embed token
			var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

			return embedToken;
		}

		/// <summary>
		/// Get Embed token for RDL Report
		/// </summary>
		/// <returns>Embed token</returns>
		public EmbedToken GetEmbedTokenForRDLReport(Guid targetWorkspaceId, Guid reportId, string accessLevel = "view")
		{
			PowerBIClient pbiClient = this.GetPowerBIClient();

			// Generate token request for RDL Report
			var generateTokenRequestParameters = new GenerateTokenRequest(
				accessLevel: accessLevel
			);

			// Generate Embed token
			var embedToken = pbiClient.Reports.GenerateTokenInGroup(targetWorkspaceId, reportId, generateTokenRequestParameters);

			return embedToken;
		}
	}
}
