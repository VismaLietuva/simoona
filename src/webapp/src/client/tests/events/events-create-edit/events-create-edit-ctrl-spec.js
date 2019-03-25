describe('addNewEventController', function () {
    var $rootScope, $q, addScope, editScope, addCtrl, editCtrl;

    beforeEach(module('simoonaApp.Events'));
    beforeEach(function () {
        module(function ($provide) {
            $provide.value('errorHandler', eventsMocks.errorHandler);
            $provide.value('authService', eventsMocks.authService);
        });
    });

    beforeEach(inject(function (_$q_) {
        $q = _$q_;

        spyOn(eventsMocks.eventRepository, 'getEventTypes').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        spyOn(eventsMocks.eventRepository, 'getEventRecurringTypes').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        spyOn(eventsMocks.eventRepository, 'getUserForAutoCompleteResponsiblePerson').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        spyOn(eventsMocks.eventRepository, 'getUserResponsiblePersonById').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(eventsMocks.responsibleUserResponse);
            return deferred.promise;
        });
        spyOn(eventsMocks.eventRepository, 'searchUsers').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        spyOn(eventsMocks.eventRepository, 'getEventUpdate').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(eventsMocks.sampleEvent);
            return deferred.promise;
        });
        spyOn(eventsMocks.eventRepository, 'getMaxEventParticipants').and.callFake(function () {
            return {
                query: function() {}
            };
        });
        spyOn(eventsMocks.notifySrv, 'success').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        spyOn(eventsMocks.notifySrv, 'error').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        spyOn(eventsMocks.errorHandler, 'handleErrorMessage').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        spyOn(eventsMocks.$state, 'go').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        spyOn(eventsMocks.$translate, 'instant').and.callFake(function (key) {
            return key;
        });
        
    }));

    beforeEach(inject(function (_$controller_,  _$rootScope_) {
        $rootScope = _$rootScope_;
        addScope = $rootScope.$new();

        addCtrl = _$controller_('addNewEventController', {
            $rootScope: $rootScope,
            $scope: addScope,
            $stateParams: {id: null},
            $state: eventsMocks.$state,
            $timeout: eventsMocks.$timeout,
            $translate: eventsMocks.$translate,
            dataHandler: eventsMocks.dataHandler,
            eventImageSettings: eventsMocks.eventImageSettings,
            authService: eventsMocks.authService,
            eventRepository: eventsMocks.eventRepository,
            pictureRepository: eventsMocks.pictureRepository,
            eventsMessages: eventsMocks.eventsMessages,
            eventSettings: eventsMocks.eventSettings,
            recurringTypesResources: eventsMocks.recurringTypesResources,
            notifySrv: eventsMocks.notifySrv,
            shroomsFileUploader: eventsMocks.shroomsFileUploader,
            localeSrv: eventsMocks.localeSrv
        });

        $rootScope.$digest();
    }));

    beforeEach(inject(function (_$controller_,  _$rootScope_) {
        $rootScope = _$rootScope_;
        editScope = $rootScope.$new();

        editCtrl = _$controller_('addNewEventController', {
            $rootScope: $rootScope,
            $scope: editScope,
            $stateParams: eventsMocks.$stateParams,
            $state: eventsMocks.$state,
            $timeout: eventsMocks.$timeout,
            $translate: eventsMocks.$translate,
            dataHandler: eventsMocks.dataHandler,
            eventImageSettings: eventsMocks.eventImageSettings,
            authService: eventsMocks.authService,
            eventRepository: eventsMocks.eventRepository,
            pictureRepository: eventsMocks.pictureRepository,
            eventsMessages: eventsMocks.eventsMessages,
            eventSettings: eventsMocks.eventSettings,
            recurringTypesResources: eventsMocks.recurringTypesResources,
            notifySrv: eventsMocks.notifySrv,
            shroomsFileUploader: eventsMocks.shroomsFileUploader,
            localeSrv: eventsMocks.localeSrv
        });

        $rootScope.$digest();
    }));

   describe('addEvent', function () {
       it('should be initialized', function () {
           expect(addCtrl).toBeDefined();
           expect(eventsMocks.eventRepository.getEventTypes).toHaveBeenCalled();
           expect(eventsMocks.eventRepository.getEventRecurringTypes).toHaveBeenCalled();
       });

       it('should set options and startDate', function () {
           expect(addCtrl.isOptions).toBeFalsy();
           expect(addCtrl.event.options.length).toEqual(2);

           expect(addCtrl.event.startDate).toBeDefined();
       });

       it('should set current user as default master', function () {
           expect(eventsMocks.eventRepository.getUserResponsiblePersonById).toHaveBeenCalled();
           expect(eventsMocks.eventRepository.getUserResponsiblePersonById).toHaveBeenCalledWith(eventsMocks.authService.identity.userId);

           expect(addCtrl.responsibleUser).toEqual(eventsMocks.responsibleUserResponse);
       });
   });

   describe('editEvent', function () {
       it('should be initialized', function () {
           expect(editCtrl).toBeDefined();

           expect(eventsMocks.eventRepository.getEventTypes).toHaveBeenCalled();
           expect(eventsMocks.eventRepository.getEventRecurringTypes).toHaveBeenCalled();
       });

       it('should set event fields and options', function () {
           expect(eventsMocks.eventRepository.getEventUpdate).toHaveBeenCalled();
           expect(eventsMocks.eventRepository.getEventUpdate).toHaveBeenCalledWith(eventsMocks.$stateParams.id);

           expect(editCtrl.event).toBeDefined();
           expect(editCtrl.isOptions).toBeTruthy();
           expect(editCtrl.event.options.length).toEqual(3);
       });

       it('should set event master', function () {
           expect(eventsMocks.eventRepository.getUserResponsiblePersonById).toHaveBeenCalled();
           expect(eventsMocks.eventRepository.getUserResponsiblePersonById).toHaveBeenCalledWith(eventsMocks.sampleEvent.responsibleUser.id);

           expect(addCtrl.responsibleUser).toEqual(eventsMocks.responsibleUserResponse);
       });
   });

   describe('searchUsers', function () {
       it('should filter user list', function() {
           addCtrl.searchUsers('John');

           expect(eventsMocks.eventRepository.getUserForAutoCompleteResponsiblePerson).toHaveBeenCalled();
           expect(eventsMocks.eventRepository.getUserForAutoCompleteResponsiblePerson).toHaveBeenCalledWith('John');
       });
   });

   describe('getResponsiblePerson', function () {
       it('should get and set responsible person information', function() {
           addCtrl.getResponsiblePerson(1);

           expect(eventsMocks.eventRepository.getUserResponsiblePersonById).toHaveBeenCalled();
           expect(eventsMocks.eventRepository.getUserResponsiblePersonById).toHaveBeenCalledWith(1);

           expect(addCtrl.responsibleUser).toBeDefined();
       });
   });

   describe('isValidOption', function () {
       it('should return false if selected option is not unique', function() {
           var result = editCtrl.isValidOption(eventsMocks.notUniqueOptions, eventsMocks.notUniqueOptions[0]);

           expect(result).toBeFalsy();
       });

       it('should return true if selected option is unique', function() {
           var result = editCtrl.isValidOption(eventsMocks.uniqueOptions, eventsMocks.uniqueOptions[0]);
           expect(result).toBeTruthy();

           result = editCtrl.isValidOption(eventsMocks.uniqueOptions, eventsMocks.uniqueOptions[1]);
           expect(result).toBeTruthy();

           result = editCtrl.isValidOption(eventsMocks.uniqueOptions, eventsMocks.uniqueOptions[2]);
           expect(result).toBeTruthy();
       });
   });

   describe('addOption', function () {
       it('should add empty option', function() {
           var length = addCtrl.event.options.length;
           addCtrl.addOption();

           expect(addCtrl.event.options.length).toEqual(length + 1);
       });
   });

   describe('deleteOption', function () {
       it('should delete option', function () {
           var length = editCtrl.event.options.length;
           editCtrl.deleteOption(1);

           $rootScope.$digest();

           expect(editCtrl.event.options.length).toEqual(length - 1);
       });

       it('should not hide options if remaining options >= 2', function () {
           editCtrl.deleteOption(1);

           expect(editCtrl.isOptions).toBeTruthy();
       });

       it('should hide options if remaining options < 2', function () {
           editCtrl.deleteOption(0);
           editCtrl.deleteOption(1);

           expect(editCtrl.isOptions).toBeFalsy();
       });
   });

   describe('createEvent', function () {
       it('should show error', function () {
           spyOn(eventsMocks.eventRepository, 'createEvent').and.callFake(function () {
               var deferred = $q.defer();
               deferred.reject({data: {message: '200'}});
               return deferred.promise;
           });
           editCtrl.createEvent('image');
           $rootScope.$digest();

           expect(eventsMocks.errorHandler.handleErrorMessage).toHaveBeenCalled();
       });

       it('should set image', function () {
           spyOn(eventsMocks.eventRepository, 'createEvent').and.callFake(function () {
               var deferred = $q.defer();
               deferred.resolve();
               return deferred.promise;
           });
           editCtrl.createEvent('image');
           $rootScope.$digest();

           expect(editCtrl.event.imageName).toEqual('image');
       });

       it('should create event, show success message and redirect to list', function() {
           spyOn(eventsMocks.eventRepository, 'createEvent').and.callFake(function () {
               var deferred = $q.defer();
               deferred.resolve();
               return deferred.promise;
           });
           editCtrl.createEvent('image');
           $rootScope.$digest();

           expect(eventsMocks.eventRepository.createEvent).toHaveBeenCalled();
           expect(eventsMocks.eventRepository.createEvent).toHaveBeenCalledWith(editCtrl.event);

           expect(eventsMocks.notifySrv.success).toHaveBeenCalled();
           expect(eventsMocks.notifySrv.success).toHaveBeenCalledWith('common.successfullySaved');

           expect(eventsMocks.$state.go).toHaveBeenCalled();
           expect(eventsMocks.$state.go).toHaveBeenCalledWith('Root.WithOrg.Client.Events.List.Type', {type: 'all'});
       });
   });

   describe('updateEvent', function () {
       it('should show error', function () {
           spyOn(eventsMocks.eventRepository, 'updateEvent').and.callFake(function () {
               var deferred = $q.defer();
               deferred.reject({data: {message: '200'}});
               return deferred.promise;
           });
           editCtrl.updateEvent('image');
           $rootScope.$digest();

           expect(eventsMocks.errorHandler.handleErrorMessage).toHaveBeenCalled();
       });

       it('should set image', function () {
           spyOn(eventsMocks.eventRepository, 'updateEvent').and.callFake(function () {
               var deferred = $q.defer();
               deferred.resolve();
               return deferred.promise;
           });
           editCtrl.updateEvent('image');
           $rootScope.$digest();

           expect(editCtrl.event.imageName).toEqual('image');
       });

       it('should update event, show success message and redirect to list', function() {
           spyOn(eventsMocks.eventRepository, 'updateEvent').and.callFake(function () {
               var deferred = $q.defer();
               deferred.resolve();
               return deferred.promise;
           });

           editCtrl.updateEvent('image');
           $rootScope.$digest();

           expect(eventsMocks.eventRepository.updateEvent).toHaveBeenCalled();
           expect(eventsMocks.eventRepository.updateEvent).toHaveBeenCalledWith(editCtrl.event);

           expect(eventsMocks.notifySrv.success).toHaveBeenCalled();
           expect(eventsMocks.notifySrv.success).toHaveBeenCalledWith('common.successfullySaved');

           expect(eventsMocks.$state.go).toHaveBeenCalled();
           expect(eventsMocks.$state.go).toHaveBeenCalledWith('Root.WithOrg.Client.Events.List.Type', {type: 'all'});
       });
   });

   describe('setEvent', function () {
       it('should set event options if options are selected', function () {
           editCtrl.setEvent();

           expect(editCtrl.event.options.length).toEqual(3);
       });

       it('should not set event options if options are not selected', function () {
           editCtrl.isOptions = false;
           editCtrl.setEvent();

           expect(editCtrl.event.options).toEqual([]);
           expect(editCtrl.event.maxOptions).toEqual(0);
       });

       it('should set endDate', function() {
           editCtrl.setEvent();

           expect(editCtrl.event.endDate).toBeDefined();
       });
   });

   describe('showRegistrationDeadline', function () {
       it('should set registrationDeadline date if user selected registrationDeadline checkbox', function () {
           addCtrl.isRegistrationDeadlineEnabled = true;
           addCtrl.showRegistrationDeadline();

           expect(addCtrl.event.registrationDeadlineDate).toEqual(addCtrl.event.startDate);
       });
   });

   describe('openDatePicker', function () {
       it('should call closeAllDatePickers method', function () {
           spyOn(addCtrl, 'closeAllDatePickers');

           addCtrl.openDatePicker(eventsMocks.$event, 'isOpenEventStartDatePicker');
           $rootScope.$digest();

           setTimeout(function() {
               expect(addCtrl.closeAllDatePickers).toHaveBeenCalled();
               expect(addCtrl.closeAllDatePickers).toHaveBeenCalledWith('isOpenEventStartDatePicker');
           }, 1000);
       });
   });

   describe('closeAllDatePickers', function () {
       it('should open current selected datePicker and close others', function () {
           addCtrl.closeAllDatePickers('isOpenEventStartDatePicker');

           expect(addCtrl.datePickers.isOpenEventStartDatePicker).toBeTruthy();
           expect(addCtrl.datePickers.isOpenEventDeadlineDatePicker).toBeFalsy();
           expect(addCtrl.datePickers.isOpenEventFinishDatePicker).toBeFalsy();
       });
   });
   
   
       describe('delete event', function () {
       it('should throw error message', function () {
           spyOn(eventsMocks.eventRepository, 'deleteEvent').and.callFake(function () {
               var deferred = $q.defer();
               deferred.reject({
                   data:  {
                       message: 'Error happened'
                   }
               });
               return deferred.promise;
           });

           editCtrl.deleteEvent(eventsMocks.events[0].id);
           $rootScope.$digest();

           expect(eventsMocks.eventRepository.deleteEvent).toHaveBeenCalled();
           expect(eventsMocks.eventRepository.deleteEvent).toHaveBeenCalledWith(eventsMocks.events[0].id);
           expect(eventsMocks.errorHandler.handleErrorMessage).toHaveBeenCalled();
       });

       it('should delete event and display successful message', function () {
           spyOn(eventsMocks.eventRepository, 'deleteEvent').and.callFake(function () {
               var deferred = $q.defer();
               deferred.resolve();
               return deferred.promise;
           });

           editCtrl.deleteEvent(eventsMocks.events[0].id);
           $rootScope.$digest();

           expect(eventsMocks.eventRepository.deleteEvent).toHaveBeenCalled();
           expect(eventsMocks.eventRepository.deleteEvent).toHaveBeenCalledWith(eventsMocks.events[0].id);
           expect(eventsMocks.notifySrv.success).toHaveBeenCalled();
           expect(eventsMocks.notifySrv.success).toHaveBeenCalledWith('events.successDelete');
       });
   });
});
