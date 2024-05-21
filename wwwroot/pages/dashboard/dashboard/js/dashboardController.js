// dashboardController.js

(function () {
	"use strict";

	angular.module("app-dashboard").controller("dashboardController", dashboardController);

	function dashboardController($scope, $routeParams, $location, stateUtil) {

		var ctrl = this;
		ctrl.isBusy = false;
		ctrl.stateContext = new stateUtil.stateContext();
		ctrl.errorData = [];

		$scope.$on("$viewContentLoaded", function () {
			ctrl.locationHandler();
		});

		$scope.$on('$locationChangeSuccess', function (event) {
			ctrl.locationHandler();
		});

		ctrl.locationHandler = function () {
			ctrl.sectionId = $routeParams.sectionId;

			ctrl.dashboardView = (ctrl.sectionId && $routeParams.section == "view");

			if (ctrl.dashboardView) {
				ctrl.stateContext.setState("view");
			} else {
				ctrl.stateContext.setState("table");
            }
		};

		ctrl.navigate = function (path) {
			$location.path(path);
		};

		
	}
})();