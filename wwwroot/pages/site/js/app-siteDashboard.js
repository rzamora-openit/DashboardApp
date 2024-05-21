// app-siteDashboard.js

(function () {
	"use strict";

	var siteApp = angular.module("app-site");

	siteApp.factory("powerbiAPI", function (httpControl) {
		var service = {};

		service.getPowerbiReferences = function () {
			var serviceName = "service.powerbiAPI.getPowerbiReferences";
			var url = "/api/dashboard/powerbiReference";
			var payload = {};

			return httpControl.get(serviceName, url, payload);
		};

		service.getPowerbiReference = function (id) {
			var serviceName = "service.powerbiAPI.getPowerbiReference";
			var url = "/api/dashboard/powerbiReference/" + id;
			var payload = {};

			return httpControl.get(serviceName, url, payload);
		};

		service.save = function (model) {
			var serviceName = "service.powerbiAPI.save";
			var url = "/api/dashboard/powerbiReference";
			var payload = model;

			return httpControl.post(serviceName, url, payload);
		}

		service.update = function (id, model) {
			var serviceName = "service.powerbiAPI.update";
			var url = "/api/dashboard/powerbiReference/" + id;
			var payload = model;

			return httpControl.put(serviceName, url, payload);
		}

		service.delete = function (id) {
			var serviceName = "service.powerbiAPI.delete";
			var url = "/api/dashboard/powerbiReference/" + id;
			var payload = {};

			return httpControl.delete(serviceName, url, payload);
		}

		return service;
	});

	siteApp.factory("powerbiAccessAPI", function (httpControl) {
		var service = {};

		service.getRefreshHistory = function (groupId, datasetId) {
			var serviceName = "service.powerbiAccessAPI.getHistory";
			var url = "/api/dashboard/powerbiAccess/group/" + groupId + "/dataset/" + datasetId + "/history";
			var payload = {};

			return httpControl.get(serviceName, url, payload);
		};

		service.getEmbedInfo = function (groupId, reportId) {
			var serviceName = "service.powerbiAccessAPI.getEmbedInfo";
			var url = "/api/dashboard/powerbiAccess/embedinfo/?groupId=" + groupId + "&reportId=" + reportId;
			var payload = {};

			return httpControl.get(serviceName, url, payload);
		};

		service.refresh = function (groupId, datasetId) {
			var serviceName = "service.powerbiAccessAPI.refresh";
			var url = "/api/dashboard/powerbiAccess/group/" + groupId + "/dataset/" + datasetId + "/refresh";
			var payload = {};

			return httpControl.get(serviceName, url, payload);
		};

		return service;
	});
})();