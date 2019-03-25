describe('itemFilterPanelFactory', function() {
    var service = {};

    beforeEach(module('simoonaApp.Common'));
    beforeEach(function() {
        module(function($provide) {
            $provide.value('lodash', itemFilterPanelMocks.lodash);
        });
    });
    beforeEach(inject(function(_itemFilterPanelFactory_) {
        service = _itemFilterPanelFactory_;
    }));

    it('itemFilterPanelFactory should be defined', function() {
        expect(service).toBeDefined();
    });

    it('should return filter parameters from filters when executing filter', function() {
        var filterParameters = service.executeFilter(itemFilterPanelMocks.executeFilters);
        expect(filterParameters).toEqual(itemFilterPanelMocks.expectedResultExecuteFilter);
    });

});
