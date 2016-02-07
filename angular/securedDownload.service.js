(function (){
    'use strict';

    angular.module('tokendownload-angular').service('securedDownloadService', ['$q', '$http', function($q, $http) {
     	
		var getTokenFromApi = function(url) {
			
			var securedUrl = url;
			
			var deferred = $q.defer();
			var args = { headers: { 'Accept': 'application/vnd.secureaccesstoken+json' } };
			
			$http.get(url, args).success(function (data) {
				if (data && data.token) {
				
					if (url.indexOf('?') !== -1) {
						securedUrl += '&sat='+ data.token;
					}
					else {
						securedUrl += '?sat='+ data.token;
					}
				
					deferred.resolve(securedUrl, data.token);
				}
				else {
					deferred.reject();
				}
			});
		
			return deferred.promise;
		};

		return {
				getDownloadUrl : getTokenFromApi
			};
		}]);
		
})();