(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .constant('errorCodeMessages', {
            200: 'invalidStartDate',
            201: 'invalidEndDate',
            202: ['requiredError', 'events.eventHost'],
            203: 'invalidOptions',
            204: 'invalidType',
            205: ['requiredError', 'events.eventMaxAmountOfOptions'],
            206: 'eventDoesNotExist',
            207: 'eventPermissions',
            208: 'messageNotEnoughOptions',
            209: 'messageEventHasExpired',
            210: 'messageTooManyOptionsProvided',
            211: 'messageEventIsFull',
            212: 'messageUserAlreadyParticipates',
            213: 'messageEventJoinStartedOrFinished',
            214: 'messageEventCannotJoinMultipleSingleJoinEventsCode',
            215: 'messageEventNoSuchOptionsCode',
            216: 'messageEventDeadlineAfterStartDate',
            217: 'messageEventInvalidRegistrationDeadlineDate',
            219: 'messageEventOptionsUnique',
            220: { default: 'messageEventCannotLeaveAlreadyLeft', expelParticipant: 'messageExpelParticipantNotFound' },
            221: 'messageEventParticipantsNotFound',
            222: 'messageEventTypeNameExists',
            227: 'messageEventAttendTypeIsNotAllowed',
            228: 'messageEventAttendTypeCannotBeChangedIfParticipantsJoined',
            302: 'messageCannotSendKudosPointsToThisUser',
            303: 'messageKudosTypeNotFound',
            304: 'messageInsufficientKudos',
            305: 'messageSenderReceiverCannotAcceptRejectKudos',
            306: 'messageKudosAlreadyApproved',
            350: 'messageShopItemAlreadyExists',
            405: 'messageWallNameExists',
            501: 'messageSomethingWentWrong',
            402: 'messageSomethingWentWrong',
            403: 'messageUpvotingAlreadyUpvoted',
            444: 'messageNameAlreadyExists',
            600: 'messageNameAlreadyExists',
            602: 'messageSomethingWentWrong',
            604: 'messageOrganizationNotFound',
            1000: 'messageError'
        })
        .constant('repeatableActionErrorCodeList', [
            '220'
        ])
        .factory('errorHandler', errorHandler);

    errorHandler.$inject = [
        'localeSrv',
        'notifySrv',
        'errorCodeMessages',
        'repeatableActionErrorCodeList',
        'lodash'
    ];

    function errorHandler(localeSrv, notifySrv, errorCodeMessages,
     repeatableActionErrorCodeList, lodash) {
        var service = {
            handleErrorMessage: handleErrorMessage,
            handleError: handleError
        };
        return service;

        /////////

        function handleErrorMessage(errorObj, errorProperty) {
            var error = errorObj.data;

            if (error) {
                var errorCode = !!error.message ? errorCodeMessages[error.message] : errorCodeMessages[error];

                if (!!error.errorCode && errorCodeMessages[error.errorCode]) {
                    notifySrv.error('errorCodeMessages.' + errorCodeMessages[error.errorCode]);
                } else {
                    if (angular.isArray(errorCode)) {
                        notifySrv.error(localeSrv.formatTranslation('errorCodeMessages.' + errorCode[0], { one: errorCode[1] }));
                    } else if (angular.isObject(errorCode)) {
                        if (errorProperty) {
                            notifySrv.error('errorCodeMessages.' + errorCode[errorProperty]);
                        } else {
                            notifySrv.error('errorCodeMessages.' + errorCode['default']);
                        }
                    } else if (angular.isString(errorCode)) {
                        notifySrv.error('errorCodeMessages.' + errorCode);
                    } else {
                        if (angular.isString(errorObj)) {
                            notifySrv.error(errorObj);
                        } else if (!!error && angular.isString(error)) {
                            notifySrv.error(error);
                        } else if (!!error && !!error.message) {
                            notifySrv.error(error.message);
                        } else if (!!error.crudViewModel) {
                            //temporary solution for unique response
                            notifySrv.error(error.crudViewModel.errors[0].errorMessage);
                        } else {
                            notifySrv.error(errorObj.message);
                        }
                    }
                }
            } else if (angular.isString(errorObj)) {
                notifySrv.error(errorObj);
            } else if (!!errorObj.message && angular.isString(errorObj.message)) {
                notifySrv.error(errorObj.message);
            }
        }

        function handleError(errorObj, errorActions) {
            if (!!errorObj.data && !!errorObj.data.message) {
                if (isRepeatableAction(errorObj.data.message)) {
                    errorActions.repeat();
                } else {
                    handleErrorMessage(errorObj);
                }
            } else {
                handleErrorMessage(errorObj);
            }
        }

        function isRepeatableAction(errorMessage) {
            return lodash.indexOf(repeatableActionErrorCodeList, errorMessage) !== -1;
        }
    }

})();
