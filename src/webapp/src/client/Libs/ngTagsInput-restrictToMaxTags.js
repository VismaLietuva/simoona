/**
 * A directive to hide the tags input when the max tags limit is reached.
 * Works with github.com/mbenford/ngTagsInput
 */
angular.module('dz.restrictToMaxTags', ['ngTagsInput']).directive( 'restrictToMaxTags', function () {
    var KEY_BACKSPACE =  8
        KEY_TAB = 9;
    return {
        require: 'ngModel',
        priority: -10,
        link: function ( $scope, $element, $attrs, ngModelController ) {
            var tagsInputScope = $element.isolateScope(),
                maxTags,
                getTags,
                checkTags,
                maxTagsReached,
                input = $element.find( 'input' ),
                placeholder;
            $attrs.$observe( 'maxTags', function ( _maxTags ) {
                maxTags = _maxTags;
            });

            getTags = function () {
                return ngModelController.$modelValue;
            };

            checkTags = function () {
                var tags = getTags();
                if ( tags && tags.length && tags.length >= maxTags ) {
                    if ( !maxTagsReached ) {
                        // trigger the autocomplete to hide
                        tagsInputScope.events.trigger( 'input-blur' );
                        placeholder = input.attr( 'placeholder' );
                        input.attr( 'placeholder', '' );
                        // use max-width to avoid conflicts with the tiAutosize
                        // directive that ngTagsInput uses
                        input.css( 'max-width', '0' );
                        maxTagsReached = true;
                    }
                } else if ( maxTagsReached ) {
                    input.attr( 'placeholder', placeholder );
                    input.css( 'max-width', '' );
                    maxTagsReached = false;
                }
            };

            $scope.$watchCollection( getTags, checkTags );

            // prevent any keys from being entered into
            // the input when max tags is reached
            input.on( 'keydown', function ( event ) {
                if ( maxTagsReached && event.keyCode !== KEY_BACKSPACE && event.keyCode !== KEY_TAB ) {
                    event.stopImmediatePropagation();
                    event.preventDefault();
                }
            });

            // prevent the autocomplete from being triggered
            input.on( 'focus', function ( event ) {
                checkTags();
                if ( maxTagsReached ) {
                    tagsInputScope.hasFocus = true;
                    event.stopImmediatePropagation();
                }
            });
        }
    };
});