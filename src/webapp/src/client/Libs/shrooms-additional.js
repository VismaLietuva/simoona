$(document).on('click', '.nc-table-active tbody tr', function(e) {
    var target = $(e.target);

    if (!target.is('a')) {
        $('.nc-table-active tbody tr').removeClass('active');
        $(this).addClass('active');
    }
});
