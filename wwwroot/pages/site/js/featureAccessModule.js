// featureAccessModule.js

(function () {
    "use strict";

	var app = angular.module("featureAccessModule", ["app-site", "angular.filter", "ui-notification"]);
	app.directive("featureAccessContainer", function () {
        return {
            restrict: "A",
            scope: {
                featureAccess: '='
            },
            link: function (scope, element, attr) {                
				scope.accessLevelFlags =	[{ flag: 0, value: "None" },
					{ flag: 1, value: "Read" },
					{ flag: 2, value: "Write" },
					{ flag: 4, value: "Admin" }];

				scope.getAccessLevelByFlag = function (flag) {
					var accessLevel = "Undefined";
					scope.accessLevelFlags.forEach(function (accessLevelFlag) {
						if (accessLevelFlag.flag == flag) {
							accessLevel = accessLevelFlag.value;
						}
					});
					return accessLevel;
				}
			},
			controller: featureAccessController,
            //replace : true,
			templateUrl: '/pages/site/views/featureAccessTemplate.html'
        };
	});

	function featureAccessController($scope, stateUtil, rolesAPI, featureAccessAPI, Notification, requestVerificationToken) {

		$scope.ctrl = this;
		var ctrl = this;
		ctrl.isBusy = false;
		ctrl.stateContext = new stateUtil.stateContext();
		ctrl.errorData = [];

		ctrl.roles = [];

		ctrl.addShareRoleModel = {};

		ctrl.$onInit = function () {
			ctrl.initUserSelect();
			ctrl.initGroupSelect();
			ctrl.initRoleSelect();
		};

		ctrl.initUserSelect = function () {
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
				if (ctrl.addShareUserModel) {
					var data = angular.element("#user-select").select2("data");					
					if (data && data.length) {
						ctrl.addShareUserModel.reference = data[0].user.displayName;
						ctrl.addShareUserModel.azureId = this.value;
						$scope.$apply();
					}
				}
			});
		}

		ctrl.initGroupSelect = function () {
			angular.element("#group-select").select2({
				allowClear: true,
				minimumInputLength: 3,
				placeholder: "Search from AD",
				ajax: {
					delay: 500,
					url: '../api/admin/azureaccess/groups',
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
				if (ctrl.addShareGroupModel) {
					var data = angular.element("#group-select").select2("data");
					if (data && data.length) {
						ctrl.addShareGroupModel.reference = data[0].text;
						ctrl.addShareGroupModel.azureId = this.value;
						$scope.$apply();
					}
				}
			});
		}

		ctrl.initRoleSelect = function () {
			rolesAPI
				.get()
				.callbacks(
					//Success Callback
					function (successData) {
						ctrl.roles = successData;						
					},
					//Error Callback
					function (errorResult) {						
					},
					//Finally Callback
					function () {
						ctrl.isBusy = false;
					}
				);
		}

		ctrl.addAccess = function (accessModel) {
			ctrl.isBusy = true;
			featureAccessAPI
				.addAccess($scope.featureAccess.id, accessModel)
				.callbacks(
					//Success Callback
					function (successData) {						
						$scope.featureAccess.accesses = successData.accesses;
						angular.element('#shareRoleModal').modal('hide');
						angular.element('#shareUserModal').modal('hide');
						angular.element('#shareGroupModal').modal('hide');

						Notification.success("Access Priviledge Added");
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

		ctrl.initDeleteAccess = function (access) {
			ctrl.deleteAccessTarget = access;
		}

		ctrl.deleteAccess = function (access) {
			ctrl.isBusy = true;
			featureAccessAPI
				.deleteAccess($scope.featureAccess.id, access.id)
				.callbacks(
					//Success Callback
					function (successData) {
						var index = $scope.featureAccess.accesses.indexOf(access);
						$scope.featureAccess.accesses.splice(index, 1);
						ctrl.deleteAccessTarget = {};
						angular.element('#deleteAccessModal').modal('hide');

						Notification.success("Access Removed");
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

		ctrl.initShareRole = function () {
			
			ctrl.addShareRoleModel = {
				type: "Role"
			};
		}

		ctrl.addRoleAccess = function (shareRoleModel) {
			var accessModel = {
				type: shareRoleModel.type,
				reference: shareRoleModel.role.value,
				azureId: shareRoleModel.role.id,
				level: shareRoleModel.level
			};
			ctrl.addAccess(accessModel);
		}

		ctrl.initShareGroup = function () {
			angular.element("#group-select").val(null).trigger('change');
			ctrl.addShareGroupModel = {
				type: "Group"
			};
		}

		ctrl.initShareUser = function () {
			angular.element("#user-select").val(null).trigger('change');
			ctrl.addShareUserModel = {
				type: "User"
			};
		}
	}

})();