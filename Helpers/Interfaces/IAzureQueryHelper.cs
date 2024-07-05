using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Helpers.Interfaces
{
    public interface IAzureQueryHelper
    {
        Task<HttpResponseMessage> deleteQuery(string uri, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> deleteQuery(string apiRoute, string graphVersion = "beta", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> getQuery(string uri, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> getQuery(string apiRoute, string graphVersion = "beta", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> patchQuery(string uri, object content, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> patchQuery(string apiRoute, object content, string graphVersion = "beta", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> postQuery(string uri, object content, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> postQuery(string apiRoute, object content, string graphVersion = "beta", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> putStreamQuery(string uri, Stream stream, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> putStreamQuery(string apiRoute, Stream stream, string graphVersion = "beta", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> putUploadSessionQuery(string uploadUrl, int fileSize, Stream stream, CancellationToken cancellationToken = default);
    }
}