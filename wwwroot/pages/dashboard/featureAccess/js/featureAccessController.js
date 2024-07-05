// featureAccessController.js

(function () {
	"use strict";

	angular.module("app-featureAccess").controller("featureAccessController", featureAccessController);

	function featureAccessController($scope, $routeParams, $window, stateUtil, featureAccessAPI, Notification) {

		var ctrl = this;
		ctrl.isBusy = false;
		ctrl.stateContext = new stateUtil.stateContext();
		ctrl.errorData = [];

		ctrl.overlayShareComponentShow = false;

		ctrl.featureAccess = [];		
		ctrl.targetFeature = {};

		$scope.$on("$viewContentLoaded", function () {
			ctrl.stateContext.setState("loading");
			ctrl.getFeatureAccess();			
		});

		ctrl.getFeatureAccess = function () {
			ctrl.isBusy = true;
						
			featureAccessAPI
				.get()
				.callbacks(
					//Success Callback
					function (successData) {
						ctrl.featureAccess = successData;
						ctrl.stateContext.pushState("table");
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

		ctrl.initShare = function (feature) {
			ctrl.overlayShareComponentShow = false;

			setTimeout(function () {
				ctrl.targetFeature = feature;
				ctrl.overlayShareComponentShow = true;
				$scope.$apply();
			}, 20);
		}

		ctrl.hideShareComponent = function () {			
			ctrl.overlayShareComponentShow = false;
			ctrl.targetfetargetFeatureatureAccess = null;
		}
	}

})();