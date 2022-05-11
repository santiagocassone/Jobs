$(document).ready(function () {
    $('.autoN').each(function (index, elem) {
        new AutoNumeric(elem);
    });

    $('.autoNCur').each(function (index, elem) {
        new AutoNumeric(elem, AutoNumeric.getPredefinedOptions().dollar);
    });

    $('.autoNPct').each(function (index, elem) {
        new AutoNumeric(elem, AutoNumeric.getPredefinedOptions().percentageUS2dec);
    })

    $('#jobCostingLeadershipTable').DataTable({
        "scrollX": true,
        "paging": true
    });

    $('#fromDate').datepicker({
        defaultDate: new Date(),
        todayHighlight: true,
        autoclose: true
    });

    $('#toDate').datepicker({
        defaultDate: new Date(),
        todayHighlight: true,
        autoclose: true
    });

    $('#jobcostingleadershipform').on('submit', function () {
        ShowLoading();
    });

    $('#export-excel').click(function () {
        $('#export-excel').html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...');
        let open = '';
        if ($(this).data('type') == 'summary') {
            open = '../files/JobCostingLeadershipSummaryReport.xls';
        } else {
            open = '../files/JobCostingLeadershipDetailsReport.xls';
        }
        $.ajax({
            type: "POST",
            url: '/jobcostingleadership/exportexcel',
            data: {
                fromDate: $('#fromDate').val(),
                toDate: $('#toDate').val(),
                projectid: $('#projectid').val(),
                customers: $('#customerid').val(),
                region: $('#region').val(),
                type: $(this).data('type')
            }
        }).done(function (result) {
            if (result === "success") {
                window.open(open, '_blank');
            } else if (result === "empty") {
                alert("There is no data for the current search. Please change the filters and try again. ");
            } else {
                alert(result);
            }
            $('#export-excel').html('Download Excel');
        }).fail(function (error) {
            alert(error);
            $('#export-excel').html('Download Excel');
        });

    });

});