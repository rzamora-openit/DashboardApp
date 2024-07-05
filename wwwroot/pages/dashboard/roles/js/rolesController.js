// rolesController.js

(function () {
	"use strict";

	angular.module("app-roles").controller("rolesController", rolesController);

	function rolesController($scope, $routeParams, $window, stateUtil, rolesAPI, Notification, requestVerificationToken) {

		var ctrl = this;
		ctrl.isBusy = false;
		ctrl.stateContext = new stateUtil.stateContext();
		ctrl.errorData = [];

		ctrl.roles = [];
		ctrl.roleAssignments = [];	

		$scope.$on("$viewContentLoaded", function () {
			ctrl.stateContext.setState("loading");
			ctrl.getRoles();
			ctrl.initSelect();
		});

		ctrl.initSelect = function () {
			angular.element("#user-select").select2({
				allowClear: true,				
				minimumInputLength: 3,
				placeholder: "Search from AD",
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
						var data_=  data.map(function (datum) {
							return {
								"id": datum.id,
								"text": "[" + datum.mail + "] " + datum.displayName
							};
						});						
						return {
							results: data_
						};
					}							
				}
			}).on('change', function (e) {			
				//var data = angular.element("#user-select").select2("data");
				if (ctrl.assignRoleModel) {
					ctrl.assignRoleModel.userId = this.value;
					$scope.$apply();
				}				
			});			
		}

		ctrl.getRoles = function () {
			ctrl.isBusy = true;
						
			rolesAPI
				.get()
				.callbacks(
					//Success Callback
					function (successData) {
						ctrl.roles = successData;						
						ctrl.getRolesAssignments();
					},
					//Error Callback
					function (errorResult) {
						ctrl.stateContext.pushState("invalidActivity");
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		};

		ctrl.getRolesAssignments = function () {
			ctrl.isBusy = true;
			
			rolesAPI
				.getRoleAssignments()
				.callbacks(
					//Success Callback
					function (successData) {
						ctrl.roleAssignments = successData;

						ctrl.roleAssignments.forEach(function (roleAssignment) {
							ctrl.roles.forEach(function (role) {
								if (roleAssignment.appRoleId == role.id)
								{
									roleAssignment.role = role;
								}
							});
						});

						ctrl.stateContext.setState("table");
					},
					//Error Callback
					function (errorResult) {
						ctrl.stateContext.pushState("invalidActivity");
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;						
					}
				);
		};

		ctrl.addRoleInit = function ()
		{
			ctrl.addRoleModel = {};
		}

		ctrl.addRole = function (roleModel) {
			ctrl.isBusy = true;

			rolesAPI
				.save(roleModel.name, roleModel.description)
				.callbacks(
					//Success Callback
					function (successData) {						
						angular.element('#addRoleModal').modal('hide');
						Notification.success("Role [" + roleModel.name + "] Added");
						setTimeout(ctrl.getRoles, 3000);
					},
					//Error Callback
					function (errorResult) {						
						Notification.error("Adding Role [" + roleModel.name + "] Failed");
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.disableRoleInit = function (role) {
			ctrl.disableRoleTarget = role;
		}

		ctrl.disableRole = function (roleModel) {
			ctrl.isBusy = true;

			rolesAPI
				.disable(roleModel.value)
				.callbacks(
					//Success Callback
					function (successData) {						
						angular.element('#disableRoleModal').modal('hide');
						Notification.success("Role [" + roleModel.value + "] Disabled");
						setTimeout(ctrl.getRoles, 3000);
					},
					//Error Callback
					function (errorResult) {						
						Notification.error("Disabling Role [" + roleModel.value + "] Failed");
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.enableRoleInit = function (role) {
			ctrl.enableRoleTarget = role;
		}

		ctrl.enableRole = function (roleModel) {
			ctrl.isBusy = true;

			rolesAPI
				.enable(roleModel.value)
				.callbacks(
					//Success Callback
					function (successData) {						
						angular.element('#enableRoleModal').modal('hide');
						Notification.success("Role [" + roleModel.value + "] Enalbed");
						setTimeout(ctrl.getRoles, 3000);
					},
					//Error Callback
					function (errorResult) {						
						Notification.error("Enabling Role [" + roleModel.value + "] Failed");
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.deleteRoleInit = function (role) {
			ctrl.deleteRoleTarget = role;
		}

		ctrl.deleteRole = function (roleModel) {
			ctrl.isBusy = true;

			rolesAPI
				.delete(roleModel.value)
				.callbacks(
					//Success Callback
					function (successData) {						
						angular.element('#deleteRoleModal').modal('hide');
						Notification.success("Role [" + roleModel.value + "] Deleted");
						setTimeout(ctrl.getRoles, 3000);
					},
					//Error Callback
					function (errorResult) {
						Notification.error("Deleting Role [" + roleModel.value + "] Failed");
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.setAssignMode = function () {
			ctrl.stateContext.pushState("assign");
		}

		ctrl.assignRoleInit = function () {
			ctrl.assignRoleModel = {};
		}

		ctrl.addAssignment = function (assignRoleModel)
		{
			ctrl.isBusy = true;

			rolesAPI
				.assignRole(assignRoleModel.userId, assignRoleModel.roleId)
				.callbacks(
					//Success Callback
					function (successData) {
						angular.element('#assignRoleModal').modal('hide');						
						Notification.success("Assignment Added");						
						setTimeout(ctrl.getRolesAssignments, 3000);
					},
					//Error Callback
					function (errorResult) {
						Notification.error("Assigning Role [" + assignRoleModel.value + "] Failed");
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;						
					}
				);
		}

		ctrl.removeAssignmentInit = function (roleAssignment) {
			ctrl.unassignTarget = roleAssignment;
		}

		ctrl.removeAssignment = function (roleAssignment)
		{
			ctrl.isBusy = true;

			rolesAPI
				.removeAssignment(roleAssignment.principalId, roleAssignment.id)
				.callbacks(
					//Success Callback
					function (successData) {
						angular.element('#unassignRoleModal').modal('hide');
						Notification.success("Assignment Removed");
						setTimeout(ctrl.getRolesAssignments, 3000);
					},
					//Error Callback
					function (errorResult) {
						Notification.error("Unassigning Role [" + assignRoleModel.value + "] Failed");
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.back = function () {
			ctrl.stateContext.popState();
		};
	}

})();