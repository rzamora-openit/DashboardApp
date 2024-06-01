using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpeniT.PowerbiDashboardApp.Data;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Models.Application;
using OpeniT.PowerbiDashboardApp.Models.Objects;
using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using OpeniT.PowerbiDashboardApp.ViewModels.PowerBI;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Controllers.Api.Dashboard
{
    [Authorize(Policy = "Internal")]
    [ApiController]
    [Route("api/dashboard/[controller]")]
    [ValidateAntiForgeryToken]
    public class PowerbiAccessController : ControllerBase
    {
        private string ControllerName = "api/dashboard" + nameof(PowerbiAccessController);

        private readonly DataRepository dataRepository;
        private readonly PowerBIQueryRepository powerBIQuery;
        private readonly PowerBIEmbedHelper powerBIEmbedHelper;
        private readonly ApplicationLogger logger;

        public PowerbiAccessController(DataRepository dataRepository,
                        PowerBIQueryRepository powerBIQuery,
                        PowerBIEmbedHelper powerBIEmbedHelper,
                        ApplicationLogger logger)
        {
            this.dataRepository = dataRepository;
            this.powerBIQuery = powerBIQuery;
            this.powerBIEmbedHelper = powerBIEmbedHelper;
            this.logger = logger;
        }

        [HttpGet("group/{groupId}/dataset/{datasetId}/history")]
        public async Task<IActionResult> GetHistory(string groupId, string datasetId)
        {
            var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Get/{groupId}/{datasetId}" };

            try
            {
                var powerbiReference = await this.dataRepository.GetPowerbiReferenceByGroupIdDatasetId(groupId, datasetId);
                if (powerbiReference == null)
                {
                    var log = await this.logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{groupId}/{datasetId}", log: "Not found"); ;
                    return this.BadRequest(log);
                }

                var queryOptions = new List<QueryOption>();
                queryOptions.Add(new QueryOption() { Key = "$top", Value = "10" });

                var results = await this.powerBIQuery.RefreshHistory(datasetId: datasetId, groupId: groupId, options: queryOptions);

                await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), $"{groupId}/{datasetId}", log: $"Get success");
                return this.Ok(results);
            }
            catch (Exception ex)
            {
                await logger.LogException(activity, ex);
                return this.BadRequest("Error in proccessing the request.");
            }

            await logger.LogUnknown(activity);
            return this.BadRequest("Unknown proccess failure.");
        }

        [HttpGet("group/{groupId}/dataset/{datasetId}/refresh")]
        public async Task<IActionResult> Refresh(string groupId, string datasetId)
        {
            var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"Get/{groupId}/{datasetId}" };

            try
            {
                var powerbiReference = await this.dataRepository.GetPowerbiReferenceByGroupIdDatasetId(groupId, datasetId);
                if (powerbiReference == null)
                {
                    var log = await this.logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{groupId}/{datasetId}", log: "Not found"); ;
                    return this.BadRequest(log);
                }

                var result = await this.powerBIQuery.RefreshDataSet(datasetId: datasetId, groupId: groupId);
                if (result == null)
                {
                    var log = await logger.LogFailure(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{groupId}/{datasetId}", log: $"Failed");
                    return this.BadRequest(log);
                }

                await logger.LogDataAccess(activity: activity, relevantObject: nameof(PowerbiReference), $"{groupId}/{datasetId}", log: $"Success");
                return this.Ok(result);
            }
            catch (Exception ex)
            {
                await logger.LogException(activity, ex);
                return this.BadRequest("Error in proccessing the request.");
            }

            await logger.LogUnknown(activity);
            return this.BadRequest("Unknown proccess failure.");
        }

        [HttpGet("embedinfo")]
        public async Task<IActionResult> GetEmbedInfo([FromQuery] string groupId, [FromQuery] string reportId)
        {
            var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"GetEmbedInfo/{groupId}/{reportId}" };

            try
            {
                #region ServerSide Validation
                if (!this.ModelState.IsValid)
                {
                    var log = await logger.LogInvalidData(activity, "Invalid model");
                    return this.BadRequest(log);
                }
                #endregion ServerSide Validation

                var powerbiReference = await this.dataRepository.GetPowerbiReferenceByGroupIdReportId(groupId, reportId);
                if (powerbiReference == null)
                {
                    var log = await this.logger.LogInvalidAccess(activity: activity, relevantObject: nameof(PowerbiReference), reference: $"{groupId}/{reportId}", log: "Not found"); ;
                    return this.BadRequest(log);
                }

                var embedParams = powerBIEmbedHelper.GetEmbedParams(new Guid(groupId), new Guid(reportId));
                if (embedParams == null)
                {
                    var log = await this.logger.LogInvalidAccess(activity: activity, relevantObject: nameof(EmbedParams), reference: $"{groupId}/{reportId}", log: "Failed to get embed params"); ;
                    return this.BadRequest(log);
                }

                await logger.LogDataAccess(activity: activity, relevantObject: nameof(EmbedParams), $"{groupId}/{reportId}", log: $"Success");
                return this.Ok(JsonSerializer.Serialize<EmbedParams>(embedParams));
            }
            catch (Exception ex)
            {
                var log = await logger.LogException(activity, ex);
                return this.BadRequest(log);
            }

            var logUnknown = await logger.LogUnknown(activity);
            return this.BadRequest(logUnknown);
        }
    }
}
