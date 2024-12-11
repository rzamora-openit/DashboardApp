using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Helpers.Interfaces
{
    public interface IPowerBIQueryHelper
    {
        Task<HttpResponseMessage> deleteQuery(string uri, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> deleteQuery(string apiRoute, string graphVersion = "v1.0", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> getQuery(string uri, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> getQuery(string apiRoute, string graphVersion = "v1.0", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> postQuery(string uri, object content, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> postQuery(string apiRoute, object content, string graphVersion = "v1.0", ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
    }
}