// app-roles.js

(function () {
	"use strict";

	angular.module("app-roles", ["ngRoute", "app-site", "smart-table", "ui.filters", "ui-notification"])
		.config(function ($routeProvider) {
			$routeProvider.when("/", {
				controller: "rolesController",
				controllerAs: "ctrl",
				templateUrl: "/pages/admin/roles/views/rolesView.html",
				reloadOnUrl: false,
				reloadOnSearch: false
			});

			$routeProvider.otherwise({ redirectTo: "/" });
		})
		.config(["$httpProvider", function ($httpProvider) {
			// http://stackoverflow.com/questions/16098430/angular-ie-caching-issue-for-http
			//initialize get if not there
			if (!$httpProvider.defaults.headers.get) {
				$httpProvider.defaults.headers.get = {};
			}

			//disable IE ajax request caching
			$httpProvider.defaults.headers.get["If-Modified-Since"] = "Fri, 28 Jun 1985 05:00:00 GMT";
			// extra
			$httpProvider.defaults.headers.get["Cache-Control"] = "no-cache";
			$httpProvider.defaults.headers.get["Pragma"] = "no-cache";
		}]);


})();