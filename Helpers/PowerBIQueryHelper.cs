using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;
using OpeniT.PowerbiDashboardApp.Exceptions;
using OpeniT.PowerbiDashboardApp.ViewModels.PowerBI;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;

namespace OpeniT.PowerbiDashboardApp.Helpers
{
    public class PowerBIQueryHelper : IPowerBIQueryHelper
	{
		private readonly string apiVersion;
		private readonly string tenantName;
		private readonly string tenantId;
		private readonly string authString;
		private readonly string clientId;
		private readonly string resourceId;
		private readonly string clientSecret;
		private readonly string apiUri;
		private readonly string resourceUri;

		public PowerBIQueryHelper(IConfiguration config)
		{

			this.tenantName = config["Microsoft:TenantName"];
			this.tenantId = config["Microsoft:TenantId"];
			this.authString = config["Microsoft:Authority"];
			this.clientId = config["Microsoft:ClientId"];
			this.resourceId = config["Microsoft:ResourceId"];
			this.clientSecret = config["Microsoft:ClientSecret"];

			this.apiVersion = config["PowerBI:ApiVersion"];
			this.apiUri = config["PowerBI:ApiUri"];
			this.resourceUri = config["PowerBI:ResourceUri"];
		}

		private async Task<string> AcquireTokenAsync()
		{
			var clientCredential = new Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential(clientId, clientSecret);
			var authenticationContext = new AuthenticationContext(authString, false);
			var authenticationResult = await authenticationContext.AcquireTokenAsync(resourceUri, clientCredential);
			return authenticationResult.AccessToken;
		}

		private async Task<HttpClient> ComposeClient()
		{
			var client = new HttpClient();
			client.BaseAddress = new Uri(apiUri);
			client.DefaultRequestHeaders.Add("Authorization", "Bearer " + await AcquireTokenAsync());

			return client;

		}

		private string BuildQueryString(ICollection<QueryOption> queryOptions)
		{
			string query = "";
			if (queryOptions != null)
			{
				foreach (var queryOption in queryOptions)
				{
					query += string.IsNullOrEmpty(query) ? "?" : "&";
					query += BuildQueryString(queryOption);
				}
			}
			return query;
		}

		private string BuildQueryString(QueryOption queryOption)
		{
			return $"{queryOption.Key}={queryOption.Value}";
		}

		public async Task<HttpResponseMessage> getQuery(string apiRoute, string graphVersion = "v1.0", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var uri = $"{graphVersion}/{tenantName}/{apiRoute}{BuildQueryString(options)}";
			return await getQuery(uri, cancellationToken);
		}

		public async Task<HttpResponseMessage> getQuery(string uri, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				using (var client = await ComposeClient())
				{

					var result = await client.GetAsync(uri, cancellationToken);

					if (!result.IsSuccessStatusCode)
					{
						var errorResult = JObject.Parse(result.Content.ReadAsStringAsync().Result).Value<JObject>("error");
						throw new PowerBIException("Post Query Failed")
						{
							Error = errorResult.ToObject<ErrorResponse>()
						};
					}

					if (cancellationToken.IsCancellationRequested)
					{
						return null;
					}

					return result;
				}
			}
			catch (AuthenticationException ex)
			{
				//await logger.Log($"ERROR: AuthenticationException at {nameof(AzureQueryHelper)}.deleteQuery.{uri} : {ex.Message}");
				throw;
			}
			catch (PowerBIException ex)
			{
				throw;
			}
			catch (Exception ex)
			{
				//await logger.Log($"ERROR: Exception at {nameof(AzureQueryHelper)}.deleteQuery.{uri}  : {ex.Message}");
				throw;
			}
			return null;
		}

		public async Task<HttpResponseMessage> postQuery(string apiRoute, object content, string graphVersion = "v1.0", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var uri = $"{graphVersion}/{tenantName}/{apiRoute}{BuildQueryString(options)}";
			return await postQuery(uri, content, cancellationToken);
		}

		public async Task<HttpResponseMessage> postQuery(string uri, object content, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				using (var client = await ComposeClient())
				{
					var serializedContent = JsonConvert.SerializeObject(
							content,
							new JsonSerializerSettings
							{
								ContractResolver = new CamelCasePropertyNamesContractResolver(),
								NullValueHandling = NullValueHandling.Ignore
							}
						);

					var stringContent = new StringContent(serializedContent, Encoding.UTF8, "application/json");
					var result = await client.PostAsync(uri, stringContent, cancellationToken);
					if (!result.IsSuccessStatusCode)
					{
						var errorResult = JObject.Parse(result.Content.ReadAsStringAsync().Result).Value<JObject>("error");
						throw new PowerBIException("Post Query Failed")
						{
							Error = errorResult.ToObject<ErrorResponse>()
						};
					}

					if (cancellationToken.IsCancellationRequested)
					{
						return null;
					}

					return result;
				}
			}
			catch (AuthenticationException ex)
			{
				//await logger.Log($"ERROR: AuthenticationException at {nameof(AzureQueryHelper)}.postQuery.{uri} : {ex.Message}");
				throw;
			}
			catch (PowerBIException ex)
			{
				throw;
			}
			catch (Exception ex)
			{
				//await logger.Log($"ERROR: Exception at {nameof(AzureQueryHelper)}.postQuery.{uri}  : {ex.Message}");
				throw;
			}
			return null;
		}

		public async Task<HttpResponseMessage> deleteQuery(string apiRoute, string graphVersion = "v1.0", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var uri = $"{graphVersion}/{tenantName}/{apiRoute}{BuildQueryString(options)}";
			return await deleteQuery(uri, cancellationToken);
		}

		public async Task<HttpResponseMessage> deleteQuery(string uri, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				using (var client = await ComposeClient())
				{
					var result = await client.DeleteAsync(uri, cancellationToken);

					if (!result.IsSuccessStatusCode)
					{
						var errorResult = JObject.Parse(result.Content.ReadAsStringAsync().Result).Value<JObject>("error");
						throw new PowerBIException("Post Query Failed")
						{
							Error = errorResult.ToObject<ErrorResponse>()
						};
					}

					if (cancellationToken.IsCancellationRequested)
					{
						return null;
					}

					return result;
				}
			}
			catch (AuthenticationException ex)
			{
				//await logger.Log($"ERROR: AuthenticationException at {nameof(AzureQueryHelper)}.deleteQuery.{uri} : {ex.Message}");
				throw;
			}
			catch (PowerBIException ex)
			{
				throw;
			}
			catch (Exception ex)
			{
				//await logger.Log($"ERROR: Exception at {nameof(AzureQueryHelper)}.deleteQuery.{uri}  : {ex.Message}");
				throw;
			}
			return null;
		}
	}
}
