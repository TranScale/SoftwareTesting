(function () {
  function safeInitDataTables() {
    if (!window.jQuery || !jQuery.fn || !jQuery.fn.DataTable) return;
    jQuery('.datatable').each(function () {
      if (jQuery.fn.dataTable.isDataTable(this)) return;
      jQuery(this).DataTable({
        paging: false,
        info: false,
        searching: false,
        ordering: true,
        autoWidth: false
      });
    });
  }

  function init() {
    if (window.jQuery) {
      jQuery(function () {
        // Bootstrap 4 tooltip
        if (jQuery.fn.tooltip) jQuery('[data-toggle="tooltip"]').tooltip();
        safeInitDataTables();
      });
    }
  }

  init();
})();
