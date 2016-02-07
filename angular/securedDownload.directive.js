(function (){
    'use strict';

    angular.module('tokendownload-angular').directive('securedDownload', function(){
       return {
           replace: true,
		   link: function(scope, elem) { 
               elem.on('click', function(e){
                    
                    if (!scope.hasToken) {
                        e.preventDefault();
                    }
                });
               elem.on('contextmenu', function(e){
                    if (!scope.hasToken) {
                        e.preventDefault();
                    }
                });
           },
           transclude: true,
           template: '<a ng-click="downloadWithToken()" ng-style="{ \'cursor\' : isLoading ?  \'progress\' : \'pointer\' }"><ng-transclude></a>',
           controller: ['$scope', '$q', '$attrs', '$window', 'securedDownloadService', function(scope, $q, attrs, $window, securedDownloadService) {
              
              scope.downloadWithToken = function() {
                  
                  if (scope.hasToken || !scope.href) {
                      return;
                  }
                  
                  scope.isLoading = true;
                  
                  var href = scope.href;

                  securedDownloadService.getDownloadUrl(href).then(function (securedUrl) {

                      attrs.$set('href', securedUrl);
                      
                      scope.isLoading = false;
                      scope.hasToken = true;
                      
                      $window.location = securedUrl;
                  }, function() {
                      scope.isLoading = false;
                      scope.hasToken = false;
                  });
              };
            }],
           scope: {
               href: '@'
           }
       };
    });
    
    if (!String.prototype.endsWith) {
      String.prototype.endsWith = function(searchString, position) {
          var subjectString = this.toString();
          if (position === undefined || position > subjectString.length) {
            position = subjectString.length;
          }
          position -= searchString.length;
          var lastIndex = subjectString.indexOf(searchString, position);
          return lastIndex !== -1 && lastIndex === position;
      };
    }    
})();
