using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Security.Authentication;
using System;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Text;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using OpeniT.PowerbiDashboardApp.Exceptions;

namespace OpeniT.PowerbiDashboardApp.Helpers
{
	public class AzureQueryHelper
	{
		private readonly string apiVersion;
		private readonly string tenantName;
		private readonly string tenantId;
		private readonly string authString;
		private readonly string clientId;
		private readonly string resourceId;
		private readonly string clientSecret;
		private readonly string graphUri;

		public AzureQueryHelper(IConfiguration config)
		{
			this.apiVersion = config["Microsoft:GraphApiVersion"];
			this.tenantName = config["Microsoft:TenantName"];
			this.tenantId = config["Microsoft:TenantId"];
			this.authString = config["Microsoft:Authority"];
			this.clientId = config["Microsoft:ClientId"];
			this.resourceId = config["Microsoft:ResourceId"];
			this.clientSecret = config["Microsoft:ClientSecret"];
			this.graphUri = config["Microsoft:GraphUri"];
		}

		private async Task<string> AcquireTokenAsync()
		{
			var clientCredential = new ClientCredential(clientId, clientSecret);
			var authenticationContext = new AuthenticationContext(authString, false);
			var authenticationResult = await authenticationContext.AcquireTokenAsync(graphUri, clientCredential);
			return authenticationResult.AccessToken;
		}

		private async Task<HttpClient> ComposeClient()
		{
			var client = new HttpClient();
			client.BaseAddress = new Uri(graphUri);
			client.DefaultRequestHeaders.Add("Authorization", "Bearer " + await AcquireTokenAsync());
			client.DefaultRequestHeaders.Add("ConsistencyLevel", "eventual");

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

		public async Task<HttpResponseMessage> getQuery(string apiRoute, string graphVersion = "beta", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
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
						throw new MicrosoftGraphException($"Get Query Failed: {uri}")
						{
							Error = errorResult.ToObject<Error>()
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
			catch (MicrosoftGraphException ex)
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

		public async Task<HttpResponseMessage> postQuery(string apiRoute, object content, string graphVersion = "beta", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
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
						throw new MicrosoftGraphException($"Post Query Failed: {uri}")
						{
							Error = errorResult.ToObject<Error>()
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
			catch (MicrosoftGraphException ex)
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

		public async Task<HttpResponseMessage> putStreamQuery(string apiRoute, System.IO.Stream stream, string graphVersion = "beta", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var uri = $"{graphVersion}/{tenantName}/{apiRoute}{BuildQueryString(options)}";
			return await putStreamQuery(uri, stream, cancellationToken);
		}

		public async Task<HttpResponseMessage> putStreamQuery(string uri, System.IO.Stream stream, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				using (var client = await ComposeClient())
				{
					var result = await client.PutAsync(uri, new StreamContent(stream), cancellationToken);

					if (!result.IsSuccessStatusCode)
					{
						var errorResult = JObject.Parse(result.Content.ReadAsStringAsync().Result).Value<JObject>("error");
						throw new MicrosoftGraphException($"Put Stream Query Failed: {uri}")
						{
							Error = errorResult.ToObject<Error>()
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
			catch (MicrosoftGraphException ex)
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

		public async Task<HttpResponseMessage> putUploadSessionQuery(string uploadUrl, int fileSize, System.IO.Stream stream, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				var client = new HttpClient();

				var streamContent = new StreamContent(stream);
				streamContent.Headers.Add("Content-Length", fileSize.ToString());
				streamContent.Headers.Add("Content-Range", $"bytes 0-{fileSize - 1}/{fileSize}");

				using (client)
				{
					var result = await client.PutAsync(uploadUrl, streamContent, cancellationToken);

					if (!result.IsSuccessStatusCode)
					{
						var errorResult = JObject.Parse(result.Content.ReadAsStringAsync().Result).Value<JObject>("error");
						throw new MicrosoftGraphException($"Upload Session Query Failed: {uploadUrl}")
						{
							Error = errorResult.ToObject<Error>()
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
			catch (MicrosoftGraphException ex)
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

		public async Task<HttpResponseMessage> patchQuery(string apiRoute, object content, string graphVersion = "beta", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var uri = $"{graphVersion}/{tenantName}/{apiRoute}{BuildQueryString(options)}";

			return await patchQuery(uri, content, cancellationToken);
		}

		public async Task<HttpResponseMessage> patchQuery(string uri, object content, CancellationToken cancellationToken = default(CancellationToken))
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
					var result = await client.PatchAsync(uri, stringContent, cancellationToken);
					if (!result.IsSuccessStatusCode)
					{
						var errorResult = JObject.Parse(result.Content.ReadAsStringAsync().Result).Value<JObject>("error");
						throw new MicrosoftGraphException($"Patch Query Failed: {uri}")
						{
							Error = errorResult.ToObject<Error>()
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
			catch (MicrosoftGraphException ex)
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

		public async Task<HttpResponseMessage> deleteQuery(string apiRoute, string graphVersion = "beta", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default(CancellationToken))
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
						throw new MicrosoftGraphException($"Failed to delete azure query: {uri}")
						{
							Error = errorResult.ToObject<Error>()
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
			catch (MicrosoftGraphException ex)
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
