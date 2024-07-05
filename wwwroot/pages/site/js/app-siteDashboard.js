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

	siteApp.factory("rolesAPI", function (httpControl) {
		var service = {};

		service.get = function () {
			var serviceName = "service.rolesAPI.get";
			var url = "/api/dashboard/azureAccess/roles";
			var payload = {};

			return httpControl.get(serviceName, url, payload);
		};

		service.save = function (name, description) {
			var serviceName = "service.rolesAPI.save";
			var url = "/api/dashboard/azureAccess/roles/" + name + "/" + description;
			var payload = {};

			return httpControl.post(serviceName, url, payload);
		};

		service.disable = function (name) {
			var serviceName = "service.rolesAPI.disable";
			var url = "/api/dashboard/azureAccess/roles/disable/" + name;
			var payload = {};

			return httpControl.put(serviceName, url, payload);
		};

		service.enable = function (name) {
			var serviceName = "service.rolesAPI.enable";
			var url = "/api/dashboard/azureAccess/roles/enable/" + name;
			var payload = {};

			return httpControl.put(serviceName, url, payload);
		};

		service.delete = function (name) {
			var serviceName = "service.rolesAPI.save";
			var url = "/api/dashboard/azureAccess/roles/delete/" + name;
			var payload = {};

			return httpControl.delete(serviceName, url, payload);
		};

		service.getRoleAssignments = function () {
			var serviceName = "service.rolesAPI.getRoleAssignments";
			var url = "/api/dashboard/azureAccess/roles/assignments";
			var payload = {};

			return httpControl.get(serviceName, url, payload);
		};

		service.assignRole = function (userId, roleId) {
			var serviceName = "service.rolesAPI.assignRole";
			var url = "/api/dashboard/azureAccess/" + userId + "/roles/" + roleId;
			var payload = {};

			return httpControl.post(serviceName, url, payload);
		};

		service.removeAssignment = function (userId, roleAssignnmentId) {
			var serviceName = "service.rolesAPI.removeAssignment";
			var url = "/api/dashboard/azureAccess/" + userId + "/appRoleAssignments/" + roleAssignnmentId;
			var payload = {};

			return httpControl.delete(serviceName, url, payload);
		};

		return service;
	});

	siteApp.factory("featureAccessAPI", function (httpControl) {
		var service = {};

		service.get = function () {
			var serviceName = "service.featureAccessAPI.get";
			var url = "/api/dashboard/featureAccess";
			var payload = {};

			return httpControl.get(serviceName, url, payload);
		};

		service.getFeatureAccess = function (featureName) {
			var serviceName = "service.featureAccessAPI.getFeatureAccess";
			var url = "/api/dashboard/featureAccess/" + featureName;
			var payload = {};

			return httpControl.get(serviceName, url, payload);
		};

		service.addAccess = function (featureAccessId, model) {
			var serviceName = "service.featureAccessAPI.addAccess";
			var url = "/api/dashboard/featureAccess/access/" + featureAccessId;
			var payload = {
				type: model.type,
				reference: model.reference,
				azureId: model.azureId,
				level: model.level
			};

			return httpControl.post(serviceName, url, payload);
		};

		service.deleteAccess = function (featureAccessId, accessId) {
			var serviceName = "service.featureAccessAPI.deleteAccess";
			var url = "/api/dashboard/featureAccess/access/" + featureAccessId + "/" + accessId;
			var payload = {};

			return httpControl.delete(serviceName, url, payload);
		};

		return service;
	});
})();