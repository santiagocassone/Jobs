$(document).ready(function () {

    var d = new Date();
    var day = d.getDate();
    var month = d.getMonth() - 2;
    var year = d.getFullYear();
    var startDate = new Date(year, month, day);

    $('#dateFrom').datepicker({
        todayHighlight: true,
        autoclose: true
    });
    $('#dateTo').datepicker({
        todayHighlight: true,
        autoclose: true
    });

    $('#dateFrom').datepicker('setDate', localStorage.getItem("inputCODateFrom") !== null ? localStorage.getItem("inputCODateFrom") : startDate);
    $('#dateTo').datepicker('setDate', localStorage.getItem("inputCODateTo") !== null ? localStorage.getItem("inputCODateTo") : new Date());

    $('.emailTooltip').tooltip({
        boundary: "window",
        container: "body"
    });
});

function SetDates() {
    localStorage.setItem("inputCODateFrom", $('#dateFrom').val());
    localStorage.setItem("inputCODateTo", $('#dateTo').val());
}