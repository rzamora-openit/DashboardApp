// dashboardTableComponent.js

(function () {
	"use strict";

	angular.module("app-dashboard").component("dashboardTableComponent", {
		templateUrl: "/pages/dashboard/dashboard/views/dashboardTableView.html",
		controller: dashboardTableController,
		bindings: {
			stateContext: '='
		}
	});

	function dashboardTableController($q, powerbiAPI, stateUtil, Notification) {

		var ctrl = this;
		ctrl.isBusy = false;
		ctrl.componentContext = new stateUtil.stateContext();			

		ctrl.powerbiReferences = [];

		ctrl.addModel = {};
		ctrl.editModel = {};
		ctrl.editModelRef = {};
		ctrl.deleteModelTarget = {};
		
		ctrl.$onInit = function () {
			ctrl.componentContext.setState("loading");

			$q.all([ctrl.getPowerbiReferences()])
				.then(function () {
					ctrl.componentContext.setState("loaded");
                });		
		};

		ctrl.getPowerbiReferences = function () {
			var deferred = $q.defer();

			powerbiAPI
				.getPowerbiReferences()
				.callbacks(
					//Success Callback
					function (successData) {
						ctrl.powerbiReferences = successData;

						deferred.resolve();
					},
					//Error Callback
					function (errorResult) {
						Notification.error("Failed to get PowerBI References");

						ctrl.stateContext.setState('invalidActivity');
						deferred.reject();
					},
					//Finally Callback
					function () {
					}
				);

			return deferred.promise;
		}

		ctrl.addInit = function () {
			ctrl.addModel = {};
		}

		ctrl.editInit = function (powerbiReference) {
			ctrl.editModel = angular.copy(powerbiReference);
			ctrl.editModelRef = angular.copy(powerbiReference);
		}

		ctrl.deleteInit = function (powerbiReference) {
			ctrl.deleteModelTarget = angular.copy(powerbiReference);
		}

		ctrl.save = function () {
			ctrl.isBusy = true;

			powerbiAPI
				.save(ctrl.addModel)
				.callbacks(
					//Success Callback
					function (successData) {
						ctrl.powerbiReferences.push(successData)

						ctrl.addModel = {};
						angular.element("#addModal").modal("hide");

						Notification.success("Successfully added reference");

					},
					//Error Callback
					function (errorResult) {
						Notification.error("Failed to add reference");
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.update = function () {
			ctrl.isBusy = true;

			powerbiAPI
				.update(ctrl.editModel.id, ctrl.editModel)
				.callbacks(
					//Success Callback
					function (successData) {
						var powerbiReference = ctrl.powerbiReferences.find(x => x.id == successData.id);
						if (powerbiReference) {
							powerbiReference.name = successData.name;
							powerbiReference.workGroupId = successData.workGroupId;
							powerbiReference.dataSetId = successData.dataSetId;
							powerbiReference.reportId = successData.reportId;
						}

						ctrl.editModel = {};
						ctrl.editModelRef = {};
						angular.element("#editModal").modal("hide");

						Notification.success("Successfully updated reference");
					},
					//Error Callback
					function (errorResult) {
						Notification.error("Failed to update reference");
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.delete = function () {
			ctrl.isBusy = true;

			powerbiAPI
				.delete(ctrl.deleteModelTarget.id)
				.callbacks(
					//Success Callback
					function () {
						var idx = ctrl.powerbiReferences.indexOf(ctrl.deleteModelTarget);
						ctrl.powerbiReferences.splice(idx, 1)

						ctrl.deleteModelTarget = {};
						angular.element("#deleteModal").modal("hide");

						Notification.success("Successfully deleted reference");
					},
					//Error Callback
					function (errorResult) {
						Notification.error("Failed to delete reference");
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.notChanged = function (ref, model) {
			return angular.equals(ref, model);
		}

	}

})();