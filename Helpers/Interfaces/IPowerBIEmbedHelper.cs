using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using OpeniT.PowerbiDashboardApp.ViewModels.PowerBI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpeniT.PowerbiDashboardApp.Helpers.Interfaces
{
    public interface IPowerBIEmbedHelper
    {
        string GetAccessToken();
        EmbedParams GetEmbedParams(Guid workspaceId, Guid reportId, [Optional] Guid additionalDatasetId);
        EmbedParams GetEmbedParams(Guid workspaceId, IList<Guid> reportIds, [Optional] IList<Guid> additionalDatasetIds);
        EmbedToken GetEmbedToken(Guid reportId, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId);
        EmbedToken GetEmbedToken(IList<Guid> reportIds, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId);
        EmbedToken GetEmbedToken(IList<Guid> reportIds, IList<Guid> datasetIds, [Optional] IList<Guid> targetWorkspaceIds);
        EmbedToken GetEmbedTokenForRDLReport(Guid targetWorkspaceId, Guid reportId, string accessLevel = "view");
        PowerBIClient GetPowerBIClient();
    }
}