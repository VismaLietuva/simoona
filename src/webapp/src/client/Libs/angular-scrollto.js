angular.module('scrollto', []);

angular.module('scrollto')
  .directive('scrollTo', ['$timeout', function ($timeout) {
    
    function scroll (settings) {
      return function () {
        var scrollPane = angular.element(settings.container);
        var scrollTo = (typeof settings.scrollTo == "number") ? settings.scrollTo : angular.element(settings.scrollTo);
        var scrollY = (typeof scrollTo == "number") ? scrollTo : scrollTo.offset().top - settings.offset;
        scrollPane.animate({scrollTop : scrollY }, settings.duration, settings.easing, function(){
          if (typeof callback == 'function') { callback.call(this); }
        });
      }
    }
    
    return {
      restrict: 'A',
      link: function (scope, element, attrs) {
        var settings = angular.extend({
          container: 'html, body',
          scrollTo: angular.element(),
          offset: 0,
          duration: 150,
          easing: 'swing'
        }, attrs);
        
        element.on('click', function () {
          $timeout(scroll(settings));
        });
      }
    };
  }]);