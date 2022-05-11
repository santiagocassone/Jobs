$(document).ready(function () {

    var d = new Date();
    var day = 1;
    var month = d.getMonth();
    var year = d.getFullYear();
    var startDate = new Date(year, month, day);

    $('#dateFromStatsReport').datepicker({
        todayHighlight: true,
        autoclose: true
    });
    $('#dateToStatsReport').datepicker({
        todayHighlight: true,
        autoclose: true
    });

    if ($('#isPostBack').val()) {
        $('#dateFromStatsReport').datepicker('setDate', localStorage.getItem("inputSRDateFrom") !== null ? localStorage.getItem("inputSRDateFrom") : startDate);
        $('#dateToStatsReport').datepicker('setDate', localStorage.getItem("inputSRDateTo") !== null ? localStorage.getItem("inputSRDateTo") : new Date());
    }
    else {
        $('#dateFromStatsReport').datepicker('setDate', startDate);
        $('#dateToStatsReport').datepicker('setDate', new Date());
    }

    $('#estimatorsSelect').selectpicker('selectAll');

});