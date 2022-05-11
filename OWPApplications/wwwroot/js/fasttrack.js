$(document).ready(function () {

    $("#fasttrackTable").tablesorter();

    $('#fasttrack-tab').click(function () {
        $('#fastTrackSummary').css('visibility', 'visible');
        $('#expWHRecGraph').css('visibility', 'hidden');
    });

    $('#expwarhousereceipt-tab').click(function () {
        $('#fastTrackSummary').css('visibility', 'hidden');
        $('#expWHRecGraph').css('visibility', 'visible');
    });

    $('.graph').click(function () {
        var dateFrom = $(this).data('datefrom');
        var dateTo = $(this).data('dateto');
        ShowWeekInfo(dateFrom, dateTo);
    });
});

function ShowWeekInfo(dateFrom, dateTo) {
    var d = new Date(dateFrom);
    var day = d.getDate(d);
    var month = d.getMonth(d) + 1;
    var title = "Details for the week of " + month + "/" + day;
    var lines = GetWeekInfoDetails(dateFrom, dateTo);
    $('#weekInfoModalTitle').html(title);
    $('#weekInfoTableDetails > tbody').html(templateWeekInfoDetails(lines));
    $('#weekInfoModal').modal('show');
}

function GetWeekInfoDetails(dateFrom, dateTo) {
    var ret;
    $.ajax({
        type: "GET",
        url: '/home/GetWeekInfoDetails?dateFrom=' + dateFrom + '&dateTo=' + dateTo,
        async: false,
        dataType: 'json'
    }).done(function (result) {
        ret = result;
    });

    return ret;
}

function templateWeekInfoDetails(data) {
    var html = '';
    for (var i = 0; i < data.length; i++) {
        var row = data[i];
        html +=
            '<tr>'
            + '<td>' + getFormattedDate(row.expectedReceiptDate) + '</td>'
            + '<td>' + row.orderNo + '</td>'
            + '<td>' + row.poSuffix + '</td>'
            + '<td>' + row.vndNo + '</td>'
            + '<td>' + row.vndName + '</td>'
            + '<td>' + row.salespersonName + '</td>'
            + '<td>' + row.customerName + '</td>'
            + '<td>' + row.lineNo + '</td>'
            + '<td>' + row.catalogNo + '</td>'
            + '<td>' + row.description + '</td>'
            + '<td>' + row.qtyOrdered + '</td>'
            + '<td>' + row.qtyReceived + '</td>'
            + '<td>' + row.expectedQty + '</td>'
            + '<td>' + row.ackNo + '</td>'
            + '<td style="color:' + row.schDateColor + '">' + getFormattedDate(row.schDate) + '</td>'
            + '</tr>';
    }

    return html;
}

function getFormattedDate(date) {
    var d = new Date(date);
    var day = d.getDate(d);
    var month = d.getMonth(d) + 1;
    var year = d.getFullYear();
    return month + "/" + day + "/" + year;
}