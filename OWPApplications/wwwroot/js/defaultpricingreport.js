$(document).ready(function () {

    var d = new Date();
    var day = d.getDate();
    var month = d.getMonth() - 1;
    var year = d.getFullYear();
    var startDate = new Date(year, month, day);

    $('#dprDateFrom').datepicker({
        todayHighlight: true,
        autoclose: true
    });
    $('#dprDateFrom').datepicker('setDate', localStorage.getItem("inputDateFrom") !== null ? localStorage.getItem("inputDateFrom") : startDate);

    $('#dprDateTo').datepicker({
        todayHighlight: true,
        autoclose: true
    });
    $('#dprDateTo').datepicker('setDate', localStorage.getItem("inputDateTo") !== null ? localStorage.getItem("inputDateTo") : new Date());

    $('#dprform').on('submit', function () {
        ShowLoading();
    });

    $('#spn-tab').click(function () {
        $('#spnSummary').css('visibility', 'visible');
        $('#vndSummary').css('visibility', 'hidden');
    });

    $('#vnd-tab').click(function () {
        $('#spnSummary').css('visibility', 'hidden');
        $('#vndSummary').css('visibility', 'visible');
    });

});