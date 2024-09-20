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

	function dashboardTableController($q, powerbiAPI, stateUtil, Notification, requestVerificationToken) {

		var ctrl = this;
		ctrl.isBusy = false;
		ctrl.componentContext = new stateUtil.stateContext();			

		ctrl.powerbiReferences = [];

		ctrl.addModel = {};
		ctrl.editModel = {};
		ctrl.editModelRef = {};
		ctrl.deleteModelTarget = {};
		ctrl.shareModelTarget = {};
		ctrl.shareToUsers = [];
		ctrl.shareToGroups = [];

		ctrl.invalidShare = false;
		
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

		ctrl.shareUserInit = function (powerbiReference) {
			ctrl.shareToUsers = [];

			ctrl.initUserSelect();

			ctrl.shareModelTarget = angular.copy(powerbiReference);
		}

		ctrl.shareGroupInit = function (powerbiReference) {
			ctrl.shareToGroups = [];

			ctrl.initGroupSelect();

			ctrl.shareModelTarget = angular.copy(powerbiReference);
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

		ctrl.shareUsers = function (reference, users) {
			if (!users.length) {
				Notification.error("Invalid action");
				return;
			}

			ctrl.isBusy = true;

			powerbiAPI
				.userShare(reference.id, users)
				.callbacks(
					//Success Callback
					function (successData) {
						var powerbiReference = ctrl.powerbiReferences.find(x => x.id == reference.id);
						if (powerbiReference) {
							powerbiReference.sharing = angular.copy(successData.sharing);
						}

						ctrl.shareModelTarget.sharing = angular.copy(successData.sharing);

						$("#user-select").val([]).trigger('change');

						Notification.success("Successfully shared dashboard");
					},
					//Error Callback
					function (errorResult) {
						Notification.error(errorResult);
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.removeUserShare = function (reference, user) {
			ctrl.isBusy = true;

			var users = [user];

			powerbiAPI
				.removeUserShare(reference.id, users)
				.callbacks(
					//Success Callback
					function (successData) {
						var powerbiReference = ctrl.powerbiReferences.find(x => x.id == successData.id);
						if (powerbiReference) {
							powerbiReference.sharing = angular.copy(successData.sharing);
						}

						ctrl.shareModelTarget.sharing = angular.copy(successData.sharing);

						Notification.success("Successfully removed sharing");
					},
					//Error Callback
					function (errorResult) {
						Notification.error(errorResult);
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.groupShare = function (reference, groups) {
			if (!groups.length) {
				Notification.error("Invalid action");
				return;
			}

			ctrl.isBusy = true;

			powerbiAPI
				.groupShare(reference.id, groups)
				.callbacks(
					//Success Callback
					function (successData) {
						var powerbiReference = ctrl.powerbiReferences.find(x => x.id == reference.id);
						if (powerbiReference) {
							powerbiReference.sharing = angular.copy(successData.sharing);
						}

						ctrl.shareModelTarget.sharing = angular.copy(successData.sharing);

						$("#group-select").val([]).trigger('change');

						Notification.success("Successfully shared dashboard");
					},
					//Error Callback
					function (errorResult) {
						Notification.error(errorResult);
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.removeGroupShare = function (reference, group) {
			ctrl.isBusy = true;

			var groups = [group];

			powerbiAPI
				.removeGroupShare(reference.id, groups)
				.callbacks(
					//Success Callback
					function (successData) {
						var powerbiReference = ctrl.powerbiReferences.find(x => x.id == successData.id);
						if (powerbiReference) {
							powerbiReference.sharing = angular.copy(successData.sharing);
						}

						ctrl.shareModelTarget.sharing = angular.copy(successData.sharing);

						Notification.success("Successfully removed sharing");
					},
					//Error Callback
					function (errorResult) {
						Notification.error(errorResult);
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.initUserSelect = function () {
			angular.element("#user-select").select2({
				allowClear: true,
				minimumInputLength: 3,
				placeholder: "Search from AD",
				multiple: true,
				ajax: {
					delay: 500,
					url: '../api/dashboard/azureaccess',
					dataType: 'json',
					beforeSend: function (xhr) {
						// Anti Forgery Validation Token
						xhr.setRequestHeader('RequestVerificationToken', requestVerificationToken);
					},
					data: function (params) {
						// Query parameters will be ?query=[term]
						var query = {
							search: params.term
						}
						return query;
					},
					processResults: function (data) {
						var data_ = data.map(function (datum) {
							return {
								"id": datum.id,
								"text": "[" + datum.mail + "] " + datum.displayName,
								"user": datum
							};
						});
						return {
							results: data_
						};
					}
				}
			}).on('change', function (e) {
				var data = angular.element("#user-select").select2("data");

				ctrl.shareToUsers = [];
				data.forEach(function (item) {
					var obj = {
						azureId: item.id,
						displayName: item.user.displayName,
						email: item.user.mail
					}

					ctrl.shareToUsers.push(obj);
				});
			});
		}

		ctrl.initGroupSelect = function () {
			angular.element("#group-select").select2({
				allowClear: true,
				minimumInputLength: 3,
				placeholder: "Search from AD",
				multiple: true,
				ajax: {
					delay: 500,
					url: '../api/dashboard/azureaccess/groups',
					dataType: 'json',
					beforeSend: function (xhr) {
						// Anti Forgery Validation Token
						xhr.setRequestHeader('RequestVerificationToken', requestVerificationToken);
					},
					data: function (params) {
						// Query parameters will be ?query=[term]
						var query = {
							search: params.term
						}
						return query;
					},
					processResults: function (data) {
						var data_ = data.map(function (datum) {
							return {
								"id": datum.id,
								"text": datum.mail
							};
						});
						return {
							results: data_
						};
					}
				}
			}).on('change', function (e) {
				var data = angular.element("#group-select").select2("data");

				ctrl.shareToGroups = [];
				data.forEach(function (item) {
					var obj = {
						azureId: item.id,
						email: item.text
					}

					ctrl.shareToGroups.push(obj);
				});
			});
		}
	}

})();