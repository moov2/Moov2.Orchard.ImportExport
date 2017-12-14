$(document).ready(function() {
    var $bulkItems = $('.bulk-items');
    $('.js-contentexport-select-all').on('click', function(e) {
        e.preventDefault();
        $bulkItems.find('input[type="checkbox"]').prop('checked', true);
    });

    $('.js-contentexport-select-none').on('click', function(e) {
        e.preventDefault();
        $bulkItems.find('input[type="checkbox"]').prop('checked', false);
    });
});