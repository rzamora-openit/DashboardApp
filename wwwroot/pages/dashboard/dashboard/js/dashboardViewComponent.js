// dashboardViewComponent.js

(function () {
	"use strict";

	angular.module("app-dashboard").component("dashboardViewComponent", {
		templateUrl: "/pages/dashboard/dashboard/views/dashboardViewView.html",
		controller: dashboardViewController,
		bindings: {
			stateContext: '='
		}
	});

	function dashboardViewController($q, $routeParams, stateUtil, powerbiAPI, powerbiAccessAPI, Notification) {

		var ctrl = this;
		ctrl.isBusy = false;
		ctrl.componentContext = new stateUtil.stateContext();	

		ctrl.powerbiReference = {};

		ctrl.refreshHistory = [];
		ctrl.lastRefresh = null;
		ctrl.pendingRefresh = null;

		ctrl.refreshing = false;

		ctrl.$onInit = function () {
			ctrl.componentContext.setState("loading");

			ctrl.powerbiReferenceId = $routeParams.sectionId;

			ctrl.getPowerbiReference()	
		};

		ctrl.getPowerbiReference = function () {
			powerbiAPI
				.getPowerbiReference(ctrl.powerbiReferenceId)
				.callbacks(
					//Success Callback
					function (successData) {
						ctrl.powerbiReference = successData;

						ctrl.stateContext.breadcrumb = successData.name;

						ctrl.getRefreshHistory();
					},
					//Error Callback
					function (errorResult) {
						ctrl.stateContext.setState('invalidActivity');
						Notification.error("Failed to get reference");
					},
					//Finally Callback
					function () {
					}
				);
        }

		ctrl.getRefreshHistory = function () {
			powerbiAccessAPI
				.getRefreshHistory(ctrl.powerbiReference.workGroupId, ctrl.powerbiReference.dataSetId)
				.callbacks(
					//Success Callback
					function (successData) {
						ctrl.refreshHistory = successData;
						
						ctrl.refreshHistory.forEach(function (refresh) {
							if (refresh.status == "Completed") {
								if (!ctrl.lastRefresh ||
									!ctrl.lastRefresh.endTime ||
									ctrl.lastRefresh.endTime < refresh.endTime) {
									ctrl.lastRefresh = refresh;
								}
							} else if (refresh.status == "Unknown") {
								ctrl.pendingRefresh = refresh;
							}
						});

						ctrl.componentContext.setState("loaded");
					},
					//Error Callback
					function (errorResult) {
						ctrl.stateContext.setState('invalidActivity');
					},
					//Finally Callback
					function () {
					}
			);
		}

		ctrl.getEmbedInfo = function () {
			powerbiAccessAPI
				.getEmbedInfo(ctrl.powerbiReference.workGroupId, ctrl.powerbiReference.reportId)
				.callbacks(
					//Success Callback
					function (successData) {
						var models = window["powerbi-client"].models;
						var reportContainer = angular.element("#report-container")[0];

						var reportLoadConfig = {
							type: "report",
							tokenType: models.TokenType.Embed,
							accessToken: successData.EmbedToken.Token,
							embedUrl: successData.EmbedReport[0].EmbedUrl
						};

						var report = powerbi.embed(reportContainer, reportLoadConfig);

						report.off("loaded");
						report.on("loaded", function () {
							console.log("Report is loaded!");
						});

						report.off("rendered");
						report.on("rendered", function () {
							console.log("Report render successful");
						});

						report.off("error");
						report.on("error", function (event) {
							var errorMsg = event.detail;
							console.error(errorMsg);
							return;
						});
					},
					//Error Callback
					function (errorResult) {
						ctrl.stateContext.setState('invalidActivity');
					},
					//Finally Callback
					function () {
					}
			);
        }

		ctrl.refresh = function () {
			ctrl.isBusy = true;

			powerbiAccessAPI
				.refresh(ctrl.powerbiReference.workGroupId, ctrl.powerbiReference.dataSetId)
				.callbacks(
					//Success Callback
					function (successData) {
						ctrl.refreshing = true;
						Notification.success("Dataset refreshing. Please wait couple of minutes for the data refresh to reflect.");
					},
					//Error Callback
					function (errorResult) {
						//ctrl.stateContext.setState("invalidActivity");
						Notification.error("Failed to refresh dashboard. Something went wrong");
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}
	}

})();