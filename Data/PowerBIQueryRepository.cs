using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using OpeniT.PowerbiDashboardApp.ViewModels.PowerBI;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Data
{
	public class PowerBIQueryRepository : IPowerBIQueryRepository
	{
		private readonly IPowerBIQueryHelper powerBIQueryHelper;
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

		public PowerBIQueryRepository(IPowerBIQueryHelper powerBIQueryHelper,
			IConfiguration config)
		{
			this.powerBIQueryHelper = powerBIQueryHelper;
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


		#region DataSet

		public async Task<JObject> RefreshExecutionDetail(string datasetId, string groupId, string refreshId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"groups/{groupId}/datasets/{datasetId}/refreshes/{refreshId}";
			var response = await this.powerBIQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "v1.0",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
			var jObject = JObject.Parse(responseString);
			return jObject;
		}

		public async Task<string> RefreshDataSet(string datasetId, string groupId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var content = new DatasetRefreshRequest()
			{
				NotifyOption = "NoNotification"
			};

			var apiRoute = $"groups/{groupId}/datasets/{datasetId}/refreshes";
			var response = await this.powerBIQueryHelper.postQuery(
				apiRoute: apiRoute,
				content: content,
				graphVersion: "v1.0",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			return response.ReasonPhrase;
		}

		public async Task<List<Refresh>> RefreshHistory(string datasetId, string groupId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var apiRoute = $"groups/{groupId}/datasets/{datasetId}/refreshes";
			var response = await this.powerBIQueryHelper.getQuery(
				apiRoute: apiRoute,
				graphVersion: "v1.0",
				options: options,
				cancellationToken: cancellationToken
			);

			if (response == null)
			{
				return null;
			}

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
			var jArray = JObject.Parse(responseString).Value<JArray>("value");
			var refreshes = jArray.ToObject<List<Refresh>>();
			return refreshes;
		}
		#endregion DataSet
	}
}
