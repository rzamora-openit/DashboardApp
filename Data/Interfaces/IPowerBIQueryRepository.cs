using Newtonsoft.Json.Linq;
using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using OpeniT.PowerbiDashboardApp.ViewModels.PowerBI;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Data.Interfaces
{
    public interface IPowerBIQueryRepository
    {
        Task<string> RefreshDataSet(string datasetId, string groupId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<JObject> RefreshExecutionDetail(string datasetId, string groupId, string refreshId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
        Task<List<Refresh>> RefreshHistory(string datasetId, string groupId, ICollection<QueryOption> options = null, CancellationToken cancellationToken = default);
    }
}