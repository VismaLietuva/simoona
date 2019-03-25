var listPaginationMocks = {};

beforeEach(function() {
    listPaginationMocks = {
        onChanged: function() {},
        currentPage: 2,
        pageSize: 10,
        pageCount: 3,
        totalItemCount: 25
    };
});
