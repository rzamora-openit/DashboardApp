// app-site.js

(function () {
	"use strict";

	var siteApp = angular.module("app-site", []);

	siteApp.value("siteValues", {
		environmentDebug: "DEBUG",
		environmentStaging: "STAGING",
		environmentRelease: "RELEASE",

		//Change this based on the environment
		environmentCurrent: "none"
	});

	// Anti Forgery Validation Token
	siteApp.value("requestVerificationToken",
		angular.element(document.querySelector('input[name="__RequestVerificationToken"]'))[0]?.value);

	siteApp.factory("debugLog", function (siteValues, requestVerificationToken) {
		var service = {};
		service.write = function (source, message) {
			if (siteValues.environmentCurrent == siteValues.environmentDebug) {
				console.log('source:', source);
				console.log('message:', message);

				if (message?.data?.type == 'https://tools.ietf.org/html/rfc7231#section-6.5.1') {
					console.error('Improper configuration of request.');
					if (!requestVerificationToken) {
						console.error('Request Verification Token might not be properly implemented.');
					}
				}
			}
		};
		return service;
	});

	siteApp.factory("httpControl", function ($http, debugLog, requestVerificationToken) {
		var service = {};

		var successFunction = function (serviceName, successCallback, transformMethod) {
			return function (successResult) {
				debugLog.write(serviceName + ".successFunction", successResult);
				var data = successResult.data;
				if (angular.isFunction(transformMethod)) {
					data = transformMethod(data);
				}
				if (angular.isFunction(successCallback)) {
					successCallback(data);
				}
			}
		};

		var errorFunction = function (serviceName, errorCallback) {
			return function (errorResult) {
				debugLog.write(serviceName + ".errorFunction", errorResult);
				if (angular.isFunction(errorCallback)) {
					errorCallback(errorResult.data);
				}
			}
		};

		var finallyFunction = function (serviceName, finallyCallback) {
			return function () {
				debugLog.write(serviceName + ".finallyFunction", "-");
				if (angular.isFunction(finallyCallback)) {
					finallyCallback();
				}
			}
		};

		var retryOnErrorFunction = function (serviceName, errorCallback, retryCheckFunction, executeHttpCall, retryCount, retryEndCallback) {
			return function (errorResult) {
				debugLog.write(serviceName + ".errorFunction", errorResult);
				if (angular.isFunction(errorCallback)) {
					errorCallback(errorResult.data);
				}

				if (angular.isFunction(retryCheckFunction)) {					
					if (retryCheckFunction(retryCount)) {
						debugLog.write(serviceName + ".retryCallback", "retrying " + retryCount);
						console.log("timeout");
						setTimeout(executeHttpCall, 200);				
					} else {
						debugLog.write(serviceName + ".retryCallback", "retry ended");
						if (angular.isFunction(retryEndCallback)) {							
							retryEndCallback(errorResult, retryCount);
						}
					}
				}
			}
		};

		var httpInstance = function (serviceName, httpContext, transformMethod) {

			this.callbacks = function (successCallback, errorCallback, finallyCallback) {
				httpContext
					.then(
						successFunction(serviceName, successCallback, transformMethod),
						errorFunction(serviceName, errorCallback)
					)
					.finally(
						finallyFunction(serviceName, finallyCallback)
					);
			}

			this.persistentcallbacks = function (successCallback, errorCallback, finallyCallback, retryCheckFunction, retryEndCallback) {
				var retryCount = 0;
				function executeHttpCall() {
					httpContext
						.then(
							successFunction(serviceName, successCallback, transformMethod),
							retryOnErrorFunction(serviceName, errorCallback, retryCheckFunction, executeHttpCall, retryCount, retryEndCallback)
						)
						.finally(
							finallyFunction(serviceName, finallyCallback)
					);
					retryCount++;
				}

				executeHttpCall();
			}

		};

		let config = {
			headers: { RequestVerificationToken: requestVerificationToken }
		};

		service.get = function (serviceName, url, payload, transformMethod) {
			return new httpInstance(serviceName, $http.get(url, config, payload), transformMethod);
		};

		service.post = function (serviceName, url, payload, transformMethod) {
			return new httpInstance(serviceName, $http.post(url, payload, config), transformMethod);
		};

		service.put = function (serviceName, url, payload, transformMethod) {
			return new httpInstance(serviceName, $http.put(url, payload, config), transformMethod);
		};

		service.delete = function (serviceName, url, payload, transformMethod) {
			return new httpInstance(serviceName, $http.delete(url, config, payload), transformMethod);
		};

		return service;
	});

	siteApp.factory("stateUtil", function () {
		var service = {};

		service.state = function (id, value) {
			this.id = id;
			this.value = value;
		};

		service.stateContext = function () {

			var sc = this;
			sc.states = [];
			sc.value = {};

			sc.setState = function (id, value) {
				sc.clearState();
				sc.addState(id, value);
			};

			sc.addState = function (id, value) {
				sc.states.push(new service.state(id, value));
				sc[id] = true;
				sc.value = value;
			};

			sc.appendState = function (id, value) {
				var state = new service.state(id, value)
				sc.states.push(state);
				sc[id] = state;
			};

			sc.deleteState = function (id) {
				delete sc[id];
			};

			sc.removeState = function (id) {
				sc.deleteState(id);
				sc.states = sc.states.filter(function (st) {
					return id != st.id;
				});
			};

			sc.clearState = function () {
				sc.states.forEach(function (state) {
					sc.deleteState(state.id);
				});
				sc.states = [];
			};

			sc.disableStates = function () {
				sc.states.forEach(function (state) {
					sc[state.id] = false;
				});
			}

			sc.pushState = function (id, value) {
				sc.disableStates();
				sc.addState(id, value);
			}

			sc.popState = function () {		

				var popped = sc.states.pop();
				sc.deleteState(popped.id);

				var last = sc.states[sc.states.length - 1];
				
				sc[last.id] = true;
				sc.value = last.value;
			}
		};

		return service;
	});

	siteApp.directive('smartTableCurrentPage', function () {
		return {
			require: '^stTable',
			templateUrl: '/pages/site/views/smartTableCurrentPage.html',
			link: function (scope, element, attr, stTable) {
				scope.$watch(function () {
					return stTable.tableState();
				}, function () {
					var tableState = stTable.tableState();
					if (tableState) {
						scope.pagination = tableState.pagination;
					}
				}, true);
			}
		}
	});

	siteApp.factory('guidService', function () {
		var service = {};

		// Reference : https://gist.github.com/nomadsource/181a54df2e49cd4e238f
		service.generate = function () {
			//console.time('start guid');
			var d = new Date().getTime();
			var uuid = 'xxxxxxxx-xxxx-xxxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
				var r = (d + Math.random() * 16) % 16 | 0;
				d = Math.floor(d / 16);
				return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
			});
			//console.timeEnd('start guid');
			return uuid;
		};

		return service;
	});

})();