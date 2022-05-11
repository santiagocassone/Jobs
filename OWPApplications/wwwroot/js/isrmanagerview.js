$(document).ready(function () {
    $('#cuttOffDate').datepicker({
        todayHighlight: true,
        autoclose: true
    });

    $('#selSalesDirectors').change(function () {
        $('#selSupportManagers option:first').prop('selected', true);
    });

    $('#selSupportManagers').change(function () {
        $('#selSalesDirectors option:first').prop('selected', true);
    });
});